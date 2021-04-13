// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    public class Tapping : OsuSkill
    {
        private const double skill_multiplier = 1.0;

        private Speed speedSkill;
        private Stamina staminaSkill;

        public double TappingStrain { get; private set; }

        public Tapping(Mod[] mods) : base(mods)
        {
            speedSkill = new Speed(mods);
            staminaSkill = new Stamina(mods);
        }

        public override double DifficultyValue()
        {
            List<double> speedStrains = speedSkill.GetCurrentStrainPeaks().ToList();
            List<double> staminaStrains = staminaSkill.GetCurrentStrainPeaks().ToList();

            for (int i = 0; i < speedStrains.Count(); i++)
            {
                AddStrain(Math.Max(speedStrains[i], staminaStrains[i]) * skill_multiplier);
            }

            return base.DifficultyValue();
        }

        protected override void Process(DifficultyHitObject current)
        {
            //Console.WriteLine(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1:#.##},{2:#.##}", current.StartTime, speedSkill.TappingStrain, staminaSkill.TappingStrain));

            TappingStrain = Math.Max(speedSkill.TappingStrain, staminaSkill.TappingStrain) * skill_multiplier;
        }

        internal void SetStrainSkills(Speed speed, Stamina stamina)
        {
            speedSkill = speed;
            staminaSkill = stamina;
        }
    }
}
