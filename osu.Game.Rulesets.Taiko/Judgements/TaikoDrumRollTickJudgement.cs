// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Rulesets.Taiko.Judgements
{
    public class TaikoDrumRollTickJudgement : TaikoJudgement
    {
        /// <summary>
        /// Drum roll ticks don't display judgement text.
        /// </summary>
        public override string ResultString => string.Empty;

        /// <summary>
        /// Drum roll ticks don't display judgement text.
        /// </summary>
        public override string MaxResultString => string.Empty;

        public override bool AffectsCombo => false;

        protected override int NumericResultForScore(TaikoHitResult result)
        {
            switch (result)
            {
                default:
                    return 0;
                case TaikoHitResult.Great:
                    return 200;
            }
        }

        protected override int NumericResultForAccuracy(TaikoHitResult result)
        {
            return 0;
        }
    }
}