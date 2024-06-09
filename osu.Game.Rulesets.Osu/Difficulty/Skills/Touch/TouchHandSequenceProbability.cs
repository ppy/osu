// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Difficulty.Utils;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills.Touch
{
    /// <summary>
    /// Represents the probability of a hand sequence to be taken by the player in Touch Device gameplay
    /// environment to hit <see cref="OsuDifficultyHitObject"/>s that have been processed up to this point.
    /// </summary>
    public class TouchHandSequenceProbability
    {
        /// <summary>
        /// The probability that the player performs the hand sequence represented by this <see cref="TouchHandSequenceProbability"/>.
        /// </summary>
        public double Probability = 1;

        /// <summary>
        /// The <see cref="TouchHandSequenceSkill"/>s that influence the probability of the hand sequence
        /// represented by this <see cref="TouchHandSequenceProbability"/> to be taken by the player.
        /// </summary>
        public readonly TouchHandSequenceSkill[] Skills;

        public TouchHandSequenceProbability(TouchHandSequenceSkill[] skills)
        {
            Skills = skills;
        }

        public TouchHandSequenceProbability(TouchHandSequenceProbability copy)
        {
            Probability = copy.Probability;
            Skills = new TouchHandSequenceSkill[copy.Skills.Length];

            for (int i = 0; i < Skills.Length; i++)
                Skills[i] = copy.Skills[i].DeepClone();
        }

        public void Process(OsuDifficultyHitObject current, TouchHand currentHand)
        {
            foreach (var skill in Skills)
                skill.Process(current, currentHand);
        }
    }
}
