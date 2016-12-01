using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Options.Gameplay
{
    public class GeneralGameplayOptions : OptionsSubsection
    {
        protected override string Header => "General";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new OptionsSlider<int>
                {
                    Label = "Background dim",
                    Bindable = (BindableInt)config.GetBindable<int>(OsuConfig.DimLevel)
                },
                new SpriteText { Text = "Progress display: TODO dropdown" },
                new SpriteText { Text = "Score meter type: TODO dropdown" },
                new OptionsSlider<double>
                {
                    Label = "Score meter size",
                    Bindable = (BindableDouble)config.GetBindable<double>(OsuConfig.ScoreMeterScale)
                },
                new CheckBoxOption
                {
                    LabelText = "Always show key overlay",
                    Bindable = config.GetBindable<bool>(OsuConfig.KeyOverlay)
                },
                new CheckBoxOption
                {
                    LabelText = "Show approach circle on first \"Hidden\" object",
                    Bindable = config.GetBindable<bool>(OsuConfig.HiddenShowFirstApproach)
                },
                new CheckBoxOption
                {
                    LabelText = "Scale osu!mania scroll speed with BPM",
                    Bindable = config.GetBindable<bool>(OsuConfig.ManiaSpeedBPMScale)
                },
                new CheckBoxOption
                {
                    LabelText = "Remember osu!mania scroll speed per beatmap",
                    Bindable = config.GetBindable<bool>(OsuConfig.UsePerBeatmapManiaSpeed)
                },
            };
        }
    }
}