// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Scoring;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Components;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Queue
{
    public partial class TierProgressBar
    {
        public partial class Segment : CompositeDrawable
        {
            protected Container BarContainer = null!;

            public Segment(int ratingWidth)
            {
                AutoSizeAxes = Axes.Y;
                Padding = new MarginPadding { Horizontal = 2 };

                RatingWidth = ratingWidth;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                InternalChildren = new Drawable[]
                {
                    BarContainer = new Container
                    {
                        Y = 15,
                        RelativeSizeAxes = Axes.X,
                        Height = 8,
                        Masking = true,
                        CornerRadius = 4,
                        Children = new Drawable[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Child = new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = GetColour(),
                                },
                                Alpha = 0.25f,
                            },
                        },
                    },
                };
            }

            public int RatingWidth { get; }

            protected virtual ColourInfo GetColour() => Color4.Black;
        }

        public partial class DivisionSegment : Segment
        {
            public readonly APIRankedPlayDivision Division;

            public readonly IBindable<float> CurrentRating = new BindableFloat();

            private Container foregroundContainer = null!;
            private CompactDivisionBadge divisionBadge = null!;

            public DivisionSegment(APIRankedPlayDivision division)
                : base(division.Width)
            {
                Division = division;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                BarContainer.Add(foregroundContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    CornerRadius = 4,
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = GetColour(),
                    },
                });

                AddRangeInternal(new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Font = OsuFont.Torus.With(size: 12, weight: FontWeight.SemiBold),
                        Colour = Colour4.White,
                        Text = Division.StartRating.ToLocalisableString(@"N0"),
                    },
                    // TODO: display actual player rank
                    divisionBadge = new CompactDivisionBadge(Division, 100)
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Y = 27,
                    },
                });
            }

            protected override ColourInfo GetColour() => Division.Tier switch
            {
                RankingTier.Bronze => ColourInfo.GradientVertical(Colour4.FromHex(@"#B88F7A"), Colour4.FromHex(@"#855C47")),
                RankingTier.Silver => ColourInfo.GradientVertical(Colour4.FromHex(@"#E0E0EB"), Colour4.FromHex(@"#A3A3C2")),
                RankingTier.Gold => ColourInfo.GradientVertical(Colour4.FromHex(@"#FFEE99"), Colour4.FromHex(@"#FFDD33")),
                RankingTier.Platinum => ColourInfo.GradientVertical(Colour4.FromHex(@"#A8F0EF"), Colour4.FromHex(@"#52E0DF")),
                RankingTier.Rhodium => ColourInfo.GradientVertical(Colour4.FromHex(@"#ADFFB6"), Colour4.FromHex(@"#6DB566")),
                RankingTier.Radiant => ColourInfo.GradientVertical(Colour4.FromHex(@"#97DCFF"), Colour4.FromHex(@"#ED82FF")),
                RankingTier.Lustrous => ColourInfo.GradientVertical(Colour4.FromHex(@"#FFE600"), Colour4.FromHex(@"#ED82FF")),
                _ => throw new ArgumentOutOfRangeException()
            };

            protected override void LoadComplete()
            {
                base.LoadComplete();

                CurrentRating.BindValueChanged(onRatingChanged, true);
            }

            private void onRatingChanged(ValueChangedEvent<float> e)
            {
                if (e.NewValue < Division.StartRating)
                {
                    foregroundContainer.Width = 0;

                    // Lustrous badges show the user's current rank, so it wouldn't make sense
                    // to display a faded out badge in the Lustrous segment if the user isn't
                    // currently in said tier.
                    divisionBadge.Alpha = Division.Tier == RankingTier.Lustrous ? 0 : 0.5f;
                }
                else if (e.NewValue >= Division.EndRating)
                {
                    foregroundContainer.Width = 1;
                    divisionBadge.Alpha = 1;
                }
                else
                {
                    foregroundContainer.Width = (e.NewValue - Division.StartRating) / Division.Width;
                    divisionBadge.Alpha = 1;
                }

                // For Lustrous, the badge is following the player's current rating,
                // while also showing their current rank.
                if (Division.Tier == RankingTier.Lustrous && CurrentRating.Value >= Division.StartRating)
                {
                    divisionBadge.RelativePositionAxes = Axes.X;
                    divisionBadge.Anchor = Anchor.TopLeft;
                    divisionBadge.X = (e.NewValue - Division.StartRating) / Division.Width;
                }
                else
                {
                    divisionBadge.RelativePositionAxes = Axes.None;
                    divisionBadge.Anchor = Anchor.TopCentre;
                    divisionBadge.X = 0;
                }
            }
        }
    }
}
