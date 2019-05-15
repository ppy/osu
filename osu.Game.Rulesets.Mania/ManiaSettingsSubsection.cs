// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Mania.Configuration;
using osu.Game.Rulesets.Mania.UI;

namespace osu.Game.Rulesets.Mania
{
    public class ManiaSettingsSubsection : RulesetSettingsSubsection
    {
        protected override string Header => "osu!mania";

        public ManiaSettingsSubsection(ManiaRuleset ruleset)
            : base(ruleset)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var config = (ManiaRulesetConfigManager)Config;

            Children = new Drawable[]
            {
                new SettingsEnumDropdown<ManiaScrollingDirection>
                {
                    LabelText = "Scrolling direction",
                    Bindable = config.GetBindable<ManiaScrollingDirection>(ManiaRulesetSetting.ScrollDirection)
                },
                new SettingsSlider<double, TimeSlider>
                {
                    LabelText = "Scroll speed",
                    Bindable = config.GetBindable<double>(ManiaRulesetSetting.ScrollTime)
                },
            };
        }

        private class TimeSlider : OsuSliderBar<double>
        {
            public override string TooltipText => Current.Value.ToString("N0") + "ms";
        }
    }
}
