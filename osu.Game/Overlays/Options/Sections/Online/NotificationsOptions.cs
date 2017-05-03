// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;

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
                new OsuCheckbox
                {
                    LabelText = "Enable chat ticker",
                    Bindable = config.GetBindable<bool>(OsuConfig.Ticker),
                    TooltipText = "Popup new chat messages in a one-liner display at the bottom of the screen when the console is not visible."
                },
                new OsuCheckbox
                {
                    LabelText = "Show a notification popup when someone says your name",
                    Bindable = config.GetBindable<bool>(OsuConfig.ChatHighlightName),
                    TooltipText = "A message will flash at the bottom of your screen (even during gameplay) when someone mentions your name in chat."
                },
                new OsuCheckbox
                {
                    LabelText = "Show chat message notifications",
                    Bindable = config.GetBindable<bool>(OsuConfig.ChatMessageNotification),
                    TooltipText = "A notification will be shown when new chat messages arrive."
                },
                new OsuCheckbox
                {
                    LabelText = "Play a sound when someone says your name",
                    Bindable = config.GetBindable<bool>(OsuConfig.ChatAudibleHighlight),
                    TooltipText = "A sound will play when someone mentions your name in chat."
                },
                new OsuCheckbox
                {
                    LabelText = "Show notification popups instantly during gameplay",
                    Bindable = config.GetBindable<bool>(OsuConfig.PopupDuringGameplay),
                    TooltipText = "Choose whether to allow notifications to appear in the notification manager during gameplay."
                },
                new OsuCheckbox
                {
                    LabelText = "Show notification popups when friends change status",
                    Bindable = config.GetBindable<bool>(OsuConfig.NotifyFriends),
                    TooltipText = "Notifications will be shown when friends go online/offline."
                },
            };
        }
    }
}