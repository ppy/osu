// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Audio;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.Edit.Audio
{
    public partial class HitSoundTable : CompositeDrawable
    {

        private const int bank_column_width = 300;
        private const int header_height = 25;

        private FillFlowContainer trackContainer = null!;

        [Resolved]
        OverlayColourProvider colours { get; set; } = null!;

        protected override void LoadComplete()
        {
            base.LoadComplete();
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Colour = colours.Background4,
                    RelativeSizeAxes = Axes.Both,
                },
                new Box
                {
                    Colour = colours.Background3,
                    RelativeSizeAxes = Axes.Y,
                    Width = bank_column_width,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = header_height,
                    Padding = new MarginPadding { Horizontal = 10 },
                    Children = new[]
                    {
                        new TableHeaderText("Sound bank")
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                        },
                        new TableHeaderText("Enables")
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Margin = new MarginPadding { Left = bank_column_width }
                        },
                    }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Top = header_height },
                    Child = trackContainer = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Spacing = new Vector2(2f),
                    }
                },
            };

            trackContainer.Add(createBankHeader("Normal"));

            foreach (string bank in HitSampleInfo.AllBanks)
            {
                trackContainer.Add(createTrackRow(bank));
            }

            trackContainer.Add(createBankHeader("Addition"));

            foreach (string bank in HitSampleInfo.AllAdditions)
            {
                trackContainer.Add(createTrackRow(bank));
            }
        }

        private Drawable createBankHeader(string title)
        {
            return new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes  = Axes.X,
                        Margin = new MarginPadding {Left = bank_column_width},
                        Anchor = Anchor.CentreLeft,
                        Height = 2,
                        Colour = colours.Background1,
                    },
                    new OsuSpriteText
                    {
                        Margin = new MarginPadding {Left = 10, Vertical = header_height / 2},
                        Text = title
                    }
                }
            };
        }

        private Drawable createTrackRow(string bank)
        {
            return new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Y,
                        Colour = colours.Background2,
                        Width = bank_column_width,
                    },
                    new OsuSpriteText
                    {
                        Text = bank,
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Padding = new MarginPadding { Left = 15 },
                    },
                    new TrackRow(),
                }
            };
        }
    }
}
