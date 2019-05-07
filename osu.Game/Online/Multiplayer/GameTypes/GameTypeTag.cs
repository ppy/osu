// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osuTK;

namespace osu.Game.Online.Multiplayer.GameTypes
{
    public class GameTypeTag : GameType
    {
        public override string Name => "Tag";

        public override Drawable GetIcon(OsuColour colours, float size)
        {
            return new SpriteIcon
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Icon = FontAwesome.Solid.Sync,
                Size = new Vector2(size),
                Colour = colours.Blue,
                Shadow = false,
            };
        }
    }
}
