//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
        private TextBoxOption chatIgnoreList;
        private TextBoxOption chatHighlightWords;
        protected override string Header => "In-game Chat";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
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
                chatIgnoreList = new TextBoxOption {
                    Height = 20,
                    RelativeSizeAxes = Axes.X,
                    Bindable = config.GetBindable<string>(OsuConfig.IgnoreList)
                },
                new SpriteText { Text = "Chat highlight words (space-seperated list)" },
                chatHighlightWords = new TextBoxOption {
                    Height = 20,
                    RelativeSizeAxes = Axes.X,
                    Bindable = config.GetBindable<string>(OsuConfig.HighlightWords)
                },
            };
        }
    }
}