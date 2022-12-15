//cs_include Scripts/BacalsoControlPlayer/Utils/Logger.cs
//cs_include Scripts/BacalsoControlPlayer/Utils/Map.cs
//cs_include Scripts/BacalsoControlPlayer/Enums/ClassType.cs

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

using BacalsoControlPlayer.Utils;
using BacalsoControlPlayer.Enums;

public class BotPlayer
{
    private IScriptInterface Bot => IScriptInterface.Instance;
    private static BotPlayer _instance;
    public static BotPlayer Instance => _instance ??= new BotPlayer();

    private Map Map = new();
    private Logger Log = new();
    private bool logEquip = true;

    public int ActionDelay { get; set; } = 700;
    public int ExitCombatDelay { get; set; } = 1600;
    private ClassType currentClass = ClassType.None;
    public ClassUseMode SoloUseMode { get; set; } = ClassUseMode.Base;
    public ClassUseMode FarmUseMode { get; set; } = ClassUseMode.Base;
    public string SoloClass { get; set; } = "Generic";
    public bool SoloGearOn { get; set; } = true;
    public string[] SoloGear { get; set; } = { "Weapon", "Headpiece", "Cape" };
    public string FarmClass { get; set; } = "Generic";
    public bool FarmGearOn { get; set; } = true;
    public string[] FarmGear { get; set; } = { "Weapon", "Headpiece", "Cape" };

    public void HuntForItem(string item, int quantity)
    {
        Log.Message($"Hunting for {item} x{quantity}");

        this.EquipClass(ClassType.Farm);
        while (!Bot.ShouldExit && !CheckInventory(new string[] {item},quantity))
        {
            this.KillMonster("celestialpast", "r2", "Left", "Blessed Deer", item, quantity);
            this.KillMonster("celestialpast", "r3", "Left", "Blessed Deer", item, quantity);
        }
    }

    public void KillMonster(string map, string cell, string pad, string monster, string? item = null, int quantity = 1, bool log = true)
    {
        if(item != null && CheckInventory(item, quantity))
            return;

        Map.Join(map, cell, pad);
        Map.JumpRoomCell(cell, pad);
        if (item == null)
        {
            if (log) Log.Message($"Killing {monster}");
            Bot.Kill.Monster(monster);
            // TODO: Player Conditions using fuzzy logic

            return;
        }
        
        _KillMonsterForItem(monster, item, quantity, log);
    }

    private void _KillMonsterForItem(string monster, string item, int quantity, bool log)
    {
        if (log)
        {
            int dynamicQuantity = Bot.Inventory.GetQuantity(item);
            Log.Message($"Killing {monster} for {item}, ({dynamicQuantity}/{quantity}) [Inventory = {item}]");
        }

        while (!Bot.ShouldExit && !CheckInventory(item, quantity))
        {
            if (!Bot.Combat.StopAttacking)
                Bot.Combat.Attack(monster);
            Bot.Sleep(ActionDelay);
        }
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

    public void EquipClass(ClassType classToUse)
    {
        bool usingSoloGeneric = false;
        bool usingFarmGeneric = false;

        if (this.currentClass == classToUse && Bot.Skills.TimerRunning)
            return;

        switch (classToUse)
        {
            case ClassType.Farm:
                if (!usingFarmGeneric)
                {
                    if (this.FarmGearOn & Bot.Player.CurrentClass?.Name != this.FarmClass)
                    {
                        this.logEquip = false;
                        Bot.Sleep(ActionDelay);
                        this.Equip(this.FarmGear);
                        this.logEquip = true;
                    }

                    int? class_id = (Bot.Inventory.Items.Find(i => i.Name.ToLower().Trim() == FarmClass.ToLower().Trim() 
                        && i.Category == ItemCategory.Class)?.ID);
                    Bot.Wait.ForItemEquip(class_id ?? 0);
                    Bot.Skills.StartAdvanced(this.FarmClass, true, this.FarmUseMode);
                    break;
                }
                Bot.Skills.StartAdvanced(Bot.Player.CurrentClass?.Name ?? "generic", false);
                break;

            default:
                if (!usingSoloGeneric)
                {
                    if (this.SoloGearOn & Bot.Player.CurrentClass?.Name != this.SoloClass)
                    {
                        this.logEquip = false;
                        Bot.Sleep(ActionDelay);
                        this.Equip(this.SoloGear);
                        this.logEquip = true;
                    }
                    int? class_id = (Bot.Inventory.Items.Find(i => i.Name.ToLower().Trim() == SoloClass.ToLower().Trim()
                         && i.Category == ItemCategory.Class)?.ID);
                    Bot.Wait.ForItemEquip(class_id ?? 0);
                    Bot.Skills.StartAdvanced(this.SoloClass, true, this.SoloUseMode);
                    break;
                }

                Bot.Skills.StartAdvanced(Bot.Player.CurrentClass?.Name ?? "generic", false);
                break;
        }

        currentClass = classToUse;
    }

    public void Equip(params string[] gear)
    {
        if (gear == null)
            return;

        Map.JumpWait();
        foreach (string item in gear)
        {
            if ((item != "Weapon" && item != "Headpiece" && item != "Cape" && item != "False") 
                && CheckInventory(item) && !Bot.Inventory.IsEquipped(item))
            {
                Bot.Inventory.EquipItem(item);
                Bot.Sleep(ActionDelay);
                if (this.logEquip)
                    Log.Message($"Equipped {item}");
            }
        }
    }
}