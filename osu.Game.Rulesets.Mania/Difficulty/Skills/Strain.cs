// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Game.Rulesets.Mania.Difficulty.Evaluators;
using osu.Game.Rulesets.Mania.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Mania.Difficulty.Skills
{
    public class Strain : ManiaSkill
    {
        private const double chord_decay_base = 0.30;
        private const double individual_decay_base = 0.125;

        private double chordDifficulty;
        private double chordStrain;

        private readonly double[] individualStrains;

        public Strain(Mod[] mods, int totalColumns)
            : base(mods, totalColumns)
        {
            individualStrains = new double[totalColumns];
            chordStrain = 1;
        }

        protected override void PreprocessChordNote(ManiaDifficultyHitObject current)
        {
            chordDifficulty += OverallStrainEvaluator.EvaluateDifficultyOf(current);
        }

        protected override void FinalizeChord()
        {
            double chordDelta = CurrentChordTime - PreviousChordTime;

            chordStrain = applyDecay(chordStrain, chordDelta, chord_decay_base);
            chordStrain += chordDifficulty;
        }

        protected override double StrainValueAt(ManiaDifficultyHitObject current)
        {
            if (current.BaseObject is TailNote)
                return 0;

            double individualDelta = current.ActualTime - PreviousColumnTimes[current.Column];

            individualStrains[current.Column] = applyDecay(individualStrains[current.Column], individualDelta, individual_decay_base);
            individualStrains[current.Column] += IndividualStrainEvaluator.EvaluateDifficultyOf(current);

            return individualStrains[current.Column] + chordStrain;
        }

        protected override void ResetChord()
        {
            chordDifficulty = 0;
        }

        // protected override double CalculateInitialStrain(double offset, DifficultyHitObject current) =>
        //     applyDecay(highestIndividualStrain, offset - current.Previous(0).StartTime, individual_decay_base)
        //     + applyDecay(chordStrain, offset - current.Previous(0).StartTime, overall_decay_base);

        private double applyDecay(double value, double deltaTime, double decayBase)
            => value * Math.Pow(decayBase, deltaTime / 1000);

        public override double DifficultyValue()
        {
            return ObjectDifficulties.Count > 0 ? ObjectDifficulties.Average() : 0;
        }
    }
}
