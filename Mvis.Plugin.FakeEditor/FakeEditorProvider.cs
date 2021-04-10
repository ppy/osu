using osu.Game.Screens.Mvis.Plugins;

namespace Mvis.Plugin.FakeEditor
{
    public class RulesetPanelProvider : MvisPluginProvider
    {
        public override MvisPlugin CreatePlugin => new Plugin.FakeEditor.FakeEditor();
    }
}
