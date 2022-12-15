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
            throw new NotImplementedException();
        }

        public void KillMonster(string map, string cell, string monster, string? item = null, int quantity = 1)
        {
            throw new NotImplementedException();
        }
    }
}