using osu.Game.Screens.LLin.Plugins;

namespace Mvis.Plugin.StoryboardSupport
{
    public class StoryboardPluginProvider : LLinPluginProvider
    {
        public override LLinPlugin CreatePlugin => new BackgroundStoryBoardLoader();
    }
}
