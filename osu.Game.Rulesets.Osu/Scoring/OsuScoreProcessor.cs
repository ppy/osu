// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Scoring
{
    public class OsuScoreProcessor : ScoreProcessor
    {
        protected override HitEvent CreateHitEvent(JudgementResult result)
            => base.CreateHitEvent(result).With((result as OsuHitCircleJudgementResult)?.CursorPositionAtHit);

        protected override JudgementResult CreateResult(HitObject hitObject, Judgement judgement)
        {
            switch (hitObject)
            {
                case HitCircle _:
                    return new OsuHitCircleJudgementResult(hitObject, judgement);

                default:
                    return new OsuJudgementResult(hitObject, judgement);
            }
        }
    }
}
