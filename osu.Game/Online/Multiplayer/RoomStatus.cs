// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
