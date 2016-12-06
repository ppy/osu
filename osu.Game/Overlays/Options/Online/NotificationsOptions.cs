//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Options.Online
{
    public class NotificationsOptions : OptionsSubsection
    {
        protected override string Header => "Notifications";
        
        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new CheckBoxOption
                {
                    LabelText = "Enable chat ticker",
                    Bindable = config.GetBindable<bool>(OsuConfig.Ticker)
                },
                new CheckBoxOption
                {
                    LabelText = "Show a notification popup when someone says your name",
                    Bindable = config.GetBindable<bool>(OsuConfig.ChatHighlightName)
                },
                new CheckBoxOption
                {
                    LabelText = "Show chat message notifications",
                    Bindable = config.GetBindable<bool>(OsuConfig.ChatMessageNotification)
                },
                new CheckBoxOption
                {
                    LabelText = "Play a sound when someone says your name",
                    Bindable = config.GetBindable<bool>(OsuConfig.ChatAudibleHighlight)
                },
                new CheckBoxOption
                {
                    LabelText = "Show notification popups instantly during gameplay",
                    Bindable = config.GetBindable<bool>(OsuConfig.PopupDuringGameplay)
                },
                new CheckBoxOption
                {
                    LabelText = "Show notification popups when friends change status",
                    Bindable = config.GetBindable<bool>(OsuConfig.NotifyFriends)
                },
            };
        }
    }
}