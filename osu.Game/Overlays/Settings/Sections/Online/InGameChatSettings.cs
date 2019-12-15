using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Settings.Sections.Online
{
    public class InGameChatSettings : SettingsSubsection
    {
        protected override string Header => "In-Game Chat";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsTextBox
                {
                    LabelText = "Chat ignore list (space-separated list)",
                    Bindable = config.GetBindable<string>(OsuSetting.IgnoreList)
                },
                new SettingsTextBox
                {
                    LabelText = "Chat highlight words (space-separated list)",
                    Bindable = config.GetBindable<string>(OsuSetting.HighlightWords)
                },
            };
        }
    }
}
