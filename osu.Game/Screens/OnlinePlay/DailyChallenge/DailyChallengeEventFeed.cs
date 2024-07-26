// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.OnlinePlay.DailyChallenge.Events;
using osu.Game.Users.Drawables;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.DailyChallenge
{
    public partial class DailyChallengeEventFeed : CompositeDrawable
    {
        private DailyChallengeEventFeedFlow flow = null!;

        public Action<long>? PresentScore { get; init; }

        private readonly Queue<NewScoreEvent> newScores = new Queue<NewScoreEvent>();

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                new SectionHeader("Events"),
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Top = 35 },
                    Child = flow = new DailyChallengeEventFeedFlow
                    {
                        RelativeSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Origin = Anchor.BottomCentre,
                        Anchor = Anchor.BottomCentre,
                        Spacing = new Vector2(5),
                        Masking = true,
                    }
                }
            };
        }

        public void AddNewScore(NewScoreEvent newScoreEvent)
        {
            newScores.Enqueue(newScoreEvent);

            // ensure things don't get too out-of-hand.
            if (newScores.Count > 25)
                newScores.Dequeue();
        }

        protected override void Update()
        {
            base.Update();

            while (newScores.TryDequeue(out var newScore))
            {
                flow.Add(new NewScoreEventRow(newScore)
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    PresentScore = PresentScore,
                });
            }

            for (int i = 0; i < flow.Count; ++i)
            {
                var row = flow[i];

                row.Alpha = Interpolation.ValueAt(Math.Clamp(row.Y + flow.DrawHeight, 0, flow.DrawHeight), 0f, 1f, 0, flow.DrawHeight, Easing.Out);

                if (row.Y < -flow.DrawHeight)
                {
                    row.RemoveAndDisposeImmediately();
                    i -= 1;
                }
            }
        }

        private partial class DailyChallengeEventFeedFlow : FillFlowContainer
        {
            public override IEnumerable<Drawable> FlowingChildren => base.FlowingChildren.Reverse();
        }

        private partial class NewScoreEventRow : CompositeDrawable
        {
            private readonly NewScoreEvent newScore;

            public Action<long>? PresentScore { get; init; }

            public NewScoreEventRow(NewScoreEvent newScore)
            {
                this.newScore = newScore;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                LinkFlowContainer text;

                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                AutoSizeDuration = 300;
                AutoSizeEasing = Easing.OutQuint;

                InternalChildren = new Drawable[]
                {
                    new ClickableAvatar(newScore.User)
                    {
                        Size = new Vector2(16),
                        Masking = true,
                        CornerRadius = 8,
                    },
                    text = new LinkFlowContainer(t =>
                    {
                        FontWeight fontWeight = FontWeight.Medium;

                        if (newScore.NewRank < 100)
                            fontWeight = FontWeight.Bold;
                        else if (newScore.NewRank < 1000)
                            fontWeight = FontWeight.SemiBold;

                        t.Font = OsuFont.Default.With(weight: fontWeight);
                        t.Colour = newScore.NewRank < 10 ? colours.Orange1 : Colour4.White;
                    })
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Padding = new MarginPadding { Left = 21 },
                    }
                };

                text.AddUserLink(newScore.User);
                text.AddText(" scored ");
                text.AddLink($"{newScore.TotalScore:N0}", () => PresentScore?.Invoke(newScore.ScoreID));

                if (newScore.NewRank != null)
                    text.AddText($" and achieved rank #{newScore.NewRank.Value:N0}");

                text.AddText("!");
            }
        }
    }
}
