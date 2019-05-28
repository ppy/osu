// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Users;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Profile.Sections.Historical
{
    public class DrawableMostPlayedBeatmap : Container
    {
        private readonly BeatmapInfo beatmap;
        private readonly OsuSpriteText mapperText;
        private readonly int playCount;
        private readonly Box background;
        private Color4 idleBackgroundColour;
        private Color4 hoveredBackgroundColour;
        private const int duration = 300;
        private const int cover_width = 100;
        private const int corner_radius = 10;
        private readonly SpriteIcon icon;
        private readonly OsuSpriteText playCountText;

        public DrawableMostPlayedBeatmap(BeatmapInfo beatmap, int playCount)
        {
            this.beatmap = beatmap;
            this.playCount = playCount;

            RelativeSizeAxes = Axes.X;
            Height = 60;
            Masking = true;
            CornerRadius = corner_radius;
            Children = new Drawable[]
            {
                new UpdateableBeatmapSetCover
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    RelativeSizeAxes = Axes.Y,
                    Width = cover_width,
                    BeatmapSet = beatmap.BeatmapSet,
                    CoverType = BeatmapSetCoverType.List,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Left = cover_width - corner_radius },
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Masking = true,
                            CornerRadius = corner_radius,
                            Children = new Drawable[]
                            {
                                background = new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                },
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Padding = new MarginPadding { Left = 15, Right = 20 },
                                    Children = new Drawable[]
                                    {
                                        new BeatmapLink(beatmap, BeatmapLinkType.TitleVersionAuthor)
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.BottomLeft,
                                            Margin = new MarginPadding { Bottom = 2 },
                                        },
                                        new FillFlowContainer
                                        {
                                            AutoSizeAxes = Axes.Both,
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.TopLeft,
                                            Direction = FillDirection.Horizontal,
                                            Margin = new MarginPadding { Top = 2 },
                                            Children = new Drawable[]
                                            {
                                                mapperText = new OsuSpriteText
                                                {
                                                    Text = "mapped by ",
                                                    Font = OsuFont.GetFont(size: 15, weight: FontWeight.Regular),
                                                },
                                                new MapperName(beatmap),
                                            }
                                        },
                                        new FillFlowContainer
                                        {
                                            Anchor = Anchor.CentreRight,
                                            Origin = Anchor.CentreRight,
                                            AutoSizeAxes = Axes.Both,
                                            Direction = FillDirection.Horizontal,
                                            Children = new Drawable[]
                                            {
                                                icon = new SpriteIcon
                                                {
                                                    Icon = FontAwesome.Solid.CaretRight,
                                                    Origin = Anchor.Centre,
                                                    Anchor = Anchor.Centre,
                                                    Size = new Vector2(20),
                                                },
                                                playCountText = new OsuSpriteText
                                                {
                                                    Origin = Anchor.Centre,
                                                    Anchor = Anchor.Centre,
                                                    Text = playCount.ToString(),
                                                    Font = OsuFont.GetFont(size: 30, weight: FontWeight.Regular, fixedWidth: true),
                                                },
                                            }
                                        }
                                    }
                                },
                            }
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colors)
        {
            idleBackgroundColour = background.Colour = colors.GreySeafoam;
            hoveredBackgroundColour = colors.GreySeafoamLight;
            mapperText.Colour = colors.GreySeafoamLighter;
            icon.Colour = playCountText.Colour = colors.Yellow;
        }

        protected override bool OnHover(HoverEvent e)
        {
            background.FadeColour(hoveredBackgroundColour, duration, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            background.FadeColour(idleBackgroundColour, duration, Easing.OutQuint);
            base.OnHoverLost(e);
        }

        private class MapperName : ClickableLink
        {
            public MapperName(BeatmapInfo beatmap)
                : base(beatmap)
            {
            }

            [BackgroundDependencyLoader(true)]
            private void load(OsuColour colors, UserProfileOverlay userProfileOverlay)
            {
                User author = Beatmap.Metadata.Author;

                TextContent.AddRange(new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Text = author.Username,
                        Font = OsuFont.GetFont(size: 15, weight: FontWeight.Bold),
                        Colour = colors.GreySeafoamLighter,
                    }
                });

                UnderscoreBackground.Colour = colors.GreySeafoamLighter;
                ClickAction = () => userProfileOverlay.ShowUser(author.Id);
            }
        }
    }
}
