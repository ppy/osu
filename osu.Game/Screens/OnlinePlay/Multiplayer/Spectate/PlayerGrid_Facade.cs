// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Spectate
{
    public partial class PlayerGrid
    {
        private class PlayerGridFacade : Drawable
        {
            public PlayerGridFacade()
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
            }
        }
    }
}
