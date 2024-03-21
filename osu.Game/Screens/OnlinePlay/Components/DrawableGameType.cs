// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Online.Rooms;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Components
{
    public partial class DrawableGameType : CircularContainer, IHasTooltip
    {
        private readonly MatchType type;

        public LocalisableString TooltipText => type.GetLocalisableDescription();

        public DrawableGameType(MatchType type)
        {
            this.type = type;
            Masking = true;

            Children = new[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4Extensions.FromHex(@"545454"),
                },
            };
        }

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(getIconFor(type));
        }

        private Drawable getIconFor(MatchType matchType)
        {
            float size = Height / 2;

            switch (matchType)
            {
                default:
                case MatchType.Playlists:
                    return new SpriteIcon
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(size),
                        Icon = FontAwesome.Regular.Clock,
                        Colour = colours.Blue,
                        Shadow = false
                    };

                case MatchType.HeadToHead:
                    return new VersusRow(colours.Blue, colours.Blue, size * 0.6f)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    };

                case MatchType.TeamVersus:
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

#pragma warning disable IDE0055 // Indentation of commented code
                    // case MatchType.TagCoop:
                    //     return new SpriteIcon
                    //     {
                    //         Anchor = Anchor.Centre,
                    //         Origin = Anchor.Centre,
                    //         Size = new Vector2(size),
                    //         Icon = FontAwesome.Solid.Sync,
                    //         Colour = colours.Blue,
                    //
                    //         Shadow = false
                    //     };

                    // case MatchType.TagTeamCoop:
                    //     return new FillFlowContainer
                    //     {
                    //         Anchor = Anchor.Centre,
                    //         Origin = Anchor.Centre,
                    //         AutoSizeAxes = Axes.Both,
                    //         Direction = FillDirection.Horizontal,
                    //         Spacing = new Vector2(2f),
                    //         Children = new[]
                    //         {
                    //             new SpriteIcon
                    //             {
                    //                 Icon = FontAwesome.Solid.Sync,
                    //                 Size = new Vector2(size * 0.75f),
                    //                 Colour = colours.Blue,
                    //                 Shadow = false,
                    //             },
                    //             new SpriteIcon
                    //             {
                    //                 Icon = FontAwesome.Solid.Sync,
                    //                 Size = new Vector2(size * 0.75f),
                    //                 Colour = colours.Pink,
                    //                 Shadow = false,
                    //             },
                    //         },
                    //     };
#pragma warning restore IDE0055
            }
        }

        private partial class VersusRow : FillFlowContainer
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
