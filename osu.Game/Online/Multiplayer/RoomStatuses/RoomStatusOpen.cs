// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Graphics;
using osuTK.Graphics;

namespace osu.Game.Online.Multiplayer.RoomStatuses
{
    public class RoomStatusOpen : RoomStatus
    {
        public override string Message => @"Welcoming Players";
        public override Color4 GetAppropriateColour(OsuColour colours) => colours.GreenLight;
    }
}
