// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Configuration;
using osu.Game.Rulesets.Configuration;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Osu.Configuration
{
    public class OsuRulesetConfigManager : RulesetConfigManager<OsuRulesetSetting>
    {
        // i absolutely hate globals but im lazy so whatever
        public static OsuRulesetConfigManager Instance { get; private set; }

        public OsuRulesetConfigManager(SettingsStore settings, RulesetInfo ruleset, int? variant = null)
            : base(settings, ruleset, variant)
        {
            Instance = this;
        }

        protected override void InitialiseDefaults()
        {
            base.InitialiseDefaults();
            Set(OsuRulesetSetting.SnakingInSliders, true);
            Set(OsuRulesetSetting.SnakingOutSliders, true);
            Set(OsuRulesetSetting.ShowCursorTrail, true);
            Set(OsuRulesetSetting.ReplayFramerate, 120f, 24f, 990f, 2f);
            Set(OsuRulesetSetting.DanceMover, OsuDanceMover.Momentum);
            Set(OsuRulesetSetting.AngleOffset, 8f / 18f, 0f, 2f, float.Epsilon);
            Set(OsuRulesetSetting.JumpMulti, 2f / 3f, 0f, 2f, float.Epsilon);
            Set(OsuRulesetSetting.NextJumpMulti, 2f / 3f, 0f, 2f, float.Epsilon);
            Set(OsuRulesetSetting.SkipStackAngles, true);
            Set(OsuRulesetSetting.BorderBounce, true);
            Set(OsuRulesetSetting.PlayfieldBorderStyle, PlayfieldBorderStyle.None);
        }
    }

    public enum OsuDanceMover
    {
        HalfCircle,
        Flower,
        Momentum
    }

    public enum OsuRulesetSetting
    {
        SnakingInSliders,
        SnakingOutSliders,
        ShowCursorTrail,
        ReplayFramerate,
        DanceMover,
        AngleOffset,
        JumpMulti,
        NextJumpMulti,
        BorderBounce,
        SkipStackAngles,
        PlayfieldBorderStyle,
    }
}
