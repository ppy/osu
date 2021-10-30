using osu.Framework.Platform;
using osu.Game.Screens.LLin.Plugins.Config;

namespace Mvis.Plugin.StoryboardSupport.Config
{
    public class SbLoaderConfigManager : PluginConfigManager<SbLoaderSettings>
    {
        public SbLoaderConfigManager(Storage storage)
            : base(storage)
        {
        }

        protected override void InitialiseDefaults()
        {
            SetDefault(SbLoaderSettings.EnableStoryboard, true);
        }

        protected override string ConfigName => "StoryboardSupport";
    }

    public enum SbLoaderSettings
    {
        EnableStoryboard
    }
}
