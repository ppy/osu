using osu.Game.Screens.LLin.Plugins;

namespace Mvis.Plugin.FakeEditor
{
    public class RulesetPanelProvider : LLinPluginProvider
    {
        public override LLinPlugin CreatePlugin => new Plugin.FakeEditor.FakeEditor();
    }
}
