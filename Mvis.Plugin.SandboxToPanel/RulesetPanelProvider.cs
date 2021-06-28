using osu.Game.Screens.Mvis.Plugins;

namespace Mvis.Plugin.RulesetPanel
{
    public class RulesetPanelProvider : MvisPluginProvider
    {
        public override MvisPlugin CreatePlugin => new Sandbox.RulesetPanel();
    }
}
