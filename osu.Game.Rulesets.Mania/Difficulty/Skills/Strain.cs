// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Mania.Difficulty.Evaluators;
using osu.Game.Rulesets.Mania.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Mania.Difficulty.Skills
{
    public class Strain : ManiaStrainSkill
    {
        private const double chord_decay_base = 0.30;
        private const double individual_decay_base = 0.125;

        private double chordDifficulty;
        private double chordStrain;

        private readonly double[] individualStrains;
        private double maxIndividualStrain;

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
            double individualDelta = Math.Max(current.ActualTime - PreviousColumnTimes[current.Column], 25);

            individualStrains[current.Column] = applyDecay(individualStrains[current.Column], individualDelta, individual_decay_base);
            individualStrains[current.Column] += IndividualStrainEvaluator.EvaluateDifficultyOf(current);

            maxIndividualStrain = Math.Max(maxIndividualStrain, individualStrains[current.Column]);

            return maxIndividualStrain + chordStrain;
        }

        protected override void ResetChord()
        {
            chordDifficulty = 0;
            maxIndividualStrain = 0;
        }

        protected override double ReadonlyStrainValueAt(double offset, ManiaDifficultyHitObject current) =>
            applyDecay(maxIndividualStrain, offset - PreviousChordTime, individual_decay_base)
            + applyDecay(chordStrain, offset - PreviousChordTime, chord_decay_base);

        private double applyDecay(double value, double deltaTime, double decayBase)
            => value * Math.Pow(decayBase, deltaTime / 1000);
    }
}
