// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Metadata;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Screens.OnlinePlay.DailyChallenge.Events;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.DailyChallenge
{
    public partial class DailyChallengeScoreBreakdown : CompositeDrawable
    {
        public Bindable<MultiplayerScore?> UserBestScore { get; } = new Bindable<MultiplayerScore?>();

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
                barsContainer.Add(new Bar(100_000 * i, 100_000 * (i + 1) - 1)
                {
                    Width = 1f / bin_count,
                });
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            UserBestScore.BindValueChanged(_ =>
            {
                foreach (var bar in barsContainer)
                    bar.ContainsLocalUser.Value = UserBestScore.Value is not null && bar.BinStart <= UserBestScore.Value.TotalScore && UserBestScore.Value.TotalScore <= bar.BinEnd;
            });
        }

        private readonly Queue<NewScoreEvent> newScores = new Queue<NewScoreEvent>();

        public void AddNewScore(NewScoreEvent newScoreEvent)
        {
            newScores.Enqueue(newScoreEvent);

            // ensure things don't get too out-of-hand.
            if (newScores.Count > 25)
            {
                bins[getTargetBin(newScores.Dequeue())] += 1;
                Scheduler.AddOnce(updateCounts);
            }
        }

        private double lastScoreDisplay;

        protected override void Update()
        {
            base.Update();

            if (Time.Current - lastScoreDisplay > 150 && newScores.TryDequeue(out var newScore))
            {
                if (lastScoreDisplay < Time.Current)
                    lastScoreDisplay = Time.Current;

                int targetBin = getTargetBin(newScore);
                bins[targetBin] += 1;

                updateCounts();

                var text = new OsuSpriteText
                {
                    Text = newScore.TotalScore.ToString(@"N0"),
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

                lastScoreDisplay = Time.Current;
            }
        }

        public void SetInitialCounts(long[] counts)
        {
            if (counts.Length != bin_count)
                throw new ArgumentException(@"Incorrect number of bins.", nameof(counts));

            bins = counts;
            updateCounts();
        }

        private static int getTargetBin(NewScoreEvent score) =>
            (int)Math.Clamp(Math.Floor((float)score.TotalScore / 100000), 0, bin_count - 1);

        private void updateCounts()
        {
            long max = Math.Max(bins.Max(), 1);
            for (int i = 0; i < bin_count; ++i)
                barsContainer[i].UpdateCounts(bins[i], max);
        }

        private partial class Bar : CompositeDrawable, IHasTooltip
        {
            public BindableBool ContainsLocalUser { get; } = new BindableBool();

            public readonly int BinStart;
            public readonly int BinEnd;

            private long count;
            private long max;

            public Container CircularBar { get; private set; } = null!;

            private Box fill = null!;
            private Box flashLayer = null!;
            private OsuSpriteText userIndicator = null!;

            public Bar(int binStart, int binEnd)
            {
                BinStart = binStart;
                BinEnd = binEnd;
            }

            [Resolved]
            private OverlayColourProvider colourProvider { get; set; } = null!;

            [Resolved]
            private OsuColour colours { get; set; } = null!;

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
                    Children = new Drawable[]
                    {
                        CircularBar = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            Height = 0.01f,
                            Masking = true,
                            CornerRadius = 10,
                            Children = new Drawable[]
                            {
                                fill = new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                },
                                flashLayer = new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Alpha = 0,
                                },
                            }
                        },
                        userIndicator = new OsuSpriteText
                        {
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            Colour = colours.Orange1,
                            Text = "You",
                            Font = OsuFont.Default.With(weight: FontWeight.Bold),
                            Alpha = 0,
                            RelativePositionAxes = Axes.Y,
                            Margin = new MarginPadding { Bottom = 5, },
                        }
                    },
                });

                string? label = null;

                switch (BinStart)
                {
                    case 200_000:
                    case 400_000:
                    case 600_000:
                    case 800_000:
                        label = @$"{BinStart / 1000}k";
                        break;

                    case 1_000_000:
                        label = @"1M";
                        break;
                }

                if (label != null)
                {
                    AddInternal(new OsuSpriteText
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomCentre,
                        Text = label,
                        Colour = colourProvider.Content2,
                    });
                }
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                ContainsLocalUser.BindValueChanged(_ =>
                {
                    fill.FadeColour(ContainsLocalUser.Value ? colours.Orange1 : colourProvider.Highlight1, 300, Easing.OutQuint);
                    userIndicator.FadeTo(ContainsLocalUser.Value ? 1 : 0, 300, Easing.OutQuint);
                }, true);
                FinishTransforms(true);
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

                float height = 0.01f + 0.99f * count / max;
                CircularBar.ResizeHeightTo(height, 300, Easing.OutQuint);
                userIndicator.MoveToY(-height, 300, Easing.OutQuint);
                if (isIncrement)
                    flashLayer.FadeOutFromOne(600, Easing.OutQuint);
            }

            public LocalisableString TooltipText => LocalisableString.Format("{0:N0} passes in {1:N0} - {2:N0} range", count, BinStart, BinEnd);
        }
    }
}
