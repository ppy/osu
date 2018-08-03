// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Online.Multiplayer;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Screens.Multi.Components
{
    public class DrawableGameType : CircularContainer, IHasTooltip
    {
        private readonly GameType type;
        private readonly float size;

        public string TooltipText => type.GetDescription();

        public DrawableGameType(GameType type, float size = 16)
        {
            this.type = type;
            this.size = size;
            Masking = true;

            Children = new[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.FromHex(@"545454"),
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Drawable icon;

            switch (type)
            {
                case GameType.Versus:
                    icon = new VersusRow(colours.Blue, colours.Blue, size * 0.6f)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    };

                    break;

                case GameType.Tag:
                    icon = new SpriteIcon
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Icon = FontAwesome.fa_refresh,
                        Size = new Vector2(size),
                        Colour = colours.Blue,
                        Shadow = false,
                    };

                    break;

                case GameType.TagTeam:
                    icon = new FillFlowContainer
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

                    break;

                case GameType.TeamVersus:
                    icon = new FillFlowContainer
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

                    break;

                default:
                    icon = null;
                    break;
            }

            Add(icon);
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
}
