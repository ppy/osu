// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK;
using osu.Framework.Graphics.Cursor;

namespace osu.Game.Overlays.Profile.Sections.Historical
{
    public class DrawableMostPlayedBeatmap : CompositeDrawable
    {
        private const int cover_width = 100;
        private const int corner_radius = 6;

        private readonly BeatmapInfo beatmap;
        private readonly int playCount;

        public DrawableMostPlayedBeatmap(BeatmapInfo beatmap, int playCount)
        {
            this.beatmap = beatmap;
            this.playCount = playCount;

            RelativeSizeAxes = Axes.X;
            Height = 50;

            Masking = true;
            CornerRadius = corner_radius;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            AddRangeInternal(new Drawable[]
            {
                new UpdateableBeatmapSetCover
                {
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
                                new MostPlayedBeatmapContainer
                                {
                                    Child = new Container
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Padding = new MarginPadding(10),
                                        Children = new Drawable[]
                                        {
                                            new FillFlowContainer
                                            {
                                                Anchor = Anchor.CentreLeft,
                                                Origin = Anchor.CentreLeft,
                                                AutoSizeAxes = Axes.Both,
                                                Direction = FillDirection.Vertical,
                                                Children = new Drawable[]
                                                {
                                                    new MostPlayedBeatmapMetadataContainer(beatmap),
                                                    new LinkFlowContainer(t =>
                                                    {
                                                        t.Font = OsuFont.GetFont(size: 12, weight: FontWeight.Regular);
                                                        t.Colour = colourProvider.Foreground1;
                                                    })
                                                    {
                                                        AutoSizeAxes = Axes.Both,
                                                        Direction = FillDirection.Horizontal,
                                                    }.With(d =>
                                                    {
                                                        d.AddText("mapped by ");
                                                        d.AddUserLink(beatmap.Metadata.Author);
                                                    }),
                                                }
                                            },
                                            new PlayCountText(playCount)
                                            {
                                                Anchor = Anchor.CentreRight,
                                                Origin = Anchor.CentreRight
                                            },
                                        }
                                    },
                                }
                            }
                        }
                    }
                }
            });
        }

        private class MostPlayedBeatmapContainer : ProfileItemContainer
        {
            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                IdleColour = colourProvider.Background4;
                HoverColour = colourProvider.Background3;
            }
        }

        private class MostPlayedBeatmapMetadataContainer : BeatmapMetadataContainer
        {
            public MostPlayedBeatmapMetadataContainer(BeatmapInfo beatmap)
                : base(beatmap)
            {
            }

            protected override Drawable[] CreateText(BeatmapInfo beatmap) => new Drawable[]
            {
                new OsuSpriteText
                {
                    Text = new LocalisedString((
                        $"{beatmap.Metadata.TitleUnicode ?? beatmap.Metadata.Title} [{beatmap.Version}] ",
                        $"{beatmap.Metadata.Title ?? beatmap.Metadata.TitleUnicode} [{beatmap.Version}] ")),
                    Font = OsuFont.GetFont(weight: FontWeight.Bold)
                },
                new OsuSpriteText
                {
                    Text = "by " + new LocalisedString((beatmap.Metadata.ArtistUnicode, beatmap.Metadata.Artist)),
                    Font = OsuFont.GetFont(weight: FontWeight.Regular)
                },
            };
        }

        private class PlayCountText : CompositeDrawable, IHasTooltip
        {
            public string TooltipText => "times played";

            public PlayCountText(int playCount)
            {
                AutoSizeAxes = Axes.Both;

                InternalChild = new FillFlowContainer
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    AutoSizeAxes = Axes.Both,
                    Spacing = new Vector2(5, 0),
                    Children = new Drawable[]
                    {
                        new SpriteIcon
                        {
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            Size = new Vector2(12),
                            Icon = FontAwesome.Solid.Play,
                        },
                        new OsuSpriteText
                        {
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            Text = playCount.ToString(),
                            Font = OsuFont.GetFont(size: 20, weight: FontWeight.Regular),
                        },
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                Colour = colours.Yellow;
            }
        }
    }
}
