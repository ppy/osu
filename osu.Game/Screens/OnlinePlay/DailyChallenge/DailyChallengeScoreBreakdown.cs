// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
// using System.Data;
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
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Screens.OnlinePlay.DailyChallenge.Events;
using osuTK;
using osu.Game.Rulesets;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Game.Utils;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Screens.OnlinePlay.DailyChallenge
{
    public partial class DailyChallengeScoreBreakdown : CompositeDrawable
    {
        public Bindable<MultiplayerScore?> UserBestScore { get; } = new Bindable<MultiplayerScore?>();

        private FillFlowContainer<Bar> barsContainer = null!;
        private int numberOfBars;
        private int barRangeValue;
        private long[] bins = null!;

        private PlaylistItem item = null!;
        private readonly Room room = null!;

        public DailyChallengeScoreBreakdown(Room room)
        {
            this.room = room;
        }

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
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
            item = room.Playlist.Single();
            var rulesetInstance = rulesets.GetRuleset(item.RulesetID)!.CreateInstance();
            IEnumerable<Mod> allowedMods = item.AllowedMods.Select(m => m.ToMod(rulesetInstance));
            IEnumerable<Mod> requiredMods = item.RequiredMods.Select(m => m.ToMod(rulesetInstance));
            List<Mod> bestMods = [];

            foreach (Mod allowedMod in allowedMods)
            {
                if (allowedMod.ScoreMultiplier > 1 && allowedMod.Ranked)
                {
                    bestMods.Add(allowedMod);
                }
            }

            bestMods = bestMods.OrderByDescending(mod => mod.ScoreMultiplier).ToList();
            bestMods.InsertRange(0, requiredMods);

            for (int i = 0; i < bestMods.Count; i++)
            {
                foreach (Type type in bestMods[i].IncompatibleMods)
                {
                    foreach (var invalid in bestMods.Where(m => type.IsInstanceOfType(m)).ToList())
                    {
                        if (invalid == bestMods[i])
                            continue;

                        bestMods.Remove(invalid);
                    }
                }
            }

            if (!ModUtils.CheckCompatibleSet(bestMods, out var invalidMods))
            {
                throw new InvalidOperationException($"incompatibe mods found. Invalid mods: {string.Join(", ", invalidMods.Select(m => m.Name))}");
            }

            // there is a small edge case where this is not the actual max score
            // for example, if mod A and B were both 1.15x and mod C was 1.16x and was incompatible with A and B
            // then A and B would be removed, and if they were incompatible only with C, then a higher score
            // would be possible with those two rather than just C, but it seems very unlikely to happen
            int theoreticalMax = 1_000_000;
            double multiplier = 1;

            foreach (Mod bestMod in bestMods)
            {
                multiplier *= bestMod.ScoreMultiplier;
            }

            theoreticalMax = (int)(theoreticalMax * multiplier);
            Console.WriteLine(theoreticalMax);
            // barRangeValue is set to 100_000 but implementation for other ranges is implemented
            // However, the server always sends the scores in 13 bins, so I'm unable to accomadate any other ranges
            // If it is made possible to request bin ranges to the server, the code could be changed to accomadate that
            (numberOfBars, barRangeValue) = (theoreticalMax / 100_000 + 1, 100_000);
            bins = new long[numberOfBars];

            for (int i = 0; i < numberOfBars; i++)
            {
                barsContainer.Add(new Bar(barRangeValue * i, barRangeValue * (i + 1) - 1, i)
                {
                    Width = 1f / numberOfBars,
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
                bins[getTargetBin(newScores.Dequeue(), numberOfBars, barRangeValue)] += 1;
                Scheduler.AddOnce(updateCounts);
            }
        }

        public void RescaleBar(int? numberOfBars = null, int? barRangeValue = null)
        {
            if (numberOfBars.HasValue)
            {
                this.numberOfBars = numberOfBars.Value;
                barsContainer.Clear();
                bins = new long[this.numberOfBars];
            }

            if (barRangeValue.HasValue)
            {
                this.barRangeValue = barRangeValue.Value;
                barsContainer.Clear();
                bins = new long[this.numberOfBars];
            }

            for (int bar = 0; bar < this.numberOfBars; bar++)
            {
                barsContainer.Add(new Bar(this.barRangeValue * bar, this.barRangeValue * (bar + 1) - 1, bar)
                {
                    Width = 1f / this.numberOfBars,
                });
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

                int targetBin = getTargetBin(newScore, numberOfBars, barRangeValue);
                bins[targetBin] += 1;

                updateCounts();

                var text = new OsuSpriteText
                {
                    Text = newScore.TotalScore.ToString(@"N0"),
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.BottomCentre,
                    Font = OsuFont.Default.With(size: 30),
                    RelativePositionAxes = Axes.X,
                    X = (targetBin + 0.5f) / numberOfBars - 0.5f,
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

        public void SetInitialCounts(long[]? counts = null, bool fits = false)
        {
            if (counts.IsNull())
            {
                // just adds values in the shape of sort of a bell curve ig
                counts = new long[numberOfBars];

                for (int i = 0; i < Math.Round((float)numberOfBars / 2, MidpointRounding.AwayFromZero); i++)
                {
                    counts[i] = i * i;
                    counts[numberOfBars - i - 1] = i * i;
                }
            }
            else if (!fits)
            {
                if (counts.Length < numberOfBars)
                {
                    long[] temp = new long[numberOfBars];
                    Array.Copy(counts, temp, counts.Length);
                    counts = temp;
                }
                else if (counts.Length >= numberOfBars)
                {
                    counts = counts.Take(numberOfBars).ToArray();
                }
            }
            else if (fits && counts.Length != numberOfBars)
            {
                throw new ArgumentException(@"Incorrect number of bins.", nameof(counts));
            }

            bins = counts;
            updateCounts();
        }

        private static int getTargetBin(NewScoreEvent score, int binCount, int range) =>
            (int)Math.Clamp(Math.Floor((float)score.TotalScore / range), 0, binCount - 1);

        private void updateCounts()
        {
            long max = Math.Max(bins.Max(), 1);
            for (int i = 0; i < numberOfBars; ++i)
                barsContainer[i].UpdateCounts(bins[i], max);
        }

        private partial class Bar : CompositeDrawable, IHasTooltip
        {
            public BindableBool ContainsLocalUser { get; } = new BindableBool();

            public readonly int BinStart;
            public readonly int BinEnd;
            public readonly int BinNumber;

            private long count;
            private long max;

            public Container CircularBar { get; private set; } = null!;

            private Box fill = null!;
            private Box flashLayer = null!;
            private OsuSpriteText userIndicator = null!;

            public Bar(int binStart, int binEnd, int binNumber)
            {
                BinStart = binStart;
                BinEnd = binEnd;
                BinNumber = binNumber;
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

                if (BinNumber % 2 == 0 && BinNumber != 0)
                {
                    if (BinStart < 1_000_000)
                    {
                        label = @$"{BinStart / 1_000}k";
                    }
                    else if (BinStart >= 1_000_000)
                    {
                        label = @$"{BinStart / 1_000_000f}M";
                    }
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
