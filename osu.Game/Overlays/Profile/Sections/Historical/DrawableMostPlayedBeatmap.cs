// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Profile.Sections.Historical
{
    public class DrawableMostPlayedBeatmap : CompositeDrawable
    {
        private const int cover_width = 100;
        private const int corner_radius = 6;

        private readonly APIUserMostPlayedBeatmap mostPlayed;

        public DrawableMostPlayedBeatmap(APIUserMostPlayedBeatmap mostPlayed)
        {
            this.mostPlayed = mostPlayed;

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
                new UpdateableOnlineBeatmapSetCover(BeatmapSetCoverType.List)
                {
                    RelativeSizeAxes = Axes.Y,
                    Width = cover_width,
                    OnlineInfo = mostPlayed.BeatmapSet,
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
                                                    new MostPlayedBeatmapMetadataContainer(mostPlayed.BeatmapInfo),
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
                                                        d.AddUserLink(mostPlayed.BeatmapSet.Author);
                                                    }),
                                                }
                                            },
                                            new PlayCountText(mostPlayed.PlayCount)
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
            public MostPlayedBeatmapMetadataContainer(IBeatmapInfo beatmapInfo)
                : base(beatmapInfo)
            {
            }

            protected override Drawable[] CreateText(IBeatmapInfo beatmapInfo)
            {
                var metadata = beatmapInfo.Metadata;

                Debug.Assert(metadata != null);

                return new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Text = new RomanisableString(metadata.TitleUnicode, metadata.Title),
                        Font = OsuFont.GetFont(weight: FontWeight.Bold)
                    },
                    new OsuSpriteText
                    {
                        Text = $" [{beatmapInfo.DifficultyName}]",
                        Font = OsuFont.GetFont(weight: FontWeight.Bold)
                    },
                    new OsuSpriteText
                    {
                        Text = " by ",
                        Font = OsuFont.GetFont(weight: FontWeight.Regular)
                    },
                    new OsuSpriteText
                    {
                        Text = new RomanisableString(metadata.ArtistUnicode, metadata.Artist),
                        Font = OsuFont.GetFont(weight: FontWeight.Regular)
                    },
                };
            }
        }

        private class PlayCountText : CompositeDrawable, IHasTooltip
        {
            public LocalisableString TooltipText => UsersStrings.ShowExtraHistoricalMostPlayedCount;

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
