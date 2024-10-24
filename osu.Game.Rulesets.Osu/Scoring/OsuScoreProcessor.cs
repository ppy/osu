// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Osu.Scoring
{
    public partial class OsuScoreProcessor : ScoreProcessor
    {
        public OsuScoreProcessor()
            : base(new OsuRuleset())
        {
        }

        public override ScoreRank RankFromScore(double accuracy, IReadOnlyDictionary<HitResult, int> results)
            => adjustRank(base.RankFromScore(accuracy, results), results.GetValueOrDefault(HitResult.Miss));

        protected override ScoreRank MinimumRankFromScore(double accuracy, IReadOnlyDictionary<HitResult, int> results)
            // this will be wrong when the remaining judgements do not affect accuracy and the player did not miss before,
            // but it's a bit of a tiny detail to fix. we'll at least show an S/SS rank when the user completes the beatmap
            // (especially when the beatmap has an outro storyboard).
            => adjustRank(base.RankFromScore(accuracy, results), JudgedHits == MaxHits ? results.GetValueOrDefault(HitResult.Miss) : int.MaxValue);

        private ScoreRank adjustRank(ScoreRank rank, int misses)
        {
            switch (rank)
            {
                case ScoreRank.S:
                case ScoreRank.X:
                    if (misses > 0)
                        rank = ScoreRank.A;
                    break;
            }

            return rank;
        }

        protected override HitEvent CreateHitEvent(JudgementResult result)
            => base.CreateHitEvent(result).With((result as OsuHitCircleJudgementResult)?.CursorPositionAtHit);
    }
}
