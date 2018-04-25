// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Game.Graphics;

namespace osu.Game.Online.Multiplayer
{
    public abstract class RoomStatus
    {
        public abstract string Message { get; }
        public abstract Color4 GetAppropriateColour(OsuColour colours);
    }

    public class RoomStatusOpen : RoomStatus
    {
        public override string Message => @"Welcoming Players";
        public override Color4 GetAppropriateColour(OsuColour colours) => colours.GreenLight;
    }

    public class RoomStatusPlaying : RoomStatus
    {
        public override string Message => @"Now Playing";
        public override Color4 GetAppropriateColour(OsuColour colours) => colours.Purple;
    }
}
