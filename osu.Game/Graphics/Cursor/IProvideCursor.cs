// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;

namespace osu.Game.Graphics.Cursor
{
    /// <summary>
    /// Interface for <see cref="IDrawable"/>s that display cursors which can replace the user's cursor.
    /// </summary>
    public interface IProvideCursor : IDrawable
    {
        /// <summary>
        /// The cursor provided by this <see cref="Drawable"/>.
        /// May be null if no cursor should be visible.
        /// </summary>
        CursorContainer Cursor { get; }

        /// <summary>
        /// Whether the cursor provided by this <see cref="Drawable"/> should be displayed as the user's cursor.
        /// </summary>
        bool ProvidesUserCursor { get; }
    }
}
