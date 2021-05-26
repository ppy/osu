// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Graphics;

namespace osu.Game.Online.Rooms
{
    public abstract class GameType
    {
        public abstract string Name { get; }

        public abstract Drawable GetIcon(OsuColour colours, float size);

        public override int GetHashCode() => GetType().GetHashCode();
        public override bool Equals(object obj) => GetType() == obj?.GetType();
    }
}
