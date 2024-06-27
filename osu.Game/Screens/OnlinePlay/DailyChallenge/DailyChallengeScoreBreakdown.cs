// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Metadata;
using osu.Game.Overlays;
using osu.Game.Scoring;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.DailyChallenge
{
    public partial class DailyChallengeScoreBreakdown : CompositeDrawable
    {
        private FillFlowContainer<Bar> barsContainer = null!;

        private const int bin_count = MultiplayerPlaylistItemStats.TOTAL_SCORE_DISTRIBUTION_BINS;
        private long[] bins = new long[bin_count];

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                new SectionHeader("Score breakdown"),
                barsContainer = new FillFlowContainer<Bar>
                {
                    Direction = FillDirection.Horizontal,
                    RelativeSizeAxes = Axes.Both,
                    Height = 0.9f,
                    Padding = new MarginPadding { Top = 35 },
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                }
            };

            for (int i = 0; i < bin_count; ++i)
            {
                LocalisableString? label = null;

                switch (i)
                {
                    case 2:
                    case 4:
                    case 6:
                    case 8:
                        label = @$"{100 * i}k";
                        break;

                    case 10:
                        label = @"1M";
                        break;
                }

                barsContainer.Add(new Bar(label)
                {
                    Width = 1f / bin_count,
                });
            }
        }

        public void AddNewScore(IScoreInfo scoreInfo)
        {
            int targetBin = (int)Math.Clamp(Math.Floor((float)scoreInfo.TotalScore / 100000), 0, bin_count - 1);
            bins[targetBin] += 1;
            updateCounts();

            var text = new OsuSpriteText
            {
                Text = scoreInfo.TotalScore.ToString(@"N0"),
                Anchor = Anchor.TopCentre,
                Origin = Anchor.BottomCentre,
                Font = OsuFont.Default.With(size: 30),
                RelativePositionAxes = Axes.X,
                X = (targetBin + 0.5f) / bin_count - 0.5f,
                Alpha = 0,
            };
            AddInternal(text);

            Scheduler.AddDelayed(() =>
            {
                float startY = ToLocalSpace(barsContainer[targetBin].CircularBar.ScreenSpaceDrawQuad.TopLeft).Y;
                text.FadeInFromZero()
                    .ScaleTo(new Vector2(0.8f), 500, Easing.OutElasticHalf)
                    .MoveToY(startY)
                    .MoveToOffset(new Vector2(0, -50), 2500, Easing.OutQuint)
                    .FadeOut(2500, Easing.OutQuint)
                    .Expire();
            }, 150);
        }

        public void SetInitialCounts(long[] counts)
        {
            if (counts.Length != bin_count)
                throw new ArgumentException(@"Incorrect number of bins.", nameof(counts));

            bins = counts;
            updateCounts();
        }

        private void updateCounts()
        {
            long max = bins.Max();
            for (int i = 0; i < bin_count; ++i)
                barsContainer[i].UpdateCounts(bins[i], max);
        }

        private partial class Bar : CompositeDrawable
        {
            private readonly LocalisableString? label;

            private long count;
            private long max;

            public Container CircularBar { get; private set; } = null!;

            public Bar(LocalisableString? label = null)
            {
                this.label = label;
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                RelativeSizeAxes = Axes.Both;

                AddInternal(new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding
                    {
                        Bottom = 20,
                        Horizontal = 3,
                    },
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Masking = true,
                    Child = CircularBar = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        Height = 0.01f,
                        Masking = true,
                        CornerRadius = 10,
                        Colour = colourProvider.Highlight1,
                        Child = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                        }
                    }
                });

                if (label != null)
                {
                    AddInternal(new OsuSpriteText
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomCentre,
                        Text = label.Value,
                        Colour = colourProvider.Content2,
                    });
                }
            }

            protected override void Update()
            {
                base.Update();

                CircularBar.CornerRadius = Math.Min(CircularBar.DrawHeight / 2, CircularBar.DrawWidth / 4);
            }

            public void UpdateCounts(long newCount, long newMax)
            {
                bool isIncrement = newCount > count;

                count = newCount;
                max = newMax;

                CircularBar.ResizeHeightTo(0.01f + 0.99f * count / max, 300, Easing.OutQuint);
                if (isIncrement)
                    CircularBar.FlashColour(Colour4.White, 600, Easing.OutQuint);
            }
        }
    }
}
