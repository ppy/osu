// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Spectate
{
    public enum MasterClockState
    {
        /// <summary>
        /// The master clock is synchronised with at least one player clock.
        /// </summary>
        Synchronised,

        /// <summary>
        /// The master clock is too far ahead of any player clock and needs to slow down.
        /// </summary>
        TooFarAhead
    }
}
