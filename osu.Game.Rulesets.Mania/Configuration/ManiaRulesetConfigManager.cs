// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Configuration.Tracking;
using osu.Game.Configuration;
using osu.Game.Rulesets.Configuration;
using osu.Game.Rulesets.Mania.UI;

namespace osu.Game.Rulesets.Mania.Configuration
{
    public class ManiaRulesetConfigManager : RulesetConfigManager<ManiaRulesetSetting>
    {
        public ManiaRulesetConfigManager(SettingsStore settings, RulesetInfo ruleset, int? variant = null)
            : base(settings, ruleset, variant)
        {
        }

        protected override void InitialiseDefaults()
        {
            base.InitialiseDefaults();

            SetDefault(ManiaRulesetSetting.ScrollTime, 1500.0, DrawableManiaRuleset.MIN_TIME_RANGE, DrawableManiaRuleset.MAX_TIME_RANGE, 5);
            SetDefault(ManiaRulesetSetting.ScrollDirection, ManiaScrollingDirection.Down);
            SetDefault(ManiaRulesetSetting.TimingBasedNoteColouring, false);
        }

        public override TrackedSettings CreateTrackedSettings() => new TrackedSettings
        {
            new TrackedSetting<double>(ManiaRulesetSetting.ScrollTime,
                scrollTime => new SettingDescription(
                    rawValue: scrollTime,
                    name: "Scroll Speed",
                    value: $"{(int)Math.Round(DrawableManiaRuleset.MAX_TIME_RANGE / scrollTime)} ({scrollTime}ms)"
                )
            )
        };
    }

    public enum ManiaRulesetSetting
    {
        ScrollTime,
        ScrollDirection,
        TimingBasedNoteColouring
    }
}
