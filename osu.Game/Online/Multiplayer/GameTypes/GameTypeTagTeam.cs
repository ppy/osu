// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osuTK;

namespace osu.Game.Online.Multiplayer.GameTypes
{
    public class GameTypeTagTeam : GameType
    {
        public override string Name => "Tag Team";

        public override Drawable GetIcon(OsuColour colours, float size)
        {
            return new FillFlowContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(2f),
                Children = new[]
                {
                    new SpriteIcon
                    {
                        Icon = FontAwesome.Solid.Sync,
                        Size = new Vector2(size * 0.75f),
                        Colour = colours.Blue,
                        Shadow = false,
                    },
                    new SpriteIcon
                    {
                        Icon = FontAwesome.Solid.Sync,
                        Size = new Vector2(size * 0.75f),
                        Colour = colours.Pink,
                        Shadow = false,
                    },
                },
            };
        }
    }
}
