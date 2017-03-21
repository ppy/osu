﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Configuration;

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
                new OptionCheckbox
                {
                    LabelText = "Filter offensive words",
                    Bindable = config.GetBindable<bool>(OsuConfig.ChatFilter)
                },
                new OptionCheckbox
                {
                    LabelText = "Filter foreign characters",
                    Bindable = config.GetBindable<bool>(OsuConfig.ChatRemoveForeign)
                },
                new OptionCheckbox
                {
                    LabelText = "Log private messages",
                    Bindable = config.GetBindable<bool>(OsuConfig.LogPrivateMessages)
                },
                new OptionCheckbox
                {
                    LabelText = "Block private messages from non-friends",
                    Bindable = config.GetBindable<bool>(OsuConfig.BlockNonFriendPM)
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