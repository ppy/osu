// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Judgements;

namespace osu.Game.Rulesets.Mods
{
    public class ModAccuracyChallenge : ModFailCondition
    {
        public override string Name => "Accuracy Challenge";
        public override string Acronym => "AC";
        public override string Description => "Fail the map if you don't maintain a certain accuracy.";
        public override IconUsage? Icon => FontAwesome.Solid.Calculator;
        public override ModType Type => ModType.DifficultyIncrease;
        public override double ScoreMultiplier => 1.0;
        public override Type[] IncompatibleMods => base.IncompatibleMods.Concat(new[] { typeof(ModEasyWithExtraLives), typeof(ModPerfect) }).ToArray();
        public override bool RequiresConfiguration => false;

        [SettingSource("Minimum accuracy", "Trigger a failure if your accuracy goes below this value.", SettingControlType = typeof(SettingsSlider<double, PercentSlider>))]
        public BindableNumber<double> MinimumAccuracy { get; } = new BindableDouble
        {
            MinValue = 0.01,
            MaxValue = 1,
            Precision = 0.01,
            Default = 0.9,
            Value = 0.9,
        };

        private double baseScore;
        private double maxBaseScore;
        private double accuracy => maxBaseScore > 0 ? baseScore / maxBaseScore : 1;

        protected override bool FailCondition(HealthProcessor healthProcessor, JudgementResult result)
        {
            // accuracy calculation logic taken from `ScoreProcessor`. should be updated here if the formula ever changes.
            if (!result.Type.IsScorable() || result.Type.IsBonus())
                return false;

            baseScore += result.Type.IsHit() ? result.Judgement.NumericResultFor(result) : 0;
            maxBaseScore += result.Judgement.MaxNumericResult;

            return accuracy < MinimumAccuracy.Value;
        }
    }

    public class PercentSlider : OsuSliderBar<double>
    {
        public PercentSlider()
        {
            DisplayAsPercentage = true;
        }
    }
}
