// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Difficulty.Utils;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills.Touch
{
    public class TouchProbability
    {
        public double Probability = 1;
        public readonly RawTouchSkill[] Skills;

        public TouchProbability(RawTouchSkill[] skills)
        {
            Skills = skills;
        }

        public TouchProbability(TouchProbability copy)
        {
            Probability = copy.Probability;
            Skills = new RawTouchSkill[copy.Skills.Length];

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
