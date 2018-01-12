// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;

namespace osu.Game.Graphics.Cursor
{
    public interface IProvideLocalCursor : IDrawable
    {
        /// <summary>
        /// The cursor provided by this <see cref="Drawable"/>.
        /// </summary>
        CursorContainer LocalCursor { get; }

        /// <summary>
        /// Whether the cursor provided by this <see cref="Drawable"/> should be displayed.
        /// If this is false, a cursor occurring earlier in the draw hierarchy will be displayed instead.
        /// </summary>
        bool ProvidesUserCursor { get; }
    }
}
