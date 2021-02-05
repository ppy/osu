// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.UI
{
    /// <summary>
    /// Ensures that <see cref="HitObject"/>s are hit in order of appearance. The classic note lock.
    /// <remarks>
    /// Hits will be blocked until the previous <see cref="HitObject"/>s have been judged.
    /// </remarks>
    /// </summary>
    public class ObjectOrderedHitPolicy : IHitPolicy
    {
        private IEnumerable<DrawableHitObject> hitObjects;

        public void SetHitObjects(IEnumerable<DrawableHitObject> hitObjects) => this.hitObjects = hitObjects;

        public bool IsHittable(DrawableHitObject hitObject, double time) => enumerateHitObjectsUpTo(hitObject.HitObject.StartTime).All(obj => obj.AllJudged);

        public void HandleHit(DrawableHitObject hitObject)
        {
        }

        private IEnumerable<DrawableHitObject> enumerateHitObjectsUpTo(double targetTime)
        {
            foreach (var obj in hitObjects)
            {
                if (obj.HitObject.StartTime >= targetTime)
                    yield break;

                switch (obj)
                {
                    case DrawableSpinner _:
                        continue;

                    case DrawableSlider slider:
                        yield return slider.HeadCircle;

                        break;

                    default:
                        yield return obj;

                        break;
                }
            }
        }
    }
}
