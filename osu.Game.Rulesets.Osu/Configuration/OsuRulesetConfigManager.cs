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
            SetDefault(OsuRulesetSetting.SnakingInSliders, true);
            SetDefault(OsuRulesetSetting.SnakingOutSliders, true);
            SetDefault(OsuRulesetSetting.ShowCursorTrail, true);
            SetDefault(OsuRulesetSetting.PlayfieldBorderStyle, PlayfieldBorderStyle.None);

            SetDefault(OsuRulesetSetting.ReplayFramerate, 120f, 24f, 990f, 2f);
            SetDefault(OsuRulesetSetting.DanceMover, OsuDanceMover.Momentum);
            SetDefault(OsuRulesetSetting.AngleOffset, 8f / 18f, 0f, 2f, float.Epsilon);
            SetDefault(OsuRulesetSetting.JumpMulti, 2f / 3f, 0f, 2f, float.Epsilon);
            SetDefault(OsuRulesetSetting.NextJumpMulti, 2f / 3f, 0f, 2f, float.Epsilon);
            SetDefault(OsuRulesetSetting.SkipStackAngles, true);
            SetDefault(OsuRulesetSetting.BorderBounce, true);
            SetDefault(OsuRulesetSetting.PippiSpinner, false);
            SetDefault(OsuRulesetSetting.PippiStream, false);
            SetDefault(OsuRulesetSetting.SpinnerDance, true);
            SetDefault(OsuRulesetSetting.SliderDance, true);
            SetDefault(OsuRulesetSetting.SpinnerRadiusStart, 235f, 10f, 350f, 1f);
            SetDefault(OsuRulesetSetting.SpinnerRadiusEnd, 15f, 10f, 250f, 1f);
        }
    }

    public enum OsuDanceMover
    {
        HalfCircle,
        Flower,
        Momentum,
        Pippi
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
        PippiSpinner,
        PippiStream,
        PlayfieldBorderStyle,
        SpinnerRadiusStart,
        SpinnerRadiusEnd,
        SpinnerDance,
        SliderDance
    }
}
