using System;
using System.Collections.Generic;
using System.Text;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Difficulty.Skills
{
    public abstract class PrePerNoteStrainSkill : Skill
    {
        protected readonly IBeatmap beatmap;
        protected readonly double clockRate;
        protected readonly List<double> strainPeaks = new List<double>();

        /// <summary>
        /// Strain values are multiplied by this number for the given skill. Used to balance the value of different skills between each other.
        /// </summary>
        protected abstract double SkillMultiplier { get; }

        /// <summary>
        /// Determines how quickly strain decays for the given skill.
        /// For example a value of 0.15 indicates that strain decays to 15% of its original value in one second.
        /// </summary>
        protected abstract double StrainDecayBase { get; }

        public PrePerNoteStrainSkill(IBeatmap beatmap, Mod[] mods, double clockRate) : base(mods)
        {
            this.beatmap = beatmap;
        }

        private double currentStrain = 0;

        protected override void Process(PrePerNoteStrainSkill[] preSkills, int index, DifficultyHitObject current)
        {
            double value = StrainValueOf(preSkills, index, current);

            if (StrainDecayBase >= 0.0)
                currentStrain *= strainDecay(current.DeltaTime);
            else
                currentStrain = 0;
            currentStrain += value * SkillMultiplier;

            strainPeaks.Add(currentStrain);
        }

        public override double DifficultyValue()
        {
            return 0;
        }

        protected abstract double StrainValueOf(PrePerNoteStrainSkill[] preSkills, int index, DifficultyHitObject current);
        private double strainDecay(double ms) => Math.Pow(StrainDecayBase, ms / 1000);

        public List<double> GetAllStrainPeaks()
        {
            return strainPeaks;
        }
    }
}
