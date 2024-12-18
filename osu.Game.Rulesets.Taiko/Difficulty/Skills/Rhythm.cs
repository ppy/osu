// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Utils;

namespace osu.Game.Rulesets.Taiko.Difficulty.Skills
{
    /// <summary>
    /// Calculates the rhythm coefficient of taiko difficulty.
    /// </summary>
    public class Rhythm : StrainDecaySkill
    {
        protected override double SkillMultiplier => 10;
        protected override double StrainDecayBase => 0;

        /// <summary>
        /// The note-based decay for rhythm strain.
        /// </summary>
        /// <remarks>
        /// <see cref="StrainDecayBase"/> is not used here, as it's time- and not note-based.
        /// </remarks>
        private const double strain_decay = 0.96;

        /// <summary>
        /// Maximum number of entries in <see cref="rhythmHistory"/>.
        /// </summary>
        private const int rhythm_history_max_length = 8;

        /// <summary>
        /// Contains the last <see cref="rhythm_history_max_length"/> changes in note sequence rhythms.
        /// </summary>
        private readonly LimitedCapacityQueue<TaikoDifficultyHitObject> rhythmHistory = new LimitedCapacityQueue<TaikoDifficultyHitObject>(rhythm_history_max_length);

        /// <summary>
        /// Contains the rolling rhythm strain.
        /// Used to apply per-note decay.
        /// </summary>
        private double currentStrain;

        /// <summary>
        /// Number of notes since the last rhythm change has taken place.
        /// </summary>
        private int notesSinceRhythmChange;

        public Rhythm(Mod[] mods)
            : base(mods)
        {
        }

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            // drum rolls and swells are exempt.
            if (!(current.BaseObject is Hit))
            {
                resetRhythmAndStrain();
                return 0.0;
            }

            currentStrain *= strain_decay;

            TaikoDifficultyHitObject hitObject = (TaikoDifficultyHitObject)current;
            notesSinceRhythmChange += 1;

            // rhythm difficulty zero (due to rhythm not changing) => no rhythm strain.
            if (hitObject.Rhythm.Difficulty == 0.0)
            {
                return 0.0;
            }

            double objectStrain = hitObject.Rhythm.Difficulty;

            objectStrain *= repetitionPenalties(hitObject);
            objectStrain *= patternLengthPenalty(notesSinceRhythmChange);
            objectStrain *= speedPenalty(hitObject.DeltaTime);

            // careful - needs to be done here since calls above read this value
            notesSinceRhythmChange = 0;

            currentStrain += objectStrain;
            return currentStrain;
        }

        /// <summary>
        /// Returns a penalty to apply to the current hit object caused by repeating rhythm changes.
        /// </summary>
        /// <remarks>
        /// Repetitions of more recent patterns are associated with a higher penalty.
        /// </remarks>
        /// <param name="hitObject">The current hit object being considered.</param>
        private double repetitionPenalties(TaikoDifficultyHitObject hitObject)
        {
            double penalty = 1;

            rhythmHistory.Enqueue(hitObject);

            for (int mostRecentPatternsToCompare = 2; mostRecentPatternsToCompare <= rhythm_history_max_length / 2; mostRecentPatternsToCompare++)
            {
                for (int start = rhythmHistory.Count - mostRecentPatternsToCompare - 1; start >= 0; start--)
                {
                    if (!samePattern(start, mostRecentPatternsToCompare))
                        continue;

                    int notesSince = hitObject.Index - rhythmHistory[start].Index;
                    penalty *= repetitionPenalty(notesSince);
                    break;
                }
            }

            return penalty;
        }

        /// <summary>
        /// Determines whether the rhythm change pattern starting at <paramref name="start"/> is a repeat of any of the
        /// <paramref name="mostRecentPatternsToCompare"/>.
        /// </summary>
        private bool samePattern(int start, int mostRecentPatternsToCompare)
        {
            for (int i = 0; i < mostRecentPatternsToCompare; i++)
            {
                if (rhythmHistory[start + i].Rhythm != rhythmHistory[rhythmHistory.Count - mostRecentPatternsToCompare + i].Rhythm)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Calculates a single rhythm repetition penalty.
        /// </summary>
        /// <param name="notesSince">Number of notes since the last repetition of a rhythm change.</param>
        private static double repetitionPenalty(int notesSince) => Math.Min(1.0, 0.032 * notesSince);

        /// <summary>
        /// Calculates a penalty based on the number of notes since the last rhythm change.
        /// Both rare and frequent rhythm changes are penalised.
        /// </summary>
        /// <param name="patternLength">Number of notes since the last rhythm change.</param>
        private static double patternLengthPenalty(int patternLength)
        {
            double shortPatternPenalty = Math.Min(0.15 * patternLength, 1.0);
            double longPatternPenalty = Math.Clamp(2.5 - 0.15 * patternLength, 0.0, 1.0);
            return Math.Min(shortPatternPenalty, longPatternPenalty);
        }

        /// <summary>
        /// Calculates a penalty for objects that do not require alternating hands.
        /// </summary>
        /// <param name="deltaTime">Time (in milliseconds) since the last hit object.</param>
        private double speedPenalty(double deltaTime)
        {
            if (deltaTime < 80) return 1;
            if (deltaTime < 210) return Math.Max(0, 1.4 - 0.005 * deltaTime);

            resetRhythmAndStrain();
            return 0.0;
        }

        /// <summary>
        /// Resets the rolling strain value and <see cref="notesSinceRhythmChange"/> counter.
        /// </summary>
        private void resetRhythmAndStrain()
        {
            currentStrain = 0.0;
            notesSinceRhythmChange = 0;
        }
    }
}
