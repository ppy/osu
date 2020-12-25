// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osuTK;

namespace osu.Game.Online.Rooms.GameTypes
{
    public class GameTypePlaylists : GameType
    {
        public override string Name => "Playlists";

        public override Drawable GetIcon(OsuColour colours, float size) => new SpriteIcon
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Icon = FontAwesome.Regular.Clock,
            Size = new Vector2(size),
            Colour = colours.Blue,
            Shadow = false
        };
    }
}
