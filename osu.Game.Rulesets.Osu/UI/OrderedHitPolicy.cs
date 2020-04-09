// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
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
            DrawableHitObject lastObject = hitObject;

            // Get the last hitobject that can block future hits
            while ((lastObject = hitObjectContainer.AliveObjects.GetPrevious(lastObject)) != null)
            {
                if (canBlockFutureHits(lastObject.HitObject))
                    break;
            }

            // If there is no previous object alive, allow the hit.
            if (lastObject == null)
                return true;

            // Ensure that either the last object has received a judgement or the hit time occurs at or after the last object's start time.
            // Simultaneous hitobjects are allowed to be hit at the same time value to account for edge-cases such as Centipede.
            if (lastObject.Judged || time >= lastObject.HitObject.StartTime)
                return true;

            return false;
        }

        /// <summary>
        /// Handles a <see cref="HitObject"/> being hit to potentially miss all earlier <see cref="HitObject"/>s.
        /// </summary>
        /// <param name="hitObject">The <see cref="HitObject"/> that was hit.</param>
        public void HandleHit(HitObject hitObject)
        {
            if (!canBlockFutureHits(hitObject))
                return;

            double minimumTime = hitObject.StartTime;

            foreach (var obj in hitObjectContainer.AliveObjects)
            {
                if (obj.HitObject.StartTime >= minimumTime)
                    break;

                switch (obj)
                {
                    case DrawableHitCircle circle:
                        miss(circle);
                        break;

                    case DrawableSlider slider:
                        miss(slider.HeadCircle);
                        break;
                }
            }

            static void miss(DrawableOsuHitObject obj)
            {
                // Hitobjects that have already been judged cannot be missed.
                if (obj.Judged)
                    return;

                obj.MissForcefully();
            }
        }

        /// <summary>
        /// Whether a <see cref="HitObject"/> blocks hits on future <see cref="HitObject"/>s until its start time is reached.
        /// </summary>
        /// <param name="hitObject">The <see cref="HitObject"/> to test.</param>
        private bool canBlockFutureHits(HitObject hitObject)
            => hitObject is HitCircle || hitObject is Slider;
    }
}
