// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Options.Sections.Online
{
    public class InGameChatOptions : OptionsSubsection
    {
        protected override string Header => "Chat";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new OsuCheckbox
                {
                    LabelText = "Filter offensive words",
                    Bindable = config.GetBindable<bool>(OsuConfig.ChatFilter),
                    TooltipText = "Attempts to remove words that may be offensive to specific cultures and younger age groups."
                },
                new OsuCheckbox
                {
                    LabelText = "Filter foreign characters",
                    Bindable = config.GetBindable<bool>(OsuConfig.ChatRemoveForeign),
                    TooltipText = "Removes any characters outside the standart ASCII (English) range. Useful if you experience lag from these"
                },
                new OsuCheckbox
                {
                    LabelText = "Log private messages",
                    Bindable = config.GetBindable<bool>(OsuConfig.LogPrivateMessages),
                    TooltipText = "Enabling this option will automatically log all private messages sent and received to the Chat folder in \"(user).txt\" format."
                },
                new OsuCheckbox
                {
                    LabelText = "Block private messages from non-friends",
                    Bindable = config.GetBindable<bool>(OsuConfig.BlockNonFriendPM),
                    TooltipText = "You will not receive any private messages from users who are not on your friends list if this option is enabled."
                },
                new OptionLabel { Text = "Chat ignore list (space-seperated list)" },
                new OptionTextBox {
                    RelativeSizeAxes = Axes.X,
                    Bindable = config.GetBindable<string>(OsuConfig.IgnoreList)
                },
                new OptionLabel { Text = "Chat highlight words (space-seperated list)" },
                new OptionTextBox {
                    RelativeSizeAxes = Axes.X,
                    Bindable = config.GetBindable<string>(OsuConfig.HighlightWords)
                },
            };
        }
    }
}