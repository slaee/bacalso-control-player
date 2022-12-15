//cs_include Scripts/BacalsoControlPlayer/BotPlayer.cs
//cs_include Scripts/BacalsoControlPlayer/Utils/Logger.cs
//cs_include Scripts/BacalsoControlPlayer/Utils/Map.cs
//cs_include Scripts/BacalsoControlPlayer/Enums/ClassType.cs

using Skua.Core.Interfaces;

public class BotWithFuzzyLogic
{
    private IScriptInterface Bot => IScriptInterface.Instance;
    private BotPlayer player => BotPlayer.Instance;

    public void ScriptMain(IScriptInterface Bot)
    {
        player.HuntForItem("firewar", "Treasure Chest", 1);
    }
}
