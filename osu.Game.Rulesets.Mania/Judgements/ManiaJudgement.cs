// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Extensions;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Mania.Judgements
{
    public class ManiaJudgement : Judgement
    {
        /// <summary>
        /// The maximum result value for the accuracy portion of the score.
        /// </summary>
        public int MaxNumericAccuracyResult => NumericResultForAccuracy(HitResult.Perfect);

        protected override int NumericResultFor(HitResult result)
        {
            switch (result)
            {
                default:
                    return 0;
                case HitResult.Meh:
                    return 50;
                case HitResult.Ok:
                    return 100;
                case HitResult.Good:
                    return 200;
                case HitResult.Great:
                case HitResult.Perfect:
                    return 300;
            }
        }

        public int NumericAccuracyResult => NumericResultForAccuracy(Result);

        /// <summary>
        /// The result value for the accuracy portion of the score.
        /// </summary>
        protected virtual int NumericResultForAccuracy(HitResult result)
        {
            switch (result)
            {
                default:
                    return 0;
                case HitResult.Meh:
                    return 50;
                case HitResult.Ok:
                    return 100;
                case HitResult.Good:
                    return 200;
                case HitResult.Great:
                    return 300;
                case HitResult.Perfect:
                    return 305;
            }
        }
    }
}
