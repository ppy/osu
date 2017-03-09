// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Options.Sections.Online
{
    public class NotificationsOptions : OptionsSubsection
    {
        protected override string Header => "Notifications";
        
        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new OptionCheckbox
                {
                    LabelText = "Enable chat ticker",
                    Bindable = config.GetBindable<bool>(OsuConfig.Ticker)
                },
                new OptionCheckbox
                {
                    LabelText = "Show a notification popup when someone says your name",
                    Bindable = config.GetBindable<bool>(OsuConfig.ChatHighlightName)
                },
                new OptionCheckbox
                {
                    LabelText = "Show chat message notifications",
                    Bindable = config.GetBindable<bool>(OsuConfig.ChatMessageNotification)
                },
                new OptionCheckbox
                {
                    LabelText = "Play a sound when someone says your name",
                    Bindable = config.GetBindable<bool>(OsuConfig.ChatAudibleHighlight)
                },
                new OptionCheckbox
                {
                    LabelText = "Show notification popups instantly during gameplay",
                    Bindable = config.GetBindable<bool>(OsuConfig.PopupDuringGameplay)
                },
                new OptionCheckbox
                {
                    LabelText = "Show notification popups when friends change status",
                    Bindable = config.GetBindable<bool>(OsuConfig.NotifyFriends)
                },
            };
        }
    }
}