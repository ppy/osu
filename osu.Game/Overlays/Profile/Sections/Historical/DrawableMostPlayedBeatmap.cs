// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Users;
using osuTK;
using osuTK.Graphics;
using System;

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
                                        new BeatmapName(beatmap)
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
                                                    Font = OsuFont.GetFont(size: 30, weight: FontWeight.Regular, italics: false, fixedWidth: true),
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

        private class ClickableLink : Container
        {
            protected readonly BeatmapInfo Beatmap;
            protected Action ClickAction;
            private Container underscore;
            protected FillFlowContainer TextContent;
            protected Box UnderscoreBackground;

            public ClickableLink(BeatmapInfo beatmap)
            {
                Beatmap = beatmap;
                AutoSizeAxes = Axes.Both;
                Child = new Container
                {
                    AutoSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        underscore = new Container
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            RelativeSizeAxes = Axes.X,
                            Height = 1,
                            Alpha = 0,
                            AlwaysPresent = true,
                            Child = UnderscoreBackground = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                            }
                        },
                        TextContent = new FillFlowContainer
                        {
                            Direction = FillDirection.Horizontal,
                            AutoSizeAxes = Axes.Both,
                        },
                    },
                };
            }

            protected override bool OnHover(HoverEvent e)
            {
                underscore.FadeIn(duration, Easing.OutQuint);
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                underscore.FadeOut(duration, Easing.OutQuint);
                base.OnHoverLost(e);
            }

            protected override bool OnClick(ClickEvent e)
            {
                ClickAction?.Invoke();
                return base.OnClick(e);
            }
        }

        private class BeatmapName : ClickableLink
        {
            public BeatmapName(BeatmapInfo beatmap)
                : base(beatmap)
            {
                TextContent.AddRange(new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Text = new LocalisedString((
                            $"{Beatmap.Metadata.TitleUnicode ?? Beatmap.Metadata.Title} [{Beatmap.Version}] ",
                            $"{Beatmap.Metadata.Title ?? Beatmap.Metadata.TitleUnicode} [{Beatmap.Version}] ")),
                        Font = OsuFont.GetFont(size: 20, weight: FontWeight.Bold)
                    },
                    new OsuSpriteText
                    {
                        Text = "by " + new LocalisedString((Beatmap.Metadata.ArtistUnicode, Beatmap.Metadata.Artist)),
                        Font = OsuFont.GetFont(size: 20, weight: FontWeight.Regular)
                    },
                });
            }

            [BackgroundDependencyLoader(true)]
            private void load(BeatmapSetOverlay beatmapSetOverlay)
            {
                ClickAction = () =>
                {
                    if (Beatmap.OnlineBeatmapID != null)
                        beatmapSetOverlay?.FetchAndShowBeatmap(Beatmap.OnlineBeatmapID.Value);
                    else if (Beatmap.BeatmapSet?.OnlineBeatmapSetID != null)
                        beatmapSetOverlay?.FetchAndShowBeatmapSet(Beatmap.BeatmapSet.OnlineBeatmapSetID.Value);
                };
            }
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
