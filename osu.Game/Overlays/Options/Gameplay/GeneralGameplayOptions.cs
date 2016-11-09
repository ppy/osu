using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Options.Gameplay
{
    public class GeneralGameplayOptions : OptionsSubsection
    {
        protected override string Header => "General";

        private CheckBoxOption keyOverlay, hiddenApproachCircle, scaleManiaScroll, rememberManiaScroll;

        public GeneralGameplayOptions()
        {
            Children = new Drawable[]
            {
                new SpriteText { Text = "Background dim: TODO slider" },
                new SpriteText { Text = "Progress display: TODO dropdown" },
                new SpriteText { Text = "Score meter type: TODO dropdown" },
                new SpriteText { Text = "Score meter size: TODO slider" },
                keyOverlay = new CheckBoxOption { LabelText = "Always show key overlay" },
                hiddenApproachCircle = new CheckBoxOption { LabelText = "Show approach circle on first \"Hidden\" object" },
                scaleManiaScroll = new CheckBoxOption { LabelText = "Scale osu!mania scroll speed with BPM" },
                rememberManiaScroll = new CheckBoxOption { LabelText = "Remember osu!mania scroll speed per beatmap" },
            };
        }

        protected override void Load(BaseGame game)
        {
            base.Load(game);
            var osuGame = game as OsuGameBase;
            if (osuGame != null)
            {
                keyOverlay.Bindable = osuGame.Config.GetBindable<bool>(OsuConfig.KeyOverlay);
                hiddenApproachCircle.Bindable = osuGame.Config.GetBindable<bool>(OsuConfig.HiddenShowFirstApproach);
                scaleManiaScroll.Bindable = osuGame.Config.GetBindable<bool>(OsuConfig.ManiaSpeedBPMScale);
                rememberManiaScroll.Bindable = osuGame.Config.GetBindable<bool>(OsuConfig.UsePerBeatmapManiaSpeed);
            }
        }
    }
}