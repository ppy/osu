// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Cards
{
    public partial class RankedPlayCardContent
    {
        private partial class CardMetadata(APIBeatmap beatmap) : CompositeDrawable
        {
            [BackgroundDependencyLoader]
            private void load(CardColours colours)
            {
                InternalChildren =
                [
                    new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Children =
                        [
                            new StarRatingBadge(beatmap)
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Margin = new MarginPadding { Top = 4 },
                            },
                        ]
                    },
                    new LinkFlowContainer(static s => s.ShadowOffset = new Vector2(0, 0.15f))
                    {
                        Name = "Beatmap Metadata",
                        RelativeSizeAxes = Axes.Both,
                        TextAnchor = Anchor.BottomLeft,
                        Padding = new MarginPadding(5) { Bottom = 10 },
                        ParagraphSpacing = 0.2f,
                    }.With(d =>
                    {
                        d.AddText(new RomanisableString(beatmap.Metadata.TitleUnicode, beatmap.Metadata.Title), static s => s.Font = OsuFont.GetFont(size: 12, weight: FontWeight.SemiBold));

                        d.NewLine();
                        d.AddText(new RomanisableString(beatmap.Metadata.ArtistUnicode, beatmap.Metadata.Artist), static s => s.Font = OsuFont.GetFont(size: 9, weight: FontWeight.SemiBold));

                        d.NewParagraph();
                        d.AddText("mapped by ", static s => s.Font = OsuFont.GetFont(size: 9, weight: FontWeight.SemiBold));
                        d.AddText(beatmap.Metadata.Author.Username, s =>
                        {
                            s.Font = OsuFont.GetFont(size: 9, weight: FontWeight.SemiBold);
                            s.Colour = colours.OnBackground;
                        });
                    }),
                ];
            }
        }

        private partial class StarRatingBadge(APIBeatmap beatmap) : CompositeDrawable
        {
            [BackgroundDependencyLoader]
            private void load(CardColours colours)
            {
                AutoSizeAxes = Axes.Y;
                Width = RankedPlayCard.SIZE.X - 20;

                Masking = true;
                CornerRadius = 3;

                InternalChildren =
                [
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colours.Primary,
                    },
                    new GridContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Padding = new MarginPadding { Horizontal = 3, Vertical = 1 },
                        ColumnDimensions =
                        [
                            new Dimension(GridSizeMode.AutoSize),
                            new Dimension(),
                        ],
                        RowDimensions = [new Dimension(GridSizeMode.AutoSize)],
                        Content = new Drawable[][]
                        {
                            [
                                new StarsDisplay(beatmap.StarRating)
                                {
                                    StarSize = 6,
                                    Colour = colours.OnPrimary,
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                },
                                new TruncatingSpriteText
                                {
                                    Text = FormattableString.Invariant($"{beatmap.StarRating:F2}"),
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    Font = OsuFont.GetFont(size: 9, weight: FontWeight.Bold),
                                    Colour = colours.OnPrimary,
                                },
                            ]
                        }
                    }
                ];
            }
        }

        private partial class StarsDisplay(double starRating) : CompositeDrawable
        {
            public required float StarSize { get; init; }

            [BackgroundDependencyLoader]
            private void load()
            {
                AutoSizeAxes = Axes.Both;

                FillFlowContainer flow;

                InternalChild = flow = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Spacing = new Vector2(1),
                };

                int numStars = (int)starRating - 1;

                for (int i = 0; i <= numStars; i++)
                {
                    flow.Add(new SpriteIcon
                    {
                        Size = new Vector2(StarSize),
                        Icon = FontAwesome.Solid.Star,
                    });
                }

                float lastStarWidth = (int)((starRating % 1) * 4) / 4f;

                if (lastStarWidth > 0)
                {
                    flow.Add(new Container
                    {
                        Size = new Vector2(StarSize * lastStarWidth, StarSize),
                        Masking = true,
                        Child = new SpriteIcon
                        {
                            Icon = FontAwesome.Solid.Star,
                            Size = new Vector2(StarSize),
                        }
                    });
                }
            }
        }

        private partial class DifficultyNameBadge(APIBeatmap beatmap) : CompositeDrawable
        {
            public new Axes AutoSizeAxes
            {
                get => base.AutoSizeAxes;
                set => base.AutoSizeAxes = value;
            }

            [BackgroundDependencyLoader]
            private void load(CardColours colours)
            {
                Masking = true;
                CornerRadius = 3;
                InternalChildren =
                [
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colours.BackgroundLighter,
                    },
                    new TruncatingSpriteText
                    {
                        MaxWidth = 100f,
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Text = beatmap.DifficultyName,
                        Font = OsuFont.GetFont(size: 10, weight: FontWeight.SemiBold),
                        Colour = colours.OnBackground,
                        Padding = new MarginPadding { Vertical = 1 },
                    }
                ];
            }
        }
    }
}
