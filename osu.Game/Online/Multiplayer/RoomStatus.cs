// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osuTK.Graphics;
using osu.Game.Graphics;

namespace osu.Game.Online.Multiplayer
{
    public abstract class RoomStatus
    {
        public abstract string Message { get; }
        public abstract Color4 GetAppropriateColour(OsuColour colours);

        public override int GetHashCode() => GetType().GetHashCode();
        public override bool Equals(object obj) => GetType() == obj?.GetType();
    }
}
