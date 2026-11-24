// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Localisation;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.RankedPlay;
using osu.Game.Scoring;
using FontWeight = osu.Game.Graphics.FontWeight;
using OsuFont = osu.Game.Graphics.OsuFont;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Queue
{
    public partial class TierProgressBar
    {
        public partial class RatingToNextDivisionDisplay : CompositeDrawable
        {
            private readonly NineSliceSprite background;
            private readonly OsuTextFlowContainer textFlow;

            public readonly IBindable<int> CurrentRating = new BindableInt();

            [Resolved]
            private IRankedPlayDivisionStore divisionStore { get; set; } = null!;

            public RatingToNextDivisionDisplay()
            {
                AutoSizeAxes = Axes.X;
                Height = 22;
                Origin = Anchor.CentreRight;

                InternalChildren = new Drawable[]
                {
                    background = new NineSliceSprite
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        TextureInset = new MarginPadding { Left = 4, Right = 11 },
                    },
                    textFlow = new OsuTextFlowContainer(p => p.Font = OsuFont.Torus.With(size: 12))
                    {
                        AutoSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Padding = new MarginPadding { Left = 15, Right = 20 },
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                background.Texture = textures.Get(@"Online/RankedPlay/to-next-division-display");
                background.Texture.ScaleAdjust = 4; // needs to be set for the nine-slice scaling to work correctly
            }

            public void UpdateLabel()
            {
                textFlow.Clear();

                divisionStore.GetDivisionForRating(CurrentRating.Value).ContinueWith(result =>
                {
                    var division = result.GetResultSafely();

                    if (division == null)
                        return;

                    Schedule(() =>
                    {
                        int remaining = division.EndRating - CurrentRating.Value + 1;

                        textFlow.AddText(remaining.ToLocalisableString(@"N0"), p => p.Font = p.Font.With(weight: FontWeight.Bold));
                        textFlow.AddText(" ");
                        textFlow.AddText(RankedPlayStrings.RatingToNextDivision);

                        // This uses a different set of colours from `OsuColour.ForRankingTier` as trying to replicate
                        // the exact colour mixing from Figma inside framework ended up being too much of a pain.
                        background.Colour = getColour(division);
                    });
                });
            }

            private ColourInfo getColour(APIRankedPlayDivision division) => division.Tier switch
            {
                RankingTier.Bronze => ColourInfo.GradientHorizontal(Colour4.FromHex(@"#242521"), Colour4.FromHex(@"#B88F7A")),
                RankingTier.Silver => ColourInfo.GradientHorizontal(Colour4.FromHex(@"#272C2D"), Colour4.FromHex(@"#E0E0EB")),
                RankingTier.Gold => ColourInfo.GradientHorizontal(Colour4.FromHex(@"#30321F"), Colour4.FromHex(@"#FFEE99")),
                RankingTier.Platinum => ColourInfo.GradientHorizontal(Colour4.FromHex(@"#1F3230"), Colour4.FromHex(@"#A8F0EF")),
                RankingTier.Rhodium => ColourInfo.GradientHorizontal(Colour4.FromHex(@"#1F2920"), Colour4.FromHex(@"#ADFFB6")),
                RankingTier.Radiant => ColourInfo.GradientHorizontal(Colour4.FromHex(@"#2E2933"), Colour4.FromHex(@"#97DCFF")),
                RankingTier.Lustrous => ColourInfo.GradientHorizontal(Colour4.FromHex(@"#2E2933"), Colour4.FromHex(@"#FFE600")),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
