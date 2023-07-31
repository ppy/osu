// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Configuration.Tracking;
using osu.Game.Configuration;
using osu.Game.Localisation;
using osu.Game.Rulesets.Configuration;
using osu.Game.Rulesets.Mania.UI;

namespace osu.Game.Rulesets.Mania.Configuration
{
    public class ManiaRulesetConfigManager : RulesetConfigManager<ManiaRulesetSetting>
    {
        public ManiaRulesetConfigManager(SettingsStore? settings, RulesetInfo ruleset, int? variant = null)
            : base(settings, ruleset, variant)
        {
        }

        protected override void InitialiseDefaults()
        {
            base.InitialiseDefaults();

            SetDefault(ManiaRulesetSetting.ScrollSpeed, 8, 1, 40);
            SetDefault(ManiaRulesetSetting.ScrollDirection, ManiaScrollingDirection.Down);
            SetDefault(ManiaRulesetSetting.TimingBasedNoteColouring, false);

#pragma warning disable CS0618
            // Although obsolete, this is still required to populate the bindable from the database in case migration is required.
            SetDefault<double?>(ManiaRulesetSetting.ScrollTime, null);

            if (Get<double?>(ManiaRulesetSetting.ScrollTime) is double scrollTime)
            {
                SetValue(ManiaRulesetSetting.ScrollSpeed, (int)Math.Round(DrawableManiaRuleset.MAX_TIME_RANGE / scrollTime));
                SetValue<double?>(ManiaRulesetSetting.ScrollTime, null);
            }
#pragma warning restore CS0618
        }

        public override TrackedSettings CreateTrackedSettings() => new TrackedSettings
        {
            new TrackedSetting<int>(ManiaRulesetSetting.ScrollSpeed,
                speed => new SettingDescription(
                    rawValue: speed,
                    name: RulesetSettingsStrings.ScrollSpeed,
                    value: RulesetSettingsStrings.ScrollSpeedTooltip((int)DrawableManiaRuleset.ComputeScrollTime(speed), speed)
                )
            )
        };
    }

    public enum ManiaRulesetSetting
    {
        [Obsolete("Use ScrollSpeed instead.")] // Can be removed 2023-11-30
        ScrollTime,
        ScrollSpeed,
        ScrollDirection,
        TimingBasedNoteColouring
    }
}
