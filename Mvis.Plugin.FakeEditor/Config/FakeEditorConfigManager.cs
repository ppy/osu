using osu.Framework.Platform;
using osu.Game.Screens.LLin.Plugins.Config;

namespace Mvis.Plugin.FakeEditor.Config
{
    public class FakeEditorConfigManager : PluginConfigManager<FakeEditorSetting>
    {
        public FakeEditorConfigManager(Storage storage)
            : base(storage)
        {
        }

        protected override void InitialiseDefaults()
        {
            SetDefault(FakeEditorSetting.EnableFakeEditor, false);
        }

        protected override string ConfigName => "FakeEditor";
    }

    public enum FakeEditorSetting
    {
        EnableFakeEditor,
    }
}
