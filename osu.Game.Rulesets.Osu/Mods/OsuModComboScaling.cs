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
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.Osu.UI.Cursor;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModComboScaling : Mod, IApplicableToDifficulty, IApplicableToScoreProcessor, IUpdatableByPlayfield
    {
        public override string Name => "Combo Scaling";
        public override string Acronym => "CS";
        public override LocalisableString Description => "Circle size adjusts dynamically based on your combo.";
        public override ModType Type => ModType.Fun;
        public override Type[] IncompatibleMods => base.IncompatibleMods.Concat(new[]
        {
            typeof(OsuModAutoplay),
            typeof(ModCinema),
            typeof(ModEasy),
            typeof(ModHardRock),
            typeof(OsuModMagnetised),
            typeof(OsuModRepel),
            typeof(OsuModFreezeFrame),
        }).ToArray();
        public override double ScoreMultiplier => 0.85f;
        public override bool Ranked => false;

        private readonly BindableNumber<int> comboCount = new BindableInt();
        private float currentComboValue;

        [SettingSource("Maximum Circle Size", "The largest size when combo is low.")]
        public BindableFloat MaxCircleSize { get; } = new BindableFloat(10f)
        {
            MinValue = 0f,
            MaxValue = 10f,
            Precision = 0.1f
        };

        [SettingSource("Minimum Circle Size", "The smallest size when combo is high.")]
        public BindableFloat MinCircleSize { get; } = new BindableFloat(1f)
        {
            MinValue = 0f,
            MaxValue = 10f,
            Precision = 0.1f
        };

        [SettingSource("Size Adjustment Speed", "How fast the circle size adjusts.")]
        public BindableFloat SizeAdjustmentSpeed { get; } = new BindableFloat(10f)
        {
            MinValue = 5f,
            MaxValue = 50f,
            Precision = 1f
        };

        [SettingSource("Inverse Scaling", "Reverse the scaling logic.")]
        public BindableBool IsInverseScaling { get; } = new BindableBool(false);

        public void ApplyToDifficulty(BeatmapDifficulty difficulty) => difficulty.CircleSize = 0;

        public void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
        {
            if (scoreProcessor == null) return;
            comboCount.BindTo(scoreProcessor.Combo);
        }

        public void Update(Playfield playfield)
        {
            OsuCursor gameCursor = (OsuCursor)playfield.Cursor.ActiveCursor;
            (playfield as OsuPlayfield)?.FollowPoints.Hide();
            playfield.DisplayJudgements.Value = false;

            comboCount.BindValueChanged(combo =>
            {
                currentComboValue = combo.NewValue;
            }, true);

            float maxSize = Math.Max(MaxCircleSize.Value, MinCircleSize.Value);
            float minSize = Math.Min(MaxCircleSize.Value, MinCircleSize.Value);

            float maxBound = Math.Clamp(maxSize / 10f, 0.175f, 1f);
            float minBound = Math.Clamp(minSize / 10f, 0.175f, 1f);

            float adjustedSize = IsInverseScaling.Value
                ? Math.Clamp(
                    ((currentComboValue / (SizeAdjustmentSpeed.Value * (maxSize - minSize))) * (maxBound - minBound)) + minBound,
                    minBound, maxBound)
                : Math.Clamp(
                    ((1 - (currentComboValue / (SizeAdjustmentSpeed.Value * (maxSize - minSize)))) * (maxBound - minBound)) + minBound,
                    minBound, maxBound);

            foreach (var entry in playfield.HitObjectContainer.AliveEntries)
            {
                var drawableObject = entry.Value;
                float scale = Math.Clamp(adjustedSize, minBound, maxBound);

                gameCursor.Scale = new Vector2(scale);
                drawableObject.Scale = new Vector2(scale);
                drawableObject.ScaleTo(new Vector2(scale), 500, Easing.OutElasticQuarter);
            }
        }

        public ScoreRank AdjustRank(ScoreRank rank, double accuracy) => rank;
    }
}
