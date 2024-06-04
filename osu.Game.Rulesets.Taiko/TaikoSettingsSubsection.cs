// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Localisation;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Taiko.Configuration;

namespace osu.Game.Rulesets.Taiko
{
    public partial class TaikoSettingsSubsection : RulesetSettingsSubsection
    {
        protected override LocalisableString Header => "osu!taiko";

        public TaikoSettingsSubsection(TaikoRuleset ruleset)
            : base(ruleset)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var config = (TaikoRulesetConfigManager)Config;

            Children = new Drawable[]
            {
                new SettingsEnumDropdown<TaikoTouchControlScheme>
                {
                    LabelText = RulesetSettingsStrings.TouchControlScheme,
                    Current = config.GetBindable<TaikoTouchControlScheme>(TaikoRulesetSetting.TouchControlScheme)
                }
            };
        }
    }
}
