// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Graphics;
using osuTK.Graphics;

namespace osu.Game.Online.Rooms.RoomStatuses
{
    public class RoomStatusOpen : RoomStatus
    {
        public override string Message => "Open";
        public override Color4 GetAppropriateColour(OsuColour colours) => colours.GreenLight;
    }
}
