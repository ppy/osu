// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Graphics;
using osuTK.Graphics;

namespace osu.Game.Online.Multiplayer.RoomStatuses
{
    public class RoomStatusEnded : RoomStatus
    {
        public override string Message => @"Ended";
        public override Color4 GetAppropriateColour(OsuColour colours) => colours.YellowDarker;
    }
}
