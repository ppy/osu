// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
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
                new OptionSlider<double>
                {
                    LabelText = "Background dim",
                    Bindable = config.GetBindable<double>(OsuConfig.DimLevel)
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
                    Bindable = config.GetBindable<double>(OsuConfig.ScoreMeterScale)
                },
                new OsuCheckbox
                {
                    LabelText = "Show score overlay",
                    Bindable = config.GetBindable<bool>(OsuConfig.ShowInterface),
                    TooltipText = "Hides/shows the score overlay. Can be toggled with shift+tab. Handy for recording replays."
                },
                new OsuCheckbox
                {
                    LabelText = "Always show key overlay",
                    Bindable = config.GetBindable<bool>(OsuConfig.KeyOverlay),
                    TooltipText = "Show the key status overlay even when playing locally. Handy for recording or streaming your play."
                },
                new OsuCheckbox
                {
                    LabelText = "Show approach circle on first \"Hidden\" object",
                    Bindable = config.GetBindable<bool>(OsuConfig.HiddenShowFirstApproach),
                    TooltipText = "Sometimes it can be hard to judge when to click the first hitobject when playing with Hidden on. This allows you to see the first hitobject's approach circle."
                },
                new OsuCheckbox
                {
                    LabelText = "Scale osu!mania scroll speed with BPM",
                    Bindable = config.GetBindable<bool>(OsuConfig.ManiaSpeedBPMScale),
                    TooltipText = "The scroll speed will depend on the current beatmaps's base BPM."
                },
                new OsuCheckbox
                {
                    LabelText = "Remember osu!mania scroll speed per beatmap",
                    Bindable = config.GetBindable<bool>(OsuConfig.UsePerBeatmapManiaSpeed),
                    TooltipText = "Per-beatmap scroll speeds will be stored and used."
                },
            };
        }
    }
}
