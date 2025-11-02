// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Localisation;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Osu.Configuration;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Osu.UI
{
    public partial class OsuSettingsSubsection : RulesetSettingsSubsection
    {
        protected override LocalisableString Header => "osu!";

        public OsuSettingsSubsection(Ruleset ruleset)
            : base(ruleset)
        {
        }

        private readonly BindableFloat hitObjectDimmingStrength = new BindableFloat
        {
            MinValue = 0f,
            MaxValue = 2f,
            Precision = 0.05f,
            Default = 1f,
        };

        private readonly BindableFloat kiaiFlashStrength = new BindableFloat
        {
            MinValue = 0.5f,
            MaxValue = 2f,
            Precision = 0.05f,
            Default = 1f,
        };

        [BackgroundDependencyLoader]
        private void load()
        {
            var config = (OsuRulesetConfigManager)Config;

            config.BindWith(OsuRulesetSetting.HitObjectDimmingStrength, hitObjectDimmingStrength);
            config.BindWith(OsuRulesetSetting.KiaiFlashStrength, kiaiFlashStrength);

            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = RulesetSettingsStrings.SnakingInSliders,
                    Current = config.GetBindable<bool>(OsuRulesetSetting.SnakingInSliders)
                },
                new SettingsCheckbox
                {
                    ClassicDefault = false,
                    LabelText = RulesetSettingsStrings.SnakingOutSliders,
                    Current = config.GetBindable<bool>(OsuRulesetSetting.SnakingOutSliders)
                },
                new SettingsCheckbox
                {
                    LabelText = RulesetSettingsStrings.CursorTrail,
                    Current = config.GetBindable<bool>(OsuRulesetSetting.ShowCursorTrail)
                },
                new SettingsCheckbox
                {
                    LabelText = RulesetSettingsStrings.CursorRipples,
                    Current = config.GetBindable<bool>(OsuRulesetSetting.ShowCursorRipples)
                },
                new SettingsEnumDropdown<PlayfieldBorderStyle>
                {
                    LabelText = RulesetSettingsStrings.PlayfieldBorderStyle,
                    Current = config.GetBindable<PlayfieldBorderStyle>(OsuRulesetSetting.PlayfieldBorderStyle),
                },
                new SettingsSlider<float>
                {
                    LabelText = RulesetSettingsStrings.HitObjectDimmingStrength,
                    Keywords = new[] { "dimming", "lightup", "pre-hit" },
                    KeyboardStep = 0.05f,
                    Current = hitObjectDimmingStrength,
                },
                new SettingsSlider<float>
                {
                    LabelText = RulesetSettingsStrings.KiaiFlashStrength,
                    Keywords = new[] { "kiai", "flash", "intensity" },
                    KeyboardStep = 0.05f,
                    Current = kiaiFlashStrength,
                },
                new SettingsEnumDropdown<KiaiFlashFrequency>
                {
                    LabelText = RulesetSettingsStrings.KiaiFlashFrequency,
                    Keywords = new[] { "kiai", "frequency", "cadence", "beat" },
                    Current = config.GetBindable<KiaiFlashFrequency>(OsuRulesetSetting.KiaiFlashFrequency),
                },
            };
        }
    }
}
