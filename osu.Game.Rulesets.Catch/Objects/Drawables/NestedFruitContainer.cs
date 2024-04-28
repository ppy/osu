// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.Objects.Drawables
{
    public partial class NestedFruitContainer : Container
    {
        /// <remarks>
        /// This comparison logic is a copy of <see cref="HitObjectContainer"/> comparison logic,
        /// which can't be easily extracted to a more common place.
        /// </remarks>
        /// <seealso cref="HitObjectContainer.Compare"/>
        protected override int Compare(Drawable x, Drawable y)
        {
            if (x is not DrawableCatchHitObject xObj || y is not DrawableCatchHitObject yObj)
                return base.Compare(x, y);

            int result = yObj.HitObject.StartTime.CompareTo(xObj.HitObject.StartTime);
            return result == 0 ? CompareReverseChildID(x, y) : result;
        }
    }
}
