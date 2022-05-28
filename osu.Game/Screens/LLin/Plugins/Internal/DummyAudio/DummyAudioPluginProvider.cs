using osu.Game.Configuration;

namespace osu.Game.Screens.LLin.Plugins.Internal.DummyAudio
{
    internal class DummyAudioPluginProvider : LLinPluginProvider
    {
        private readonly MConfigManager config;
        private readonly LLinPluginManager plmgr;

        internal DummyAudioPluginProvider(MConfigManager config, LLinPluginManager plmgr)
        {
            this.config = config;
            this.plmgr = plmgr;
        }

        public override LLinPlugin CreatePlugin => new DummyAudioPlugin(config, plmgr);
    }
}
