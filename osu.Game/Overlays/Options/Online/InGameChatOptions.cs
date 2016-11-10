using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Options.Online
{
    public class InGameChatOptions : OptionsSubsection
    {
        protected override string Header => "In-game Chat";

        [Initializer]
        private void Load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new CheckBoxOption
                {
                    LabelText = "Filter offensive words",
                    Bindable = config.GetBindable<bool>(OsuConfig.ChatFilter)
                },
                new CheckBoxOption
                {
                    LabelText = "Filter foreign characters",
                    Bindable = config.GetBindable<bool>(OsuConfig.ChatRemoveForeign)
                },
                new CheckBoxOption
                {
                    LabelText = "Log private messages",
                    Bindable = config.GetBindable<bool>(OsuConfig.LogPrivateMessages)
                },
                new CheckBoxOption
                {
                    LabelText = "Block private messages from non-friends",
                    Bindable = config.GetBindable<bool>(OsuConfig.BlockNonFriendPM)
                },
                new SpriteText { Text = "Chat ignore list (space-seperated list)" },
                new TextBox { Height = 20, RelativeSizeAxes = Axes.X },
                new SpriteText { Text = "Chat highlight words (space-seperated list)" },
                new TextBox { Height = 20, RelativeSizeAxes = Axes.X },
            };
        }
    }
}