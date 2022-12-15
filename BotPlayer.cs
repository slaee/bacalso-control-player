using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using Newtonsoft.Json;
using Skua.Core.Interfaces;
using Skua.Core.Models;
using Skua.Core.Models.Items;
using Skua.Core.Models.Monsters;
using Skua.Core.Models.Quests;
using Skua.Core.Models.Servers;
using Skua.Core.Models.Shops;
using Skua.Core.Models.Skills;
using Skua.Core.Options;
using Skua.Core.Utils;

public class BotPlayer
{
    private IScriptInterface Bot => IScriptInterface.Instance;

    private static BotPlayer _instance;
    public static BotPlayer Instance => _instance ??= new BotPlayer();

    public void HuntForItem(string item, int quantity)
    {
        this.Logger($"Hunting for {item} x{quantity}");
        string [] items = { item };
        while (!Bot.ShouldExit && !CheckInventory(items, quantity))
        {
            this.KillMonster("celestialpast", "r2", "Left", "Blessed Deer", item, quantity);
            this.KillMonster("celestialpast", "r3", "Left", "Blessed Deer", item, quantity);
        }
    }

    public void KillMonster(string map, string cell, string pad, string monster, string? item = null, int quantity = 1, bool log = true)
    {
        if(item != null && CheckInventory(item, quantity))
            return;

        this.JoinMap(map, cell, pad);
        this.JumpRoomCell(cell, pad);

        if (item == null)
        {
            if (log)
                Logger($"Killing {monster}");
            Bot.Kill.Monster(monster);
            // TODO: Player Conditions using fuzzy logic

            return;
        }

        if (log)
        {
            int dynamicQuantity = Bot.Inventory.GetQuantity(item);
            Logger($"Killing {monster} for {item}, ({dynamicQuantity}/{quantity}) [Inventory = {item}]");
        }

        while (!Bot.ShouldExit && !CheckInventory(item, quantity))
        {
            if (!Bot.Combat.StopAttacking)
                Bot.Combat.Attack(monster);
            Bot.Sleep(1000);
        }
    }

    public void JoinMap(string map, string cell = "Enter", string pad = "Spawn")
    {
        string mapName = map.Contains('-') ? map.Split('-').First() : map;
        bool hasMapNumber = map.Contains('-') 
            && Int32.TryParse(map.Split('-').Last(), out int result) 
            && (result >= 1000);

        if(hasMapNumber)
            Bot.Map.Join(map, cell, pad, false);
        else 
            Bot.Map.Join($"{map}-2022", cell, pad, true);

        Bot.Wait.ForMapLoad(mapName);            
    }

    public void JumpRoomCell(string cell="Enter", string pad="Spawn")
    {
        Bot.Map.Jump(cell, pad);
        Bot.Sleep(200);
    }

    public bool CheckInventory(string[] itemNames, int quantity, bool any = false, bool toInv = true)
    {
        if (itemNames == null)
            return true;

        foreach (string name in itemNames)
        {
            if (CheckInventory(name, quantity, toInv))
            {
                if (any)
                    return true;
                else
                    continue;
            }

            if (!any)
                return false;
        }

        return !any;
    }

    public bool CheckInventory(string item, int quant = 1, bool toInv = true)
    {
        if (Bot.TempInv.Contains(item, quant))
            return true;

        if (Bot.Inventory.Contains(item, quant))
            return true;

        if (Bot.Bank.Contains(item))
        {
            if ((toInv && Bot.Inventory.GetQuantity(item) >= quant) ||
               (!toInv && Bot.Bank.TryGetItem(item, out InventoryItem? _item) && _item != null && _item.Quantity >= quant))
                return true;
        }

        if (Bot.House.Contains(item))
            return true;

        return false;
    }

    public void Logger(string message = "", [CallerMemberName] string caller = "", bool messageBox = false, bool stopBot = false)
    {
        Bot.Log($"[{DateTime.Now:HH:mm:ss}] ({caller})  {message}");
    }
}