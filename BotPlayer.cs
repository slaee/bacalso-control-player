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

namespace BacalsoBOt
{
    public class BotPlayer
    {
        private IScriptInterface Bot => IScriptInterface.Instance;

        public void HuntForItem(string item, int quantity)
        {
            while (!Bot.ShouldExit && CheckInventory(item, quantity))
            {
                this.KillMonster("celestialpast", "r2", "Left", "Blessed Deer", "Treasure Chest");
                this.KillMonster("celestialpast", "r3", "Left", "Blessed Deer", "Treasure Chest");
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
                Bot.Map.Join("${map}-2022", cell, pad, true);

            Bot.Wait.ForMapLoad(mapName);            
        }

        public void JumpRoomCell(string cell="Enter", string pad="Spawn")
        {
            Bot.Map.Jump(cell, pad);
            Bot.Sleep(200);
        }

        public bool CheckInventory(int[] itemNames, int quantity, bool any = false, bool toInv = true)
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
    }
}