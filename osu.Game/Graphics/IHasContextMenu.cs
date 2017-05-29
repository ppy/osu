// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Graphics
{
    public interface IHasContextMenu : IDrawable
    {
        /// <summary>
        /// Menu that opens when clicked on the drawable
        /// </summary>
        ContextMenuItem[] Items { get; }
    }
}
