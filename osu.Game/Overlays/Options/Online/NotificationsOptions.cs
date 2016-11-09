using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Options.Online
{
    public class NotificationsOptions : OptionsSubsection
    {
        protected override string Header => "Notifications";
        
        private CheckBoxOption chatTicker, notifyMention, notifyChat, audibleNotification,
            notificationsDuringGameplay, notifyFriendStatus;
    
        public NotificationsOptions()
        {
            Children = new Drawable[]
            {
                chatTicker = new CheckBoxOption { LabelText = "Enable chat ticker" },
                notifyMention = new CheckBoxOption { LabelText = "Show a notification popup when someone says your name" },
                notifyChat = new CheckBoxOption { LabelText = "Show chat message notifications" },
                audibleNotification = new CheckBoxOption { LabelText = "Play a sound when someone says your name" },
                notificationsDuringGameplay = new CheckBoxOption { LabelText = "Show notification popups instantly during gameplay" },
                notifyFriendStatus = new CheckBoxOption { LabelText = "Show notification popups when friends change status" },
            };
        }
        
        protected override void Load(BaseGame game)
        {
            base.Load(game);
            var osuGame = game as OsuGameBase;
            if (osuGame != null)
            {
                chatTicker.Bindable = osuGame.Config.GetBindable<bool>(OsuConfig.Ticker);
                notifyMention.Bindable = osuGame.Config.GetBindable<bool>(OsuConfig.ChatHighlightName);
                notifyChat.Bindable = osuGame.Config.GetBindable<bool>(OsuConfig.ChatMessageNotification);
                audibleNotification.Bindable = osuGame.Config.GetBindable<bool>(OsuConfig.ChatAudibleHighlight);
                notificationsDuringGameplay.Bindable = osuGame.Config.GetBindable<bool>(OsuConfig.PopupDuringGameplay);
                notifyFriendStatus.Bindable = osuGame.Config.GetBindable<bool>(OsuConfig.NotifyFriends);
            }
        }
    }
}