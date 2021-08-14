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
            SetDefault(OsuRulesetSetting.BorderBounce, true);
            SetDefault(OsuRulesetSetting.PippiSpinner, false);
            SetDefault(OsuRulesetSetting.PippiStream, false);
            SetDefault(OsuRulesetSetting.SpinnerDance, true);
            SetDefault(OsuRulesetSetting.SliderDance, true);
            SetDefault(OsuRulesetSetting.SpinnerRadiusStart, 235f, 10f, 350f, 1f);
            SetDefault(OsuRulesetSetting.SpinnerRadiusEnd, 15f, 10f, 250f, 1f);

            SetDefault(OsuRulesetSetting.AngleOffset, 0.45f, 0f, 2f, 0.01f);
            SetDefault(OsuRulesetSetting.JumpMult, 0.6f, 0f, 2f, 0.01f);
            SetDefault(OsuRulesetSetting.NextJumpMult, 0.25f, 0f, 2f, 0.01f);
            SetDefault(OsuRulesetSetting.DurationTrigger, 500f, 0f, 5000f, 1f);
            SetDefault(OsuRulesetSetting.DurationMult, 2f, 0f, 50f, 0.1f);
            SetDefault(OsuRulesetSetting.StreamMult, 0.7f, 0f, 50f, 0.1f);
            SetDefault(OsuRulesetSetting.RestrictAngle, 90f, 1f, 180f);
            SetDefault(OsuRulesetSetting.RestrictArea, 40f, 1f, 180f);
            SetDefault(OsuRulesetSetting.StreamRestrict, false);
            SetDefault(OsuRulesetSetting.RestrictInvert, true);
            SetDefault(OsuRulesetSetting.SkipStackAngles, false);

            //Bezier mover settings
            SetDefault(OsuRulesetSetting.BezierAggressiveness, 60f, 1f, 180f);
            SetDefault(OsuRulesetSetting.BezierSliderAggressiveness, 3f, 1f, 20f);
        }
    }

    public enum OsuDanceMover
    {
        HalfCircle,
        Flower,
        Momentum,
        Pippi,
        AxisAligned,
        Aggresive,
        Bezier
    }

    public enum OsuRulesetSetting
    {
        SnakingInSliders,
        SnakingOutSliders,
        ShowCursorTrail,
        ReplayFramerate,
        DanceMover,
        AngleOffset,
        JumpMult,
        NextJumpMult,
        BorderBounce,
        SkipStackAngles,
        PippiSpinner,
        PippiStream,
        PlayfieldBorderStyle,
        SpinnerRadiusStart,
        SpinnerRadiusEnd,
        SpinnerDance,
        SliderDance,
        StreamMult,
        RestrictInvert,
        RestrictArea,
        RestrictAngle,
        StreamRestrict,
        DurationTrigger,
        DurationMult,
        BezierAggressiveness,
        BezierSliderAggressiveness,
    }
}
