// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI.Scrolling;
using osuTK;

namespace osu.Game.Rulesets.Catch.Edit
{
    /// <summary>
    /// Utility functions used by the editor.
    /// </summary>
    public static class CatchHitObjectUtils
    {
        /// <summary>
        /// Get the position of the hit object in the playfield based on <see cref="CatchHitObject.OriginalX"/> and <see cref="HitObject.StartTime"/>.
        /// </summary>
        public static Vector2 GetStartPosition(ScrollingHitObjectContainer hitObjectContainer, CatchHitObject hitObject)
        {
            return new Vector2(hitObject.OriginalX, hitObjectContainer.PositionAtTime(hitObject.StartTime));
        }

        /// <summary>
        /// Get the range of horizontal position occupied by the hit object.
        /// </summary>
        /// <remarks>
        /// <see cref="TinyDroplet"/>s are excluded and returns <see cref="PositionRange.EMPTY"/>.
        /// </remarks>
        public static PositionRange GetPositionRange(HitObject hitObject)
        {
            switch (hitObject)
            {
                case Fruit fruit:
                    return new PositionRange(fruit.OriginalX);

                case Droplet droplet:
                    return droplet is TinyDroplet ? PositionRange.EMPTY : new PositionRange(droplet.OriginalX);

                case JuiceStream:
                    return GetPositionRange(hitObject.NestedHitObjects);

                case BananaShower:
                    // A banana shower occupies the whole screen width.
                    return new PositionRange(0, CatchPlayfield.WIDTH);

                default:
                    return PositionRange.EMPTY;
            }
        }

        /// <summary>
        /// Get the range of horizontal position occupied by the hit objects.
        /// </summary>
        /// <remarks>
        /// <see cref="TinyDroplet"/>s are excluded.
        /// </remarks>
        public static PositionRange GetPositionRange(IEnumerable<HitObject> hitObjects) => hitObjects.Select(GetPositionRange).Aggregate(PositionRange.EMPTY, PositionRange.Union);
    }
}
