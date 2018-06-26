// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Taiko.Judgements
{
    public class TaikoIntermediateSwellJudgement : TaikoJudgement
    {
        public override HitResult MaxResult => HitResult.Perfect;

        public override bool AffectsCombo => false;

        public TaikoIntermediateSwellJudgement()
        {
            Final = false;
        }

        /// <summary>
        /// Computes the numeric result value for the combo portion of the score.
        /// </summary>
        /// <param name="result">The result to compute the value for.</param>
        /// <returns>The numeric result value.</returns>
        protected override int NumericResultFor(HitResult result) => 0;
    }
}
