// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Graphics;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Spectate
{
    public partial class PlayerGrid
    {
        /// <summary>
        /// A facade of the grid which is used as a dummy object to store the required position/size of cells.
        /// </summary>
        private class Facade : Drawable
        {
            public Facade()
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
            }
        }
    }
}
