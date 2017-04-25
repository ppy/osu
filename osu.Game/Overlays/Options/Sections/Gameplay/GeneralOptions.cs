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
                    Bindable = (BindableInt)config.GetBindable<int>(OsuConfig.DimLevel)
                },
                new OptionEnumDropdown<ProgressBarType>
                {
                    LabelText = "Progress display",
                    Bindable = config.GetBindable<ProgressBarType>(OsuConfig.ProgressBarType)
                },
                new OptionEnumDropdown<ScoreMeterType>
                {
                    LabelText = "Score meter type",
                    Bindable = config.GetBindable<ScoreMeterType>(OsuConfig.ScoreMeter)
                },
                new OptionSlider<double>
                {
                    LabelText = "Score meter size",
                    Bindable = (BindableDouble)config.GetBindable<double>(OsuConfig.ScoreMeterScale)
                },
                new OsuCheckbox
                {
                    LabelText = "Show score overlay",
                    Bindable = config.GetBindable<bool>(OsuConfig.ShowInterface)
                },
                new OsuCheckbox
                {
                    LabelText = "Always show key overlay",
                    Bindable = config.GetBindable<bool>(OsuConfig.KeyOverlay)
                },
                new OsuCheckbox
                {
                    LabelText = "Show approach circle on first \"Hidden\" object",
                    Bindable = config.GetBindable<bool>(OsuConfig.HiddenShowFirstApproach)
                },
                new OsuCheckbox
                {
                    LabelText = "Scale osu!mania scroll speed with BPM",
                    Bindable = config.GetBindable<bool>(OsuConfig.ManiaSpeedBPMScale)
                },
                new OsuCheckbox
                {
                    LabelText = "Remember osu!mania scroll speed per beatmap",
                    Bindable = config.GetBindable<bool>(OsuConfig.UsePerBeatmapManiaSpeed)
                },
            };
        }
    }
}
