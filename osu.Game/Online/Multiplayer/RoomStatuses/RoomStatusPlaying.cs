// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Graphics;
using osuTK.Graphics;

namespace osu.Game.Online.Multiplayer.RoomStatuses
{
    public class RoomStatusPlaying : RoomStatus
    {
        public override string Message => @"Now Playing";
        public override Color4 GetAppropriateColour(OsuColour colours) => colours.Purple;
    }
}
