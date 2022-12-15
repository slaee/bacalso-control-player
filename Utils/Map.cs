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

namespace BacalsoControlPlayer.Utils
{
    public class Map
    {
        private IScriptInterface Bot => IScriptInterface.Instance;
        private  Map _instance;
        public Map Instance => _instance ??= new Map();

        private string lastJumpWait = "";
        public int ActionDelay { get; set; } = 700;
        public int ExitCombatDelay { get; set; } = 1600;

        public void Join(string map, string cell = "Enter", string pad = "Spawn")
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
    }
}