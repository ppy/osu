// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Objects
{
    /// <summary>
    /// Compares two hit objects by their start time, falling back to creation order if their start time is equal.
    /// </summary>
    public class HitObjectStartTimeComparer : Drawable.CreationOrderDepthComparer
    {
        public override int Compare(Drawable x, Drawable y)
        {
            var hitObjectX = x as DrawableHitObject;
            var hitObjectY = y as DrawableHitObject;

            // If either of the two drawables are not hit objects, fall back to the base comparer
            if (hitObjectX?.HitObject == null || hitObjectY?.HitObject == null)
                return base.Compare(x, y);

            // Compare by start time
            int i = hitObjectX.HitObject.StartTime.CompareTo(hitObjectY.HitObject.StartTime);
            if (i != 0)
                return i;

            return base.Compare(x, y);
        }
    }

    /// <summary>
    /// Compares two hit objects by their start time, falling back to creation order if their start time is equal.
    /// This will compare the two hit objects in reverse order.
    /// </summary>
    public class HitObjectReverseStartTimeComparer : Drawable.ReverseCreationOrderDepthComparer
    {
        public override int Compare(Drawable x, Drawable y)
        {
            var hitObjectX = x as DrawableHitObject;
            var hitObjectY = y as DrawableHitObject;

            // If either of the two drawables are not hit objects, fall back to the base comparer
            if (hitObjectX?.HitObject == null || hitObjectY?.HitObject == null)
                return base.Compare(x, y);

            // Compare by start time
            int i = hitObjectY.HitObject.StartTime.CompareTo(hitObjectX.HitObject.StartTime);
            if (i != 0)
                return i;

            return base.Compare(x, y);
        }
    }
}