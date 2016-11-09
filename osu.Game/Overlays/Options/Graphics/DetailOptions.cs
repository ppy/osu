using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Options.Graphics
{
    public class DetailOptions : OptionsSubsection
    {
        protected override string Header => "Detail Settings";

        private CheckBoxOption snakingSliders, backgroundVideo, storyboards, comboBursts,
            hitLighting, shaders, softeningFilter;

        public DetailOptions()
        {
            Children = new Drawable[]
            {
                snakingSliders = new CheckBoxOption { LabelText = "Snaking sliders" },
                backgroundVideo = new CheckBoxOption { LabelText = "Background video" },
                storyboards = new CheckBoxOption { LabelText = "Storyboards" },
                comboBursts = new CheckBoxOption { LabelText = "Combo bursts" },
                hitLighting = new CheckBoxOption { LabelText = "Hit lighting" },
                shaders = new CheckBoxOption { LabelText = "Shaders" },
                softeningFilter = new CheckBoxOption { LabelText = "Softening filter" },
                new SpriteText { Text = "Screenshot format TODO: dropdown" }
            };
        }

        protected override void Load(BaseGame game)
        {
            base.Load(game);
            var osuGame = game as OsuGameBase;
            if (osuGame != null)
            {
                snakingSliders.Bindable = osuGame.Config.GetBindable<bool>(OsuConfig.SnakingSliders);
                backgroundVideo.Bindable = osuGame.Config.GetBindable<bool>(OsuConfig.Video);
                storyboards.Bindable = osuGame.Config.GetBindable<bool>(OsuConfig.ShowStoryboard);
                comboBursts.Bindable = osuGame.Config.GetBindable<bool>(OsuConfig.ComboBurst);
                hitLighting.Bindable = osuGame.Config.GetBindable<bool>(OsuConfig.HitLighting);
                shaders.Bindable = osuGame.Config.GetBindable<bool>(OsuConfig.Bloom);
                softeningFilter.Bindable = osuGame.Config.GetBindable<bool>(OsuConfig.BloomSoftening);
            }
        }
    }
}