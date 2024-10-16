// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModDynamic : Mod, IApplicableToDifficulty, IApplicableToScoreProcessor, IUpdatableByPlayfield
    {
        public override string Name => "Dynamic Circle";
        public override string Acronym => "DC";
        public override LocalisableString Description => "Circle size adjusts dynamically based on your combo.";
        public override ModType Type => ModType.Fun;
        public override Type[] IncompatibleMods => base.IncompatibleMods.Concat(new[] { typeof(OsuModAutoplay), typeof(ModCinema), typeof(ModEasy), typeof(ModHardRock), typeof(OsuModMagnetised), typeof(OsuModRepel), typeof(OsuModFreezeFrame), typeof(ModWithVisibilityAdjustment) }).ToArray();
        public override double ScoreMultiplier => 0.85f;
        public override bool Ranked => false;

        [SettingSource("Maximum Circle Size", "The largest size when combo is low.")]
        public BindableFloat MaxCircleSize { get; } = new BindableFloat(10f)
        {
            MinValue = 0f,
            MaxValue = 10f,
            Precision = 0.1f,
        };

        [SettingSource("Minimum Circle Size", "The smallest size when combo is high.")]
        public BindableFloat MinCircleSize { get; } = new BindableFloat(1f)
        {
            MinValue = 0f,
            MaxValue = 10f,
            Precision = 0.1f,
        };

        [SettingSource("Size Adjustment Speed", "How fast the circle size adjusts.")]
        public BindableFloat SizeAdjustmentSpeed { get; } = new BindableFloat(10f)
        {
            MinValue = 5f,
            MaxValue = 50f,
            Precision = 1f,
        };

        [SettingSource("Inverse Scaling", "Reverse the scaling logic.")]
        public BindableBool IsInverseScaling { get; } = new BindableBool(false);

        private readonly BindableNumber<int> comboCount = new BindableInt();
        private float currentComboValue;

        public void ApplyToDifficulty(BeatmapDifficulty difficulty) => ApplySettingsToDifficulty(difficulty);

        private void ApplySettingsToDifficulty(BeatmapDifficulty difficulty)
        {
            difficulty.CircleSize = 0;
        }

        public void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
        {
            if (scoreProcessor == null) return;

            comboCount.BindTo(scoreProcessor.Combo);
        }

        public void Update(Playfield playfield)
        {
            comboCount.BindValueChanged(combo =>
            {
                currentComboValue = combo.NewValue;
            }, true);

            float maxSize = Math.Max(MaxCircleSize.Value, MinCircleSize.Value);
            float minSize = Math.Min(MaxCircleSize.Value, MinCircleSize.Value);

            float maxBound = Math.Clamp(maxSize / 10f, 0.175f, 1f);
            float minBound = Math.Clamp(minSize / 10f, 0.175f, 1f);

            float adjustedSize;

            if (IsInverseScaling.Value)
            {
                adjustedSize = Math.Clamp((currentComboValue / (SizeAdjustmentSpeed.Value * (maxSize - minSize))) * (maxBound - minBound) + minBound, minBound, maxBound);
            }
            else
            {
                adjustedSize = Math.Clamp((1 - (currentComboValue / (SizeAdjustmentSpeed.Value * (maxSize - minSize)))) * (maxBound - minBound) + minBound, minBound, maxBound);
            }

            foreach (var entry in playfield.HitObjectContainer.AliveEntries)
            {
                var drawableObject = entry.Value;
                drawableObject.Scale = new Vector2(Math.Clamp(adjustedSize, minBound, maxBound));
                drawableObject.ScaleTo(new Vector2(Math.Clamp(adjustedSize, minBound, maxBound)), 500, Easing.OutElasticQuarter);
            }
        }

        public ScoreRank AdjustRank(ScoreRank rank, double accuracy)
        {
            return rank;
        }
    }
}
