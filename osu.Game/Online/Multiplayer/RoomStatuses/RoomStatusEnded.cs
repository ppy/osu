// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Graphics;

namespace osu.Game.Online.Multiplayer.RoomStatuses
{
    public class RoomStatusEnded : RoomStatus
    {
        public override string Message => @"Ended";
        public override Colour4 GetAppropriateColour(OsuColour colours) => colours.YellowDarker;
    }
}
