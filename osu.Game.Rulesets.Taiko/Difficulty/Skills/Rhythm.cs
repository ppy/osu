// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Difficulty.Skills
{
    public class Rhythm : Skill
    {
        protected override double SkillMultiplier => 10;
        protected override double StrainDecayBase => 0;
        private const double strain_decay = 0.96;
        private double currentStrain;

        private readonly LimitedCapacityQueue<TaikoDifficultyHitObject> rhythmHistory = new LimitedCapacityQueue<TaikoDifficultyHitObject>(rhythm_history_max_length);
        private const int rhythm_history_max_length = 8;

        private int notesSinceRhythmChange;

        private double repetitionPenalty(int notesSince)
        {
            return Math.Min(1.0, 0.032 * notesSince);
        }

        // Finds repetitions and applies penalties
        private double repetitionPenalties(TaikoDifficultyHitObject hitobject)
        {
            double penalty = 1;

            rhythmHistory.Enqueue(hitobject);

            for (int l = 2; l <= rhythm_history_max_length / 2; l++)
            {
                for (int start = rhythmHistory.Count - l - 1; start >= 0; start--)
                {
                    if (!samePattern(start, l))
                        continue;

                    int notesSince = hitobject.ObjectIndex - rhythmHistory[start].ObjectIndex;
                    penalty *= repetitionPenalty(notesSince);
                    break;
                }
            }

            return penalty;
        }

        private bool samePattern(int start, int l)
        {
            for (int i = 0; i < l; i++)
            {
                if (rhythmHistory[start + i].Rhythm != rhythmHistory[rhythmHistory.Count - l + i].Rhythm)
                    return false;
            }

            return true;
        }

        private double patternLengthPenalty(int patternLength)
        {
            double shortPatternPenalty = Math.Min(0.15 * patternLength, 1.0);
            double longPatternPenalty = Math.Clamp(2.5 - 0.15 * patternLength, 0.0, 1.0);
            return Math.Min(shortPatternPenalty, longPatternPenalty);
        }

        // Penalty for notes so slow that alternating is not necessary.
        private double speedPenalty(double noteLengthMs)
        {
            if (noteLengthMs < 80) return 1;
            if (noteLengthMs < 210) return Math.Max(0, 1.4 - 0.005 * noteLengthMs);

            resetRhythmStrain();
            return 0.0;
        }

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            if (!(current.BaseObject is Hit))
            {
                resetRhythmStrain();
                return 0.0;
            }

            currentStrain *= strain_decay;

            TaikoDifficultyHitObject hitobject = (TaikoDifficultyHitObject)current;
            notesSinceRhythmChange += 1;

            if (hitobject.Rhythm.Difficulty == 0.0)
            {
                return 0.0;
            }

            double objectStrain = hitobject.Rhythm.Difficulty;

            objectStrain *= repetitionPenalties(hitobject);
            objectStrain *= patternLengthPenalty(notesSinceRhythmChange);
            objectStrain *= speedPenalty(hitobject.DeltaTime);

            notesSinceRhythmChange = 0;

            currentStrain += objectStrain;
            return currentStrain;
        }

        private void resetRhythmStrain()
        {
            currentStrain = 0.0;
            notesSinceRhythmChange = 0;
        }
    }
}
