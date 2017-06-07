// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Rulesets.Mania.Judgements
{
    public class HoldNoteTailJudgement : ManiaJudgement
    {
        /// <summary>
        /// Whether the hold note has been released too early and shouldn't give full score for the release.
        /// </summary>
        public bool HasBroken;

        public override int NumericResultForScore(ManiaHitResult result)
        {
            switch (result)
            {
                default:
                    return base.NumericResultForScore(result);
                case ManiaHitResult.Great:
                case ManiaHitResult.Perfect:
                    return base.NumericResultForScore(HasBroken ? ManiaHitResult.Good : result);
            }
        }

        public override int NumericResultForAccuracy(ManiaHitResult result)
        {
            switch (result)
            {
                default:
                    return base.NumericResultForAccuracy(result);
                case ManiaHitResult.Great:
                case ManiaHitResult.Perfect:
                    return base.NumericResultForAccuracy(HasBroken ? ManiaHitResult.Good : result);
            }
        }
    }
}