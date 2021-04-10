using osu.Game.Screens.Mvis.Plugins;

namespace Mvis.Plugin.StoryboardSupport
{
    public class RulesetPanelProvider : MvisPluginProvider
    {
        public override MvisPlugin CreatePlugin => new BackgroundStoryBoardLoader();
    }
}
