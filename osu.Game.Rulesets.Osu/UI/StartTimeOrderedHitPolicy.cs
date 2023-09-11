// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Osu.UI
{
    /// <summary>
    /// Ensures that <see cref="HitObject"/>s are hit in-order of their start times. Affectionately known as "note lock".
    /// If a <see cref="HitObject"/> is hit out of order:
    /// <list type="number">
    /// <item><description>The hit is blocked if it occurred earlier than the previous <see cref="HitObject"/>'s start time.</description></item>
    /// <item><description>The hit causes all previous <see cref="HitObject"/>s to missed otherwise.</description></item>
    /// </list>
    /// </summary>
    public class StartTimeOrderedHitPolicy : IHitPolicy
    {
        public IHitObjectContainer? HitObjectContainer { get; set; }

        public ClickAction CheckHittable(DrawableHitObject hitObject, double time, HitResult _)
        {
            if (HitObjectContainer == null)
                throw new InvalidOperationException($"{nameof(HitObjectContainer)} should be set before {nameof(CheckHittable)} is called.");

            DrawableHitObject? blockingObject = null;

            foreach (var obj in enumerateHitObjectsUpTo(hitObject.HitObject.StartTime))
            {
                if (hitObjectCanBlockFutureHits(obj))
                    blockingObject = obj;
            }

            // If there is no previous hitobject, allow the hit.
            if (blockingObject == null)
                return ClickAction.Hit;

            // A hit is allowed if:
            // 1. The last blocking hitobject has been judged.
            // 2. The current time is after the last hitobject's start time.
            // Hits at exactly the same time as the blocking hitobject are allowed for maps that contain simultaneous hitobjects (e.g. /b/372245).
            return (blockingObject.Judged || time >= blockingObject.HitObject.StartTime) ? ClickAction.Hit : ClickAction.Shake;
        }

        public void HandleHit(DrawableHitObject hitObject)
        {
            if (HitObjectContainer == null)
                throw new InvalidOperationException($"{nameof(HitObjectContainer)} should be set before {nameof(HandleHit)} is called.");

            // Hitobjects which themselves don't block future hitobjects don't cause misses (e.g. slider ticks, spinners).
            if (!hitObjectCanBlockFutureHits(hitObject))
                return;

            if (CheckHittable(hitObject, hitObject.HitObject.StartTime + hitObject.Result.TimeOffset, hitObject.Result.Type) != ClickAction.Hit)
                throw new InvalidOperationException($"A {hitObject} was hit before it became hittable!");

            // Miss all hitobjects prior to the hit one.
            foreach (var obj in enumerateHitObjectsUpTo(hitObject.HitObject.StartTime))
            {
                if (obj.Judged)
                    continue;

                if (hitObjectCanBlockFutureHits(obj))
                    ((DrawableOsuHitObject)obj).MissForcefully();
            }
        }

        /// <summary>
        /// Whether a <see cref="HitObject"/> blocks hits on future <see cref="HitObject"/>s until its start time is reached.
        /// </summary>
        /// <param name="hitObject">The <see cref="HitObject"/> to test.</param>
        private static bool hitObjectCanBlockFutureHits(DrawableHitObject hitObject)
            => hitObject is DrawableHitCircle;

        private IEnumerable<DrawableHitObject> enumerateHitObjectsUpTo(double targetTime)
        {
            foreach (var obj in HitObjectContainer!.AliveObjects)
            {
                if (obj.HitObject.StartTime >= targetTime)
                    yield break;

                yield return obj;

                foreach (var nestedObj in obj.NestedHitObjects)
                {
                    if (nestedObj.HitObject.StartTime >= targetTime)
                        break;

                    yield return nestedObj;
                }
            }
        }
    }
}
