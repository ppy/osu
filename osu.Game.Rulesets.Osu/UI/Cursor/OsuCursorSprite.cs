// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Rulesets.Osu.UI.Cursor
{
    public abstract class OsuCursorSprite : CompositeDrawable
    {
        /// <summary>
        /// The an optional piece of the cursor to expand when in a clicked state.
        /// If null, the whole cursor will be affected by expansion.
        /// </summary>
        public Drawable ExpandTarget { get; protected set; }
    }
}
