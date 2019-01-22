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
        public  SettingsSlider<int, TimeSlider> FollowpointFadeoutSlider;
        public  SettingsSlider<int, TimeSlider> FollowpointDelaySlider;

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
                FollowpointFadeoutSlider = CreateFadeOutSettingsSlider(config),
                FollowpointDelaySlider = CreateOffsetSettingsSlider(config)
            };
            FollowpointDelaySlider.Bindable.ValueChanged += _ =>
            {
                if (FollowpointFadeoutSlider.Bindable.Value - FollowpointDelaySlider.Bindable.Value > 800)
                {
                    FollowpointFadeoutSlider.Bindable.Value = 800 + FollowpointDelaySlider.Bindable.Value;
                }
            };
            FollowpointFadeoutSlider.Bindable.ValueChanged += _ =>
            {
                if (FollowpointFadeoutSlider.Bindable.Value - FollowpointDelaySlider.Bindable.Value > 800)
                {
                    FollowpointDelaySlider.Bindable.Value = FollowpointFadeoutSlider.Bindable.Value - 800;
                }
            };
        }
       public class TimeSlider : OsuSliderBar<int>
        {
            public override string TooltipText => base.TooltipText + "ms";

        }

        protected virtual SettingsSlider<int, TimeSlider> CreateFadeOutSettingsSlider(OsuConfigManager config)
        {
            return new SettingsSlider<int, TimeSlider>
            {
                LabelText = "Followpoint fadeout time",
                TransferValueOnCommit = true,
                Bindable = config.GetBindable<int>(OsuSetting.FollowPointAppearTime),
                KeyboardStep = 1
            };
        }

        protected virtual SettingsSlider<int, TimeSlider> CreateOffsetSettingsSlider(OsuConfigManager config)
        {
            return new SettingsSlider<int, TimeSlider>
            {
                LabelText = "Followpoint fadeout offset",
                TransferValueOnCommit = true,
                Bindable = config.GetBindable<int>(OsuSetting.FollowPointDelay),
                KeyboardStep = 1
            };
        }
    }
}
