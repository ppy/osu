// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Osu.UI
{
    /// <summary>
    /// Ensures that <see cref="HitObject"/>s are hit in-order.
    /// If a <see cref="HitObject"/> is hit out of order:
    /// <list type="number">
    /// <item><description>The hit is blocked if it occurred earlier than the previous <see cref="HitObject"/>'s start time.</description></item>
    /// <item><description>The hit causes all previous <see cref="HitObject"/>s to missed otherwise.</description></item>
    /// </list>
    /// </summary>
    public class OrderedHitPolicy
    {
        private readonly HitObjectContainer hitObjectContainer;

        public OrderedHitPolicy(HitObjectContainer hitObjectContainer)
        {
            this.hitObjectContainer = hitObjectContainer;
        }

        /// <summary>
        /// Determines whether a <see cref="DrawableHitObject"/> can be hit at a point in time.
        /// </summary>
        /// <param name="hitObject">The <see cref="DrawableHitObject"/> to check.</param>
        /// <param name="time">The time to check.</param>
        /// <returns>Whether <paramref name="hitObject"/> can be hit at the given <paramref name="time"/>.</returns>
        public bool IsHittable(DrawableHitObject hitObject, double time)
        {
            DrawableHitObject blockingObject = null;

            var enumerator = new HitObjectEnumerator(hitObjectContainer, hitObject.HitObject.StartTime);

            while (enumerator.MoveNext())
            {
                Debug.Assert(enumerator.Current != null);

                if (hitObjectCanBlockFutureHits(enumerator.Current))
                    blockingObject = enumerator.Current;
            }

            // If there is no previous hitobject, allow the hit.
            if (blockingObject == null)
                return true;

            // A hit is allowed if:
            // 1. The last blocking hitobject has been judged.
            // 2. The current time is after the last hitobject's start time.
            // Hits at exactly the same time as the blocking hitobject are allowed for maps that contain simultaneous hitobjects (e.g. /b/372245).
            if (blockingObject.Judged || time >= blockingObject.HitObject.StartTime)
                return true;

            return false;
        }

        /// <summary>
        /// Handles a <see cref="HitObject"/> being hit to potentially miss all earlier <see cref="HitObject"/>s.
        /// </summary>
        /// <param name="hitObject">The <see cref="HitObject"/> that was hit.</param>
        public void HandleHit(DrawableHitObject hitObject)
        {
            // Hitobjects which themselves don't block future hitobjects don't cause misses (e.g. slider ticks, spinners).
            if (!hitObjectCanBlockFutureHits(hitObject))
                return;

            var enumerator = new HitObjectEnumerator(hitObjectContainer, hitObject.HitObject.StartTime);

            while (enumerator.MoveNext())
            {
                Debug.Assert(enumerator.Current != null);

                if (enumerator.Current.Judged)
                    continue;

                if (hitObjectCanBlockFutureHits(enumerator.Current))
                    ((DrawableOsuHitObject)enumerator.Current).MissForcefully();
            }
        }

        /// <summary>
        /// Whether a <see cref="HitObject"/> blocks hits on future <see cref="HitObject"/>s until its start time is reached.
        /// </summary>
        /// <param name="hitObject">The <see cref="HitObject"/> to test.</param>
        private static bool hitObjectCanBlockFutureHits(DrawableHitObject hitObject)
            => hitObject is DrawableHitCircle;

        private struct HitObjectEnumerator : IEnumerator<DrawableHitObject>
        {
            private readonly IEnumerator<DrawableHitObject> hitObjectEnumerator;
            private readonly double targetTime;

            private DrawableHitObject currentTopLevel;
            private int currentNestedIndex;

            public HitObjectEnumerator(HitObjectContainer hitObjectContainer, double targetTime)
            {
                hitObjectEnumerator = hitObjectContainer.AliveObjects.GetEnumerator();
                this.targetTime = targetTime;

                currentTopLevel = null;
                currentNestedIndex = -1;
                Current = null;
            }

            /// <summary>
            /// Attempts to move to the next top-level or nested hitobject.
            /// Stops when no such hitobject is found or until the hitobject start time reaches <see cref="targetTime"/>.
            /// </summary>
            /// <returns>Whether a new hitobject was moved to.</returns>
            public bool MoveNext()
            {
                // If we don't already have a top-level hitobject, try to get one.
                if (currentTopLevel == null)
                    return moveNextTopLevel();

                // If we have a top-level hitobject, try to move to the next nested hitobject or otherwise move to the next top-level hitobject.
                if (!moveNextNested())
                    return moveNextTopLevel();

                // Guaranteed by moveNextNested() to have a hitobject.
                return true;
            }

            /// <summary>
            /// Attempts to move to the next top-level hitobject.
            /// </summary>
            /// <returns>Whether a new top-level hitobject was found.</returns>
            private bool moveNextTopLevel()
            {
                currentNestedIndex = -1;

                hitObjectEnumerator.MoveNext();
                currentTopLevel = hitObjectEnumerator.Current;

                Current = currentTopLevel;

                return Current?.HitObject.StartTime < targetTime;
            }

            /// <summary>
            /// Attempts to move to the next nested hitobject in the current top-level hitobject.
            /// </summary>
            /// <returns>Whether a new nested hitobject was moved to.</returns>
            private bool moveNextNested()
            {
                currentNestedIndex++;
                if (currentNestedIndex >= currentTopLevel.NestedHitObjects.Count)
                    return false;

                Current = currentTopLevel.NestedHitObjects[currentNestedIndex];
                Debug.Assert(Current != null);

                return Current?.HitObject.StartTime < targetTime;
            }

            public void Reset()
            {
                hitObjectEnumerator.Reset();
                currentTopLevel = null;
                currentNestedIndex = -1;
                Current = null;
            }

            public DrawableHitObject Current { get; set; }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }
        }
    }
}
