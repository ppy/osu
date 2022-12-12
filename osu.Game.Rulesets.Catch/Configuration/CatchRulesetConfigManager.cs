// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Configuration.Tracking;
using osu.Game.Configuration;
using osu.Game.Rulesets.Configuration;

namespace osu.Game.Rulesets.Catch.Configuration
{
    public class CatchRulesetConfigManager : RulesetConfigManager<CatchRulesetSetting>
    {
        public CatchRulesetConfigManager(SettingsStore? settings, RulesetInfo ruleset, int? variant = null)
            : base(settings, ruleset, variant)
        {
        }

        protected override void InitialiseDefaults()
        {
            base.InitialiseDefaults();

            SetDefault(CatchRulesetSetting.ShowCursorDuringPlay, false);
        }
        public override TrackedSettings CreateTrackedSettings() => new TrackedSettings
        {
            new TrackedSetting<bool>(CatchRulesetSetting.ShowCursorDuringPlay,
                showCursorDuringPlay => new SettingDescription(
                    rawValue: showCursorDuringPlay,
                    name: "Show Cursor During Play",
                    value: showCursorDuringPlay.ToString()
                )
            )
        };
    }

    public enum CatchRulesetSetting
    {
        ShowCursorDuringPlay
    }
}
