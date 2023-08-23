// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Osu.UI
{
    /// <summary>
    /// Ensures that <see cref="HitObject"/>s are hit in order of appearance. The classic note lock.
    /// <remarks>
    /// Hits will be blocked until the previous <see cref="HitObject"/>s have been judged.
    /// </remarks>
    /// </summary>
    public class LegacyHitPolicy : IHitPolicy
    {
        public IHitObjectContainer HitObjectContainer { get; set; }

        public void HandleHit(DrawableHitObject hitObject)
        {
        }

        public virtual ClickAction CheckHittable(DrawableHitObject hitObject, double time, HitResult result)
        {
            var aliveObjects = HitObjectContainer.AliveObjects.ToList();
            int index = aliveObjects.IndexOf(hitObject);

            if (index > 0)
            {
                var previousHitObject = (DrawableOsuHitObject)aliveObjects[index - 1];
                if (previousHitObject.HitObject.StackHeight > 0 && !previousHitObject.AllJudged)
                    return ClickAction.Ignore;
            }

            if (result == HitResult.None)
                return ClickAction.Shake;

            foreach (DrawableHitObject testObject in aliveObjects)
            {
                if (testObject.AllJudged)
                    continue;

                // if we found the object being checked, we can move on to the final timing test.
                if (testObject == hitObject)
                    break;

                // for all other objects, we check for validity and block the hit if any are still valid.
                // 3ms of extra leniency to account for slightly unsnapped objects.
                if (testObject.HitObject.GetEndTime() + 3 < hitObject.HitObject.StartTime)
                    return ClickAction.Shake;
            }

            // stable has `const HitObjectManager.HITTABLE_RANGE = 400;`, which is only used for notelock code.
            // probably not a coincidence that this is equivalent to lazer's OsuHitWindows.MISS_WINDOW.

            // TODO stable compares to 200 when autopilot is enabled, instead of 400.
            return Math.Abs(hitObject.HitObject.StartTime - time) < 400 ? ClickAction.Hit : ClickAction.Shake;
        }
    }
}
