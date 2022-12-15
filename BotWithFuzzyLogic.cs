//cs_include Scripts/bacalso-control-player/BotPlayer.cs
using Skua.Core.Interfaces;

public class BotWithFuzzyLogic
{
    private IScriptInterface Bot => IScriptInterface.Instance;
    private BotPlayer player => BotPlayer.Instance;

    public void ScriptMain(IScriptInterface Bot)
    {
        player.HuntForItem("Treasure Chest", 1);
    }
}
