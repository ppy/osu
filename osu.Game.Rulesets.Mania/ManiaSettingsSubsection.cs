// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Mania.Configuration;
using osu.Game.Rulesets.Mania.UI;

namespace osu.Game.Rulesets.Mania
{
    public class ManiaSettingsSubsection : RulesetSettingsSubsection
    {
        protected override LocalisableString Header => "osu!mania";

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
                    Current = config.GetBindable<ManiaScrollingDirection>(ManiaRulesetSetting.ScrollDirection)
                },
                new SettingsSlider<double, TimeSlider>
                {
                    LabelText = "Scroll speed",
                    Current = config.GetBindable<double>(ManiaRulesetSetting.ScrollTime),
                    KeyboardStep = 5
                },
                new SettingsCheckbox
                {
                    LabelText = "Timing-based note colouring",
                    Current = config.GetBindable<bool>(ManiaRulesetSetting.TimingBasedNoteColouring),
                }
            };
        }
    }
}
