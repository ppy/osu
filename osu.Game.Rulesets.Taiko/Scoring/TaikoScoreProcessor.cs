// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Judgements;

namespace osu.Game.Rulesets.Taiko.Scoring
{
    internal class TaikoScoreProcessor : ScoreProcessor
    {
        protected override double DefaultAccuracyPortion => 0.75;

        protected override double DefaultComboPortion => 0.25;

        public override HitWindows CreateHitWindows() => new TaikoHitWindows();

        protected override int GetNumericBonusResult(HitResult result)
        {
            switch (result)
            {
                case HitResult.SmallBonusHit:
                    return TaikoJudgement.SMALL_BONUS_RESULT;

                case HitResult.LargeBonusHit:
                    return TaikoJudgement.LARGE_BONUS_RESULT;
            }

            return 0;
        }
    }
}
