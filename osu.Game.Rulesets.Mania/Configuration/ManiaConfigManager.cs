// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Configuration.Tracking;
using osu.Game.Configuration;
using osu.Game.Rulesets.Configuration;
using osu.Game.Rulesets.Mania.UI;

namespace osu.Game.Rulesets.Mania.Configuration
{
    public class ManiaConfigManager : RulesetConfigManager<ManiaSetting>
    {
        public ManiaConfigManager(SettingsStore settings, RulesetInfo ruleset, int? variant = null)
            : base(settings, ruleset, variant)
        {
        }

        protected override void InitialiseDefaults()
        {
            base.InitialiseDefaults();

            Set(ManiaSetting.ScrollTime, 1500.0, 50.0, 10000.0, 50.0);
            Set(ManiaSetting.ScrollDirection, ManiaScrollingDirection.Down);
        }

        public override TrackedSettings CreateTrackedSettings() => new TrackedSettings
        {
            new TrackedSetting<double>(ManiaSetting.ScrollTime, v => new SettingDescription(v, "Scroll Time", $"{v}ms"))
        };
    }

    public enum ManiaSetting
    {
        ScrollTime,
        ScrollDirection
    }
}
