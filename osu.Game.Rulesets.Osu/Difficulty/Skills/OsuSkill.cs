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
        protected virtual double StarsPerDouble => 1.15;

        protected OsuSkill(Mod[] mods) : base(mods)
        {
        }

        private double calculateDifficultyValue()
        {
            double k = 1.0 / Math.Log(StarsPerDouble, 2);
            double SR = 0;


            for (int i = 0; i < strains.Count; i++)
            {
                SR += Math.Pow(strains[i], k);
            }

            return Math.Pow(SR, 1.0 / k);
        }

        public override double DifficultyValue()
        {
            return calculateDifficultyValue();
        }

        protected void AddStrain(double strain)
        {
            strains.Add(strain);
        }
    }
}
