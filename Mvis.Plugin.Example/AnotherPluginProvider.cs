using osu.Game.Screens.LLin.Plugins;

namespace Mvis.Plugin.Example
{
    public class AnotherPanelProvider : LLinPluginProvider
    {
        public override LLinPlugin CreatePlugin => new AnotherPlugin();
    }
}
