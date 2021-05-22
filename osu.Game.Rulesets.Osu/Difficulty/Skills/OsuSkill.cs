// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    public abstract class OsuSkill : Skill
    {
        private readonly List<double> strains = new List<double>();
        private readonly List<double> times = new List<double>();
        private double target_fc_precision = 0.01;
        private double target_fc_time = 3600*1000;

        protected virtual double StarsPerDouble => 1.15;

        private double difficultyExponent => 1.0 / Math.Log(StarsPerDouble, 2);

        protected OsuSkill(Mod[] mods) : base(mods)
        {
        }

        protected abstract double strainValueAt(DifficultyHitObject current);

        protected override void Process(DifficultyHitObject current)
        {
            strains.Add(strainValueAt(current));
            times.Add(current.StartTime);
        }

        private double calculateDifficultyValue()
        {
            double difficultyExponent = 1.0 / Math.Log(StarsPerDouble, 2);
            double SR = 0;

            // double avgStrain = 0;
            // for (int i = 0; i < strains.Count; i++)
            // {
            //     avgStrain += strains[i];
            // }
            //
            // avgStrain /= strains.Count;
            //
            // for (int i = 0; i < strains.Count; i++)
            // {
            //     strains[i] += avgStrain;
            // }

            for (int i = 0; i < strains.Count; i++)
            {
                SR += Math.Pow(strains[i], difficultyExponent);
            }

            return Math.Pow(SR, 1.0 / difficultyExponent);
        }

        public override double DifficultyValue()
        {
            return fcTimeSkillLevel(calculateDifficultyValue());
        }

        /// <summary>
        /// The probability a player of the given skill full combos a map of the given difficulty.
        /// </summary>
        /// <param name="skill">The skill level of the player.</param>
        /// <param name="difficulty">The difficulty of a range of notes.</param>
        private double fcProbability(double skill, double difficulty) => Math.Exp(-Math.Pow(difficulty / Math.Max(1e-10, skill), difficultyExponent));


        /// <summary>
        /// Approximates the skill level of a player that can FC a map with the given <paramref name="difficulty"/>,
        /// if their probability of success in doing so is equal to <paramref name="probability"/>.
        /// </summary>
        private double skillLevel(double probability, double difficulty) => difficulty * Math.Pow(-Math.Log(probability), -1 / difficultyExponent);

        private double expectedFcTime(double skill)
        {
            double last_timestamp = times[0]-5; // time taken to retry map
            double fcTime = 0;

            for (int i=0;i<strains.Count;i++)
            {
                double dt = times[i]-last_timestamp;
                last_timestamp = times[i];
                fcTime = (fcTime + dt) / fcProbability(skill, strains[i]);
            }
            return fcTime - (times[times.Count - 1] - times[0]);
        }

        private double fcTimeSkillLevel(double totalDifficulty)
        {
            double lengthEstimate = 0.4 * (times[times.Count - 1] - times[0]);
            target_fc_time += (times[times.Count - 1] - times[0]);
            double fcProb = lengthEstimate / target_fc_time;
            double skill = skillLevel(fcProb, totalDifficulty);
            for (int i=0; i<5; ++i)
            {
                double fcTime = expectedFcTime(skill);
                lengthEstimate = fcTime * fcProb;
                fcProb = lengthEstimate / target_fc_time;
                skill = skillLevel(fcProb, totalDifficulty);
                if (Math.Abs(fcTime - target_fc_time) < target_fc_precision * target_fc_time)
                {
                    //enough precision
                    break;
                }
            }
            return skill;
        }
    }
}
