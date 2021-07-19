// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Catch.Objects;
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
    }
}
