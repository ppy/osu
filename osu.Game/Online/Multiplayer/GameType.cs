// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;

namespace osu.Game.Online.Multiplayer
{
    public abstract class GameType
    {
        public abstract string Name { get; }
        public abstract Drawable GetIcon(OsuColour colours, float size);
    }

    public class GameTypeTag : GameType
    {
        public override string Name => "Tag";
        public override Drawable GetIcon(OsuColour colours, float size)
        {
            return new SpriteIcon
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Icon = FontAwesome.fa_refresh,
                Size = new Vector2(size),
                Colour = colours.Blue,
                Shadow = false,
            };
        }
    }

    public class GameTypeVersus : GameType
    {
        public override string Name => "Versus";
        public override Drawable GetIcon(OsuColour colours, float size)
        {
            return new VersusRow(colours.Blue, colours.Blue, size * 0.6f)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            };
        }
    }

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
                        Icon = FontAwesome.fa_refresh,
                        Size = new Vector2(size * 0.75f),
                        Colour = colours.Blue,
                        Shadow = false,
                    },
                    new SpriteIcon
                    {
                        Icon = FontAwesome.fa_refresh,
                        Size = new Vector2(size * 0.75f),
                        Colour = colours.Pink,
                        Shadow = false,
                    },
                },
            };
        }
    }

    public class GameTypeTeamVersus : GameType
    {
        public override string Name => "Team Versus";
        public override Drawable GetIcon(OsuColour colours, float size)
        {
            return new FillFlowContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(2f),
                Children = new[]
                {
                    new VersusRow(colours.Blue, colours.Pink, size * 0.5f),
                    new VersusRow(colours.Blue, colours.Pink, size * 0.5f),
                },
            };
        }
    }

    public class VersusRow : FillFlowContainer
    {
        public VersusRow(Color4 first, Color4 second, float size)
        {
            var triangleSize = new Vector2(size);
            AutoSizeAxes = Axes.Both;
            Spacing = new Vector2(2f, 0f);

            Children = new[]
            {
                new Container
                {
                    Size = triangleSize,
                    Colour = first,
                    Children = new[]
                    {
                        new EquilateralTriangle
                        {
                            Origin = Anchor.BottomLeft,
                            RelativeSizeAxes = Axes.Both,
                            Rotation = 90,
                            EdgeSmoothness = new Vector2(1f),
                        },
                    },
                },
                new Container
                {
                    Size = triangleSize,
                    Colour = second,
                    Children = new[]
                    {
                        new EquilateralTriangle
                        {
                            Anchor = Anchor.BottomLeft,
                            RelativeSizeAxes = Axes.Both,
                            Rotation = -90,
                            EdgeSmoothness = new Vector2(1f),
                        },
                    },
                },
            };
        }
    }
}
