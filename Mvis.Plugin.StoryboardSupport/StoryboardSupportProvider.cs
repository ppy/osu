using osu.Game.Screens.LLin.Plugins;

namespace Mvis.Plugin.StoryboardSupport
{
    public class RulesetPanelProvider : LLinPluginProvider
    {
        public override LLinPlugin CreatePlugin => new BackgroundStoryBoardLoader();
    }
}
