using osu.Game.Screens.Mvis.Plugins;

namespace Mvis.Plugin.BottomBar
{
    public class BottomBarProvider : MvisPluginProvider
    {
        public override MvisPlugin CreatePlugin => new LegacyBottomBar();
    }
}
