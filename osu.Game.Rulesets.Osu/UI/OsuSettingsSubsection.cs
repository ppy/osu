// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Osu.Configuration;
using osu.Game.Rulesets.UI;
using osu.Game.Localisation;

namespace osu.Game.Rulesets.Osu.UI
{
    public class OsuSettingsSubsection : RulesetSettingsSubsection
    {
        protected override LocalisableString Header => "osu!";

        public OsuSettingsSubsection(Ruleset ruleset)
            : base(ruleset)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var config = (OsuRulesetConfigManager)Config;

            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = OsuSettingsSubsectionStrings.SnakingInSliders,
                    Current = config.GetBindable<bool>(OsuRulesetSetting.SnakingInSliders)
                },
                new SettingsCheckbox
                {
                    ClassicDefault = false,
                    LabelText = OsuSettingsSubsectionStrings.SnakingOutSliders,
                    Current = config.GetBindable<bool>(OsuRulesetSetting.SnakingOutSliders)
                },
                new SettingsCheckbox
                {
                    LabelText = OsuSettingsSubsectionStrings.CursorTrail,
                    Current = config.GetBindable<bool>(OsuRulesetSetting.ShowCursorTrail)
                },
                new SettingsEnumDropdown<PlayfieldBorderStyle>
                {
                    LabelText = OsuSettingsSubsectionStrings.PlayfieldBorderStyle,
                    Current = config.GetBindable<PlayfieldBorderStyle>(OsuRulesetSetting.PlayfieldBorderStyle),
                },
            };
        }
    }
}
