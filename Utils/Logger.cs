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
    public class Logger
    {
        private IScriptInterface Bot => IScriptInterface.Instance;

        private  Logger _instance;

        public Logger Instance => _instance ??= new Logger();

        public void Message(string message, [CallerMemberName] string caller = "", bool messageBox = false, bool stopBot = false)
        {
            Bot.Log($"[{DateTime.Now:HH:mm:ss}] ({caller})  {message}");
        }
    }
}





