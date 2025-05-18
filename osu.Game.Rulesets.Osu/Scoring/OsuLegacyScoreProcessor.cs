// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Legacy;
using osu.Game.Rulesets.Osu.Difficulty;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Scoring.Legacy;
using osu.Game.Scoring.Legacy;

namespace osu.Game.Rulesets.Osu.Scoring
{
    public class OsuLegacyScoreProcessor : ILegacyScoreProcessor
    {
        private double difficultyMultiplier, modMultuplier = 1.0;

        public void ApplyBeatmap(IBeatmap beatmap)
        {
            int objectCount = beatmap.HitObjects.Count;
            int drainLength = 0;

            if (objectCount > 0)
            {
                int breakLength = beatmap.Breaks.Select(b => (int)Math.Round(b.EndTime) - (int)Math.Round(b.StartTime)).Sum();
                drainLength = ((int)Math.Round(beatmap.HitObjects[^1].StartTime) - (int)Math.Round(beatmap.HitObjects[0].StartTime) - breakLength) / 1000;
            }

            difficultyMultiplier = LegacyRulesetExtensions.CalculateDifficultyPeppyStars(beatmap.Difficulty, objectCount, drainLength);
        }

        public void ApplyMods(IReadOnlyList<Mod> mods)
        {
            var simulator = new OsuLegacyScoreSimulator();
            modMultuplier = simulator.GetLegacyScoreMultiplier(mods, new LegacyBeatmapConversionDifficultyInfo());
        }

        private long getBaseScore(JudgementResult result)
        {
            switch (result.Type)
            {
                case HitResult.Miss:
                    return 0;

                case HitResult.Meh:
                    return 50;

                case HitResult.Ok:
                    return 100;

                case HitResult.Great:
                    return 300;

                default:
                    return 0;
            }
        }

        private long getBonusScore(JudgementResult result)
        {
            switch (result.HitObject)
            {
                case Slider:
                case SliderHeadCircle:
                case SliderTailCircle:
                case SliderRepeat:
                    return 30;

                case SliderTick:
                    return 10;

                case SpinnerBonusTick:
                    return 1100;

                case SpinnerTick:
                    return 100;

                default:
                    return 0;
            }
        }

        private long getScoreForNormalResult(JudgementResult result)
        {
            double baseScore = getBaseScore(result);

            // WARNING: for some reason at the end of the slider combo is not increased before this is called
            int combo = result.ComboAtJudgement;
            if (result.HitObject is Slider) combo++;

            // The combo multiplier is equal to (combo before this hit - 1) or 0, whichever is higher.
            double comboMultiplier = Math.Max(combo - 1, 0);

            // Score = Hit value * (1 + (Combo multiplier * Difficulty multiplier * Mod multiplier / 25))
            double multiplier = comboMultiplier * difficultyMultiplier * modMultuplier / 25;
            double score = baseScore * (1 + multiplier);

            return (long)Math.Round(score);
        }

        public long GetScoreForResult(JudgementResult result)
        {
            if (result.FailedAtJudgement) return 0;

            if (result.Type == HitResult.Meh || result.Type == HitResult.Ok || result.Type == HitResult.Great)
                return getScoreForNormalResult(result);

            return getBonusScore(result);
        }
    }
}
