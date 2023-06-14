// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Globalization;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Localisation.HUD;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Judgements;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Mods
{
    public class ModAccuracyChallenge : ModFailCondition, IApplicableToScoreProcessor
    {
        public override string Name => "Accuracy Challenge";

        public override string Acronym => "AC";

        public override LocalisableString Description => "Fail if your accuracy drops too low!";

        public override ModType Type => ModType.DifficultyIncrease;

        public override double ScoreMultiplier => 1.0;

        public override Type[] IncompatibleMods => base.IncompatibleMods.Concat(new[] { typeof(ModEasyWithExtraLives), typeof(ModPerfect) }).ToArray();

        public override bool RequiresConfiguration => false;

        public override string SettingDescription => base.SettingDescription.Replace(MinimumAccuracy.ToString(), MinimumAccuracy.Value.ToString("##%", NumberFormatInfo.InvariantInfo));

        [SettingSource("Minimum accuracy", "Trigger a failure if your accuracy goes below this value.", SettingControlType = typeof(SettingsPercentageSlider<double>))]
        public BindableNumber<double> MinimumAccuracy { get; } = new BindableDouble
        {
            MinValue = 0.60,
            MaxValue = 0.99,
            Precision = 0.01,
            Default = 0.9,
            Value = 0.9,
        };

        [SettingSource("Accuracy mode", "The mode of accuracy that will trigger failure.")]
        public Bindable<AccuracyMode> AccuracyJudgeMode { get; } = new Bindable<AccuracyMode>();

        private readonly Bindable<double> currentAccuracy = new Bindable<double>();

        public void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
        {
            switch (AccuracyJudgeMode.Value)
            {
                case AccuracyMode.Standard:
                    currentAccuracy.BindTo(scoreProcessor.Accuracy);
                    break;

                case AccuracyMode.MaximumAchievable:
                    currentAccuracy.BindTo(scoreProcessor.MaximumAccuracy);
                    break;
            }

            currentAccuracy.BindValueChanged(s =>
            {
                if (s.NewValue < MinimumAccuracy.Value)
                {
                    TriggerFailure();
                }
            });
        }

        public ScoreRank AdjustRank(ScoreRank rank, double accuracy) => rank;

        protected override bool FailCondition(HealthProcessor healthProcessor, JudgementResult result) => false;

        public enum AccuracyMode
        {
            [LocalisableDescription(typeof(GameplayAccuracyCounterStrings), nameof(GameplayAccuracyCounterStrings.AccuracyDisplayModeMax))]
            MaximumAchievable,

            [LocalisableDescription(typeof(GameplayAccuracyCounterStrings), nameof(GameplayAccuracyCounterStrings.AccuracyDisplayModeStandard))]
            Standard,
        }
    }
}
