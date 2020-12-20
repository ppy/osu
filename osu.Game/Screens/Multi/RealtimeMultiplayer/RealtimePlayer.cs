// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.Multiplayer;
using osu.Game.Screens.Multi.Play;

namespace osu.Game.Screens.Multi.RealtimeMultiplayer
{
    public class RealtimePlayer : TimeshiftPlayer
    {
        public RealtimePlayer(PlaylistItem playlistItem)
            : base(playlistItem)
        {
        }
    }
}
