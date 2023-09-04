// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Mania.UI
{
    /// <summary>
    /// Ensures that only the most recent <see cref="HitObject"/> is hittable, affectionately known as "note lock".
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
        /// <remarks>
        /// Only the most recent <see cref="DrawableHitObject"/> can be hit, a previous hitobject's window cannot extend past the next one.
        /// </remarks>
        /// <param name="hitObject">The <see cref="DrawableHitObject"/> to check.</param>
        /// <param name="time">The time to check.</param>
        /// <returns>Whether <paramref name="hitObject"/> can be hit at the given <paramref name="time"/>.</returns>
        public bool IsHittable(DrawableHitObject hitObject, double time)
        {
            var nextObject = hitObjectContainer.AliveObjects.GetNext(hitObject);
            return nextObject == null || time < nextObject.HitObject.StartTime;
        }

        /// <summary>
        /// Handles a <see cref="HitObject"/> being hit to potentially miss all earlier <see cref="HitObject"/>s.
        /// </summary>
        /// <param name="hitObject">The <see cref="HitObject"/> that was hit.</param>
        public void HandleHit(DrawableHitObject hitObject)
        {
            foreach (var obj in enumerateHitObjectsUpTo(hitObject.HitObject.StartTime))
            {
                if (obj.Judged)
                    continue;

                ((DrawableManiaHitObject)obj).MissForcefully();
            }
        }

        private IEnumerable<DrawableHitObject> enumerateHitObjectsUpTo(double targetTime)
        {
            foreach (var obj in hitObjectContainer.AliveObjects)
            {
                if (obj.HitObject.GetEndTime() >= targetTime)
                    yield break;

                yield return obj;

                foreach (var nestedObj in obj.NestedHitObjects)
                {
                    if (nestedObj.HitObject.GetEndTime() >= targetTime)
                        break;

                    yield return nestedObj;
                }
            }
        }
    }
}
