using osu.Game.Screens.LLin.Plugins;

namespace Mvis.Plugin.BottomBar
{
    public class BottomBarProvider : LLinPluginProvider
    {
        public override LLinPlugin CreatePlugin => new LegacyBottomBar();
    }
}
