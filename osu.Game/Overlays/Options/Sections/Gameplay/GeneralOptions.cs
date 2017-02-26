// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Options.Sections.Gameplay
{
    public class GeneralOptions : OptionsSubsection
    {
        protected override string Header => "General";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new OptionSlider<int>
                {
                    LabelText = "Background dim",
                    Bindable = (BindableInt)config.GetWeldedBindable<int>(OsuConfig.DimLevel)
                },
                new OptionEnumDropDown<ProgressBarType>
                {
                    LabelText = "Progress display",
                    Bindable = config.GetWeldedBindable<ProgressBarType>(OsuConfig.ProgressBarType)
                },
                new OptionEnumDropDown<ScoreMeterType>
                {
                    LabelText = "Score meter type",
                    Bindable = config.GetWeldedBindable<ScoreMeterType>(OsuConfig.ScoreMeter)
                },
                new OptionSlider<double>
                {
                    LabelText = "Score meter size",
                    Bindable = (BindableDouble)config.GetWeldedBindable<double>(OsuConfig.ScoreMeterScale)
                },
                new OsuCheckbox
                {
                    LabelText = "Always show key overlay",
                    Bindable = config.GetWeldedBindable<bool>(OsuConfig.KeyOverlay)
                },
                new OsuCheckbox
                {
                    LabelText = "Show approach circle on first \"Hidden\" object",
                    Bindable = config.GetWeldedBindable<bool>(OsuConfig.HiddenShowFirstApproach)
                },
                new OsuCheckbox
                {
                    LabelText = "Scale osu!mania scroll speed with BPM",
                    Bindable = config.GetWeldedBindable<bool>(OsuConfig.ManiaSpeedBPMScale)
                },
                new OsuCheckbox
                {
                    LabelText = "Remember osu!mania scroll speed per beatmap",
                    Bindable = config.GetWeldedBindable<bool>(OsuConfig.UsePerBeatmapManiaSpeed)
                },
            };
        }
    }
}
