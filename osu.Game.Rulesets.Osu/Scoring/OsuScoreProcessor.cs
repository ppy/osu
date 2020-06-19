// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osuTK;

namespace osu.Game.Rulesets.Osu.Scoring
{
    public class OsuScoreProcessor : ScoreProcessor
    {
        private readonly List<HitEvent> hitEvents = new List<HitEvent>();
        private HitObject lastHitObject;

        protected override void OnResultApplied(JudgementResult result)
        {
            base.OnResultApplied(result);

            hitEvents.Add(new HitEvent(result.TimeOffset, result.Type, result.HitObject, lastHitObject, (result as OsuHitCircleJudgementResult)?.HitPosition));
            lastHitObject = result.HitObject;
        }

        protected override void OnResultReverted(JudgementResult result)
        {
            base.OnResultReverted(result);

            hitEvents.RemoveAt(hitEvents.Count - 1);
        }

        protected override void Reset(bool storeResults)
        {
            base.Reset(storeResults);

            hitEvents.Clear();
            lastHitObject = null;
        }

        public override void PopulateScore(ScoreInfo score)
        {
            base.PopulateScore(score);

            score.HitEvents.AddRange(hitEvents.Select(e => e).Cast<object>());
        }

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

        public override HitWindows CreateHitWindows() => new OsuHitWindows();
    }

    public readonly struct HitEvent
    {
        /// <summary>
        /// The time offset from the end of <see cref="HitObject"/> at which the event occurred.
        /// </summary>
        public readonly double TimeOffset;

        /// <summary>
        /// The hit result.
        /// </summary>
        public readonly HitResult Result;

        /// <summary>
        /// The <see cref="HitObject"/> on which the result occurred.
        /// </summary>
        public readonly HitObject HitObject;

        /// <summary>
        /// The <see cref="HitObject"/> occurring prior to <see cref="HitObject"/>.
        /// </summary>
        [CanBeNull]
        public readonly HitObject LastHitObject;

        /// <summary>
        /// The player's position offset, if available, at the time of the event.
        /// </summary>
        public readonly Vector2? PositionOffset;

        public HitEvent(double timeOffset, HitResult result, HitObject hitObject, [CanBeNull] HitObject lastHitObject, Vector2? positionOffset)
        {
            TimeOffset = timeOffset;
            Result = result;
            HitObject = hitObject;
            LastHitObject = lastHitObject;
            PositionOffset = positionOffset;
        }
    }
}
