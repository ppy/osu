using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Difficulty.Skills
{
    public abstract class PrePerNoteStrainSkill : Skill
    {
        protected virtual double DecayWeight => 0.9;
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

            // if the decaybase is smaller than 0, it meant previous notes does not affect next notes 
            if (StrainDecayBase >= 0.0)
                currentStrain *= strainDecay(current.DeltaTime);
            else
                currentStrain = 0;
            currentStrain += value * SkillMultiplier;

            strainPeaks.Add(currentStrain);
        }

        public override double DifficultyValue()
        {
            double difficulty = 0;
            double weight = 1;

            // Difficulty is the weighted sum of the highest strains from every section.
            // We're sorting from highest to lowest strain.
            foreach (double strain in GetAllStrainPeaks().AsEnumerable().OrderByDescending(d => d))
            {
                difficulty += strain * weight;
                weight *= DecayWeight;
            }

            return difficulty;
        }

        protected abstract double StrainValueOf(PrePerNoteStrainSkill[] preSkills, int index, DifficultyHitObject current);
        private double strainDecay(double ms) => Math.Pow(StrainDecayBase, ms / 1000);

        public List<double> GetAllStrainPeaks()
        {
            return strainPeaks;
        }
    }
}
