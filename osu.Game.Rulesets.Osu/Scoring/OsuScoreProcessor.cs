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
            => adjustRankFromMisses(base.RankFromScore(accuracy, results), results.GetValueOrDefault(HitResult.Miss));

        protected override ScoreRank MinimumRankFromScore(double accuracy, IReadOnlyDictionary<HitResult, int> results)
        {
            // when computing minimum rank in osu!, always assume the player has missed...
            int misses = int.MaxValue;

            // ...unless the player reached the end, at which point show the minimum rank with the player's misses count.
            // this gives the effect where minimum rank becomes equal to actual rank when the player finishes the beatmap.
            if (JudgedHits == MaxHits)
                misses = results.GetValueOrDefault(HitResult.Miss);

            return adjustRankFromMisses(base.RankFromScore(accuracy, results), misses);
        }

        private ScoreRank adjustRankFromMisses(ScoreRank rank, int misses)
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
