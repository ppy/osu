// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Difficulty.Skills
{
    public class Stamina : Skill
    {
        private readonly int hand;

        protected override double SkillMultiplier => 1;
        protected override double StrainDecayBase => 0.4;

        private const int max_history_length = 2;
        private readonly LimitedCapacityQueue<double> notePairDurationHistory = new LimitedCapacityQueue<double>(max_history_length);

        private double offhandObjectDuration = double.MaxValue;

        // Penalty for tl tap or roll
        private double cheesePenalty(double notePairDuration)
        {
            if (notePairDuration > 125) return 1;
            if (notePairDuration < 100) return 0.6;

            return 0.6 + (notePairDuration - 100) * 0.016;
        }

        private double speedBonus(double notePairDuration)
        {
            if (notePairDuration >= 200) return 0;

            double bonus = 200 - notePairDuration;
            bonus *= bonus;
            return bonus / 100000;
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

        public Stamina(bool rightHand)
        {
            hand = rightHand ? 1 : 0;
        }
    }
}
