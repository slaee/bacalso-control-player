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

    public int ActionDelay { get; set; } = 700;
    public int ExitCombatDelay { get; set; } = 1600;
    public string SoloClass { get; set; } = "Generic";
    public ClassUseMode SoloUseMode { get; set; } = ClassUseMode.Base;
    public bool SoloGearOn { get; set; } = true;
    public string[] SoloGear { get; set; } = { "Weapon", "Headpiece", "Cape" };
    public string FarmClass { get; set; } = "Generic";
    public ClassUseMode FarmUseMode { get; set; } = ClassUseMode.Base;
    public bool FarmGearOn { get; set; } = true;
    public string[] FarmGear { get; set; } = { "Weapon", "Headpiece", "Cape" };

    public void HuntForItem(string item, int quantity)
    {
        this.Logger($"Hunting for {item} x{quantity}");
        string [] items = { item };

        this.EquipClass(ClassType.Farm);
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

    ClassType currentClass = ClassType.None;
    bool usingSoloGeneric = false;
    bool usingFarmGeneric = false;

    private bool logEquip = true;
   
    public void EquipClass(ClassType classToUse)
    {
        if (currentClass == classToUse && Bot.Skills.TimerRunning)
            return;

        switch (classToUse)
        {
            case ClassType.Farm:
                if (!usingFarmGeneric)
                {
                    if (FarmGearOn & Bot.Player.CurrentClass?.Name != FarmClass)
                    {
                        logEquip = false;
                        Bot.Sleep(ActionDelay);
                        Equip(FarmGear);
                        logEquip = true;
                    }

                    int? class_id = Bot.Inventory.Items.Find(i => i.Name.ToLower().Trim() == FarmClass.ToLower().Trim() && i.Category == ItemCategory.Class)?.ID;
                    if (class_id == null)
                        Logger("Class not found", stopBot: true);
                    Bot.Wait.ForItemEquip(class_id ?? 0);
                    Bot.Skills.StartAdvanced(FarmClass, true, FarmUseMode);
                    break;
                }
                Bot.Skills.StartAdvanced(Bot.Player.CurrentClass?.Name ?? "generic", false);
                break;
            default:
                if (!usingSoloGeneric)
                {
                    if (SoloGearOn & Bot.Player.CurrentClass?.Name != SoloClass)
                    {
                        logEquip = false;
                        Bot.Sleep(ActionDelay);
                        Equip(SoloGear);
                        logEquip = true;
                    }
                    int? class_id = Bot.Inventory.Items.Find(i => i.Name.ToLower().Trim() == SoloClass.ToLower().Trim() && i.Category == ItemCategory.Class)?.ID;
                    if (class_id == null)
                        Logger("Class not found", stopBot: true);
                    Bot.Wait.ForItemEquip(class_id ?? 0);

                    Bot.Skills.StartAdvanced(SoloClass, true, SoloUseMode);
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

        JumpWait();

        foreach (string Item in gear)
        {
            if ((Item != "Weapon" && Item != "Headpiece" && Item != "Cape" && Item != "False") && CheckInventory(Item) && !Bot.Inventory.IsEquipped(Item))
            {
                Bot.Inventory.EquipItem(Item);
                Bot.Sleep(ActionDelay);
                if (logEquip)
                    Logger($"Equipped {Item}");
            }
        }
    }

    public void JumpWait()
    {
        if (!Bot.Player.InCombat)
            return;

        List<string> MonsterCells = Bot.Monsters.MapMonsters.Select(monster => monster.Cell).ToList();

        if (!MonsterCells.Contains(Bot.Player.Cell))
            return;
        string[] blankCells = new[] { "wait", "blank" };
        string cell = string.Empty;
        string pad = string.Empty;
        bool jumpTwice = false;

        if (!MonsterCells.Contains("Enter"))
        {
            cell = "Enter";
            pad = "Spawn";
        }
        else
        {
            foreach (string _cell in Bot.Map.Cells)
            {
                if (_cell == Bot.Player.Cell || blankCells.Contains(_cell.ToLower()) || _cell.ToLower().Contains("cut"))
                    continue;
                if (!MonsterCells.Contains(cell))
                {
                    cell = _cell;
                    pad = "Left";
                    break;
                }
            }
        }

        if (string.IsNullOrEmpty(cell) || string.IsNullOrEmpty(pad))
        {
            cell = Bot.Player.Cell;
            pad = Bot.Player.Pad;
            jumpTwice = true;
        }

        if (lastJumpWait != $"{Bot.Map.Name} | {cell} | {pad}" || Bot.Player.InCombat)
        {
            JumpRoomCell(cell, pad);
            if (jumpTwice)
                JumpRoomCell(cell, pad);

            lastJumpWait = $"{Bot.Map.Name} | {cell} | {pad}";

            Bot.Sleep(ExitCombatDelay < 200 ? ExitCombatDelay : ExitCombatDelay - 200);
            Bot.Wait.ForCombatExit();
        }
        Bot.Combat.Exit();
        Bot.Wait.ForCombatExit();
    }
    private string lastJumpWait = "";
}

public enum ClassType
{
    Solo,
    Farm,
    None
}