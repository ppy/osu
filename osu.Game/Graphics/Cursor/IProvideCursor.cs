// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
        /// The cursor provided by this <see cref="IDrawable"/>.
        /// May be null if no cursor should be visible.
        /// </summary>
        CursorContainer Cursor { get; }

        /// <summary>
        /// Whether <see cref="Cursor"/> should be displayed as the singular user cursor. This will temporarily hide any other user cursor.
        /// This value is checked every frame and may be used to control whether multiple cursors are displayed (e.g. watching replays).
        /// </summary>
        bool ProvidingUserCursor { get; }
    }
}
