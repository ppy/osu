//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
                new SliderOption<int>
                {
                    LabelText = "Background dim",
                    Bindable = (BindableInt)config.GetBindable<int>(OsuConfig.DimLevel)
                },
                new DropdownOption<ProgressBarType>
                {
                    LabelText = "Progress display",
                    Bindable = config.GetBindable<ProgressBarType>(OsuConfig.ProgressBarType)
                },
                new DropdownOption<ScoreMeterType>
                {
                    LabelText = "Score meter type",
                    Bindable = config.GetBindable<ScoreMeterType>(OsuConfig.ScoreMeter)
                },
                new SliderOption<double>
                {
                    LabelText = "Score meter size",
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
