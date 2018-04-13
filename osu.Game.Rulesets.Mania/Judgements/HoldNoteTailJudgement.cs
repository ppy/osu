// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mania.Judgements
{
    public class HoldNoteTailJudgement : ManiaJudgement
    {
        /// <summary>
        /// Whether the hold note has been released too early and shouldn't give full score for the release.
        /// </summary>
        public bool HasBroken;

        protected override int NumericResultFor(HitResult result)
        {
            switch (result)
            {
                default:
                    return base.NumericResultFor(result);
                case HitResult.Great:
                case HitResult.Perfect:
                    return base.NumericResultFor(HasBroken ? HitResult.Good : result);
            }
        }
    }
}
