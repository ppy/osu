// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;

namespace osu.Game.Rulesets.Osu.UI
{
    public class OsuSettings : RulesetSettingsSubsection
    {
        protected override string Header => "osu!";

        public OsuSettings(Ruleset ruleset)
            : base(ruleset)
        {
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = "Snaking in sliders",
                    Bindable = config.GetBindable<bool>(OsuSetting.SnakingInSliders)
                },
                new SettingsCheckbox
                {
                    LabelText = "Snaking out sliders",
                    Bindable = config.GetBindable<bool>(OsuSetting.SnakingOutSliders)
                },
                new SettingsSlider<int, TimeSlider>
                {
                    LabelText = "Followpoint fadeout time",
                    TransferValueOnCommit = true,
                    Bindable = config.GetBindable<int>(OsuSetting.FollowPointAppearTime),
                    KeyboardStep = 1
                },
                new SettingsSlider<int, TimeSlider>
                {
                    LabelText = "Followpoint fadeout offset",
                    TransferValueOnCommit = true,
                    Bindable = config.GetBindable<int>(OsuSetting.FollowPointDelay),
                    KeyboardStep = 1
                }
            };
        }
        private class TimeSlider : OsuSliderBar<int>
        {
            public override string TooltipText => base.TooltipText + "ms";
        }
    }
}
