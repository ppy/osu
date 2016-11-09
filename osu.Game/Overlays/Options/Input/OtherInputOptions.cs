using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Options.Input
{
    public class OtherInputOptions : OptionsSubsection
    {
        protected override string Header => "Other";

        private CheckBoxOption tabletSupport, wiimoteSupport;

        public OtherInputOptions()
        {
            Children = new Drawable[]
            {
                tabletSupport = new CheckBoxOption { LabelText = "OS TabletPC support" },
                wiimoteSupport = new CheckBoxOption { LabelText = "Wiimote/TaTaCon Drum Support" },
            };
        }

        protected override void Load(BaseGame game)
        {
            base.Load(game);
            var osuGame = game as OsuGameBase;
            if (osuGame != null)
            {
                tabletSupport.Bindable = osuGame.Config.GetBindable<bool>(OsuConfig.Tablet);
                wiimoteSupport.Bindable = osuGame.Config.GetBindable<bool>(OsuConfig.Wiimote);
            }
        }
    }
}

