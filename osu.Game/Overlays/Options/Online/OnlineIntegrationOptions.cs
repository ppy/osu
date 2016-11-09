using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Options.Online
{
    public class OnlineIntegrationOptions : OptionsSubsection
    {
        protected override string Header => "Integration";

        private CheckBoxOption yahoo, msn, autoDirect, noVideo;

        public OnlineIntegrationOptions()
        {
            Children = new Drawable[]
            {
                yahoo = new CheckBoxOption { LabelText = "Integrate with Yahoo! status display" },
                msn = new CheckBoxOption { LabelText = "Integrate with MSN Live status display" },
                autoDirect = new CheckBoxOption { LabelText = "Automatically start osu!direct downloads" },
                noVideo = new CheckBoxOption { LabelText = "Prefer no-video downloads" },
            };
        }

        protected override void Load(BaseGame game)
        {
            base.Load(game);
            var osuGame = game as OsuGameBase;
            if (osuGame != null)
            {
                yahoo.Bindable = osuGame.Config.GetBindable<bool>(OsuConfig.YahooIntegration);
                msn.Bindable = osuGame.Config.GetBindable<bool>(OsuConfig.MsnIntegration);
                autoDirect.Bindable = osuGame.Config.GetBindable<bool>(OsuConfig.AutomaticDownload);
                noVideo.Bindable = osuGame.Config.GetBindable<bool>(OsuConfig.AutomaticDownloadNoVideo);
            }
        }
    }
}