using osu.Framework;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Options.Graphics
{
    public class MainMenuOptions : OptionsSubsection
    {
        protected override string Header => "Main Menu";

        private CheckBoxOption snow, parallax, tips, voices, musicTheme;

        public MainMenuOptions()
        {
            Children = new[]
            {
                snow = new CheckBoxOption { LabelText = "Snow" },
                parallax = new CheckBoxOption { LabelText = "Parallax" },
                tips = new CheckBoxOption { LabelText = "Menu tips" },
                voices = new CheckBoxOption { LabelText = "Interface voices" },
                musicTheme = new CheckBoxOption { LabelText = "osu! music theme" },
            };
        }

        protected override void Load(BaseGame game)
        {
            base.Load(game);
            var osuGame = game as OsuGameBase;
            if (osuGame != null)
            {
                snow.Bindable = osuGame.Config.GetBindable<bool>(OsuConfig.MenuSnow);
                parallax.Bindable = osuGame.Config.GetBindable<bool>(OsuConfig.MenuParallax);
                tips.Bindable = osuGame.Config.GetBindable<bool>(OsuConfig.ShowMenuTips);
                voices.Bindable = osuGame.Config.GetBindable<bool>(OsuConfig.MenuVoice);
                musicTheme.Bindable = osuGame.Config.GetBindable<bool>(OsuConfig.MenuMusic);
            }
        }
    }
}