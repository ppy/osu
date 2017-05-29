// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Mania.Judgements
{
    public class ManiaJudgement : Judgement
    {
        /// <summary>
        /// The maximum possible hit result.
        /// </summary>
        public const ManiaHitResult MAX_HIT_RESULT = ManiaHitResult.Perfect;

        /// <summary>
        /// The result value for the combo portion of the score.
        /// </summary>
        public int ResultValueForScore => Result == HitResult.Miss ? 0 : NumericResultForScore(ManiaResult);

        /// <summary>
        /// The result value for the accuracy portion of the score.
        /// </summary>
        public int ResultValueForAccuracy => Result == HitResult.Miss ? 0 : NumericResultForAccuracy(ManiaResult);

        /// <summary>
        /// The maximum result value for the combo portion of the score.
        /// </summary>
        public int MaxResultValueForScore => NumericResultForScore(MAX_HIT_RESULT);

        /// <summary>
        /// The maximum result value for the accuracy portion of the score.
        /// </summary>
        public int MaxResultValueForAccuracy => NumericResultForAccuracy(MAX_HIT_RESULT);

        public override string ResultString => string.Empty;

        public override string MaxResultString => string.Empty;

        /// <summary>
        /// The hit result.
        /// </summary>
        public ManiaHitResult ManiaResult;

        public virtual int NumericResultForScore(ManiaHitResult result)
        {
            switch (result)
            {
                default:
                    return 0;
                case ManiaHitResult.Bad:
                    return 50;
                case ManiaHitResult.Ok:
                    return 100;
                case ManiaHitResult.Good:
                    return 200;
                case ManiaHitResult.Great:
                case ManiaHitResult.Perfect:
                    return 300;
            }
        }

        public virtual int NumericResultForAccuracy(ManiaHitResult result)
        {
            switch (result)
            {
                default:
                    return 0;
                case ManiaHitResult.Bad:
                    return 50;
                case ManiaHitResult.Ok:
                    return 100;
                case ManiaHitResult.Good:
                    return 200;
                case ManiaHitResult.Great:
                    return 300;
                case ManiaHitResult.Perfect:
                    return 305;
            }
        }
    }
}
