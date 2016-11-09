using osu.Framework.Graphics;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Options.Online
{
    public class OnlineSection : OptionsSection
    {
        protected override string Header => "Online";
        public override FontAwesome Icon => FontAwesome.fa_globe;

        public OnlineSection()
        {
            Children = new Drawable[]
            {
                new InGameChatOptions(),
                new PrivacyOptions(),
                new NotificationsOptions(),
                new OnlineIntegrationOptions(),
            };
        }
    }
}