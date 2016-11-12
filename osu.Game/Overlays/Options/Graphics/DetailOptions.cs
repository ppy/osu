using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Options.Graphics
{
    public class DetailOptions : OptionsSubsection
    {
        protected override string Header => "Detail Settings";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new CheckBoxOption
                {
                    LabelText = "Snaking sliders",
                    Bindable = config.GetBindable<bool>(OsuConfig.SnakingSliders)
                },
                new CheckBoxOption
                {
                    LabelText = "Background video",
                    Bindable = config.GetBindable<bool>(OsuConfig.Video)
                },
                new CheckBoxOption
                {
                    LabelText = "Storyboards",
                    Bindable = config.GetBindable<bool>(OsuConfig.ShowStoryboard)
                },
                new CheckBoxOption
                {
                    LabelText = "Combo bursts",
                    Bindable = config.GetBindable<bool>(OsuConfig.ComboBurst)
                },
                new CheckBoxOption
                {
                    LabelText = "Hit lighting",
                    Bindable = config.GetBindable<bool>(OsuConfig.HitLighting)
                },
                new CheckBoxOption
                {
                    LabelText = "Shaders",
                    Bindable = config.GetBindable<bool>(OsuConfig.Bloom)
                },
                new CheckBoxOption
                {
                    LabelText = "Softening filter",
                    Bindable = config.GetBindable<bool>(OsuConfig.BloomSoftening)
                },
                new SpriteText { Text = "Screenshot format TODO: dropdown" }
            };
        }
    }
}