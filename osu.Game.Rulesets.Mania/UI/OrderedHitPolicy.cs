// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Mania.UI
{
    public class OrderedHitPolicy
    {
        private readonly HitObjectContainer hitObjectContainer;

        public OrderedHitPolicy(HitObjectContainer hitObjectContainer)
        {
            this.hitObjectContainer = hitObjectContainer;
        }

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
            if (!IsHittable(hitObject, hitObject.HitObject.StartTime + hitObject.Result.TimeOffset))
                throw new InvalidOperationException($"A {hitObject} was hit before it became hittable!");

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
