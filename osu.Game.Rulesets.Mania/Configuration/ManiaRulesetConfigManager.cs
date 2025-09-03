// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
            Migrate();
        }

        protected override void InitialiseDefaults()
        {
            base.InitialiseDefaults();

            SetDefault(ManiaRulesetSetting.ScrollSpeed, 8.0, 1.0, 40.0, 0.1);
            SetDefault(ManiaRulesetSetting.ScrollDirection, ManiaScrollingDirection.Down);
            SetDefault(ManiaRulesetSetting.TimingBasedNoteColouring, false);
            SetDefault(ManiaRulesetSetting.MobileLayout, ManiaMobileLayout.Portrait);
            SetDefault(ManiaRulesetSetting.TouchOverlay, false);
        }

        public void Migrate()
        {
            var mobileLayout = GetBindable<ManiaMobileLayout>(ManiaRulesetSetting.MobileLayout);

#pragma warning disable CS0618 // Type or member is obsolete
            if (mobileLayout.Value == ManiaMobileLayout.LandscapeWithOverlay)
#pragma warning restore CS0618 // Type or member is obsolete
            {
                mobileLayout.Value = ManiaMobileLayout.Landscape;
                SetValue(ManiaRulesetSetting.TouchOverlay, true);
            }
        }

        public override TrackedSettings CreateTrackedSettings() => new TrackedSettings
        {
            new TrackedSetting<double>(ManiaRulesetSetting.ScrollSpeed,
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
        ScrollSpeed,
        ScrollDirection,
        TimingBasedNoteColouring,
        MobileLayout,
        TouchOverlay,
    }
}
