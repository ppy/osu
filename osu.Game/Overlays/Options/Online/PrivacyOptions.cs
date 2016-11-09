using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Options.Online
{
    public class PrivacyOptions : OptionsSubsection
    {
        protected override string Header => "Privacy";

        private CheckBoxOption shareCity, allowInvites;
    
        public PrivacyOptions()
        {
            Children = new Drawable[]
            {
                shareCity = new CheckBoxOption { LabelText = "Share your city location with others" },
                allowInvites = new CheckBoxOption { LabelText = "Allow multiplayer game invites from all users" },
            };
        }
        
        protected override void Load(BaseGame game)
        {
            base.Load(game);
            var osuGame = game as OsuGameBase;
            if (osuGame != null)
            {
                shareCity.Bindable = osuGame.Config.GetBindable<bool>(OsuConfig.DisplayCityLocation);
                allowInvites.Bindable = osuGame.Config.GetBindable<bool>(OsuConfig.AllowPublicInvites);
            }
        }
    }
}