// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Difficulty.Skills
{
    /// <summary>
    /// Calculates the stamina coefficient of taiko difficulty.
    /// </summary>
    /// <remarks>
    /// The reference play style chosen uses two hands, with full alternating (the hand changes after every hit).
    /// </remarks>
    public class Stamina : Skill
    {
        protected override double SkillMultiplier => 1;
        protected override double StrainDecayBase => 0.4;

        /// <summary>
        /// Maximum number of entries to keep in <see cref="notePairDurationHistory"/>.
        /// </summary>
        private const int max_history_length = 2;

        /// <summary>
        /// The index of the hand this <see cref="Stamina"/> instance is associated with.
        /// </summary>
        /// <remarks>
        /// The value of 0 indicates the left hand (full alternating gameplay starting with left hand is assumed).
        /// This naturally translates onto index offsets of the objects in the map.
        /// </remarks>
        private readonly int hand;

        /// <summary>
        /// Stores the last <see cref="max_history_length"/> durations between notes hit with the hand indicated by <see cref="hand"/>.
        /// </summary>
        private readonly LimitedCapacityQueue<double> notePairDurationHistory = new LimitedCapacityQueue<double>(max_history_length);

        /// <summary>
        /// Stores the <see cref="DifficultyHitObject.DeltaTime"/> of the last object that was hit by the <i>other</i> hand.
        /// </summary>
        private double offhandObjectDuration = double.MaxValue;

        /// <summary>
        /// Creates a <see cref="Stamina"/> skill.
        /// </summary>
        /// <param name="mods">Mods for use in skill calculations.</param>
        /// <param name="rightHand">Whether this instance is performing calculations for the right hand.</param>
        public Stamina(Mod[] mods, bool rightHand)
            : base(mods)
        {
            hand = rightHand ? 1 : 0;
        }

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            if (!(current.BaseObject is Hit))
            {
                return 0.0;
            }

            TaikoDifficultyHitObject hitObject = (TaikoDifficultyHitObject)current;

            if (hitObject.ObjectIndex % 2 == hand)
            {
                double objectStrain = 1;

                if (hitObject.ObjectIndex == 1)
                    return 1;

                notePairDurationHistory.Enqueue(hitObject.DeltaTime + offhandObjectDuration);

                double shortestRecentNote = notePairDurationHistory.Min();
                objectStrain += speedBonus(shortestRecentNote);

                if (hitObject.StaminaCheese)
                    objectStrain *= cheesePenalty(hitObject.DeltaTime + offhandObjectDuration);

                return objectStrain;
            }

            offhandObjectDuration = hitObject.DeltaTime;
            return 0;
        }

        /// <summary>
        /// Applies a penalty for hit objects marked with <see cref="TaikoDifficultyHitObject.StaminaCheese"/>.
        /// </summary>
        /// <param name="notePairDuration">The duration between the current and previous note hit using the hand indicated by <see cref="hand"/>.</param>
        private double cheesePenalty(double notePairDuration)
        {
            if (notePairDuration > 125) return 1;
            if (notePairDuration < 100) return 0.6;

            return 0.6 + (notePairDuration - 100) * 0.016;
        }

        /// <summary>
        /// Applies a speed bonus dependent on the time since the last hit performed using this hand.
        /// </summary>
        /// <param name="notePairDuration">The duration between the current and previous note hit using the hand indicated by <see cref="hand"/>.</param>
        private double speedBonus(double notePairDuration)
        {
            if (notePairDuration >= 200) return 0;

            double bonus = 200 - notePairDuration;
            bonus *= bonus;
            return bonus / 100000;
        }
    }
}
