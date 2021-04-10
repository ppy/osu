using osu.Game.Screens.Mvis.Plugins;

namespace Mvis.Plugin.Example
{
    public class AnotherPanelProvider : MvisPluginProvider
    {
        public override MvisPlugin CreatePlugin => new AnotherPlugin();
    }
}
