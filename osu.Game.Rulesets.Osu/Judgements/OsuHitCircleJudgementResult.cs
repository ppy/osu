// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;

namespace osu.Game.Rulesets.Osu.Judgements
{
    public class OsuHitCircleJudgementResult : OsuJudgementResult
    {
        /// <summary>
        /// The <see cref="HitCircle"/>.
        /// </summary>
        public HitCircle HitCircle => (HitCircle)HitObject;

        /// <summary>
        /// The position of the player's cursor when <see cref="HitCircle"/> was hit.
        /// </summary>
        public Vector2? CursorPositionAtHit;

        public OsuHitCircleJudgementResult(HitObject hitObject, Judgement judgement)
            : base(hitObject, judgement)
        {
        }
    }
}
