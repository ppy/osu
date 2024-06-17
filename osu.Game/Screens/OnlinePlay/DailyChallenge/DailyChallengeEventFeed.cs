// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Scoring;
using osu.Game.Users.Drawables;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.DailyChallenge
{
    public partial class DailyChallengeEventFeed : CompositeDrawable
    {
        private DailyChallengeEventFeedFlow flow = null!;

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
            var row = new NewScoreEventRow(newScoreEvent)
            {
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.BottomCentre,
            };
            flow.Add(row);
            row.Delay(15000).Then().FadeOut(300, Easing.OutQuint).Expire();
        }

        protected override void Update()
        {
            base.Update();

            for (int i = 0; i < flow.Count; ++i)
            {
                var row = flow[i];

                if (row.Y < -flow.DrawHeight)
                {
                    row.RemoveAndDisposeImmediately();
                    i -= 1;
                }
            }
        }

        public record NewScoreEvent(IScoreInfo Score, int? NewRank);

        private partial class DailyChallengeEventFeedFlow : FillFlowContainer
        {
            public override IEnumerable<Drawable> FlowingChildren => base.FlowingChildren.Reverse();
        }

        private partial class NewScoreEventRow : CompositeDrawable
        {
            private readonly NewScoreEvent newScore;

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
                    // TODO: cast is temporary, will be removed later
                    new ClickableAvatar((APIUser)newScore.Score.User)
                    {
                        Size = new Vector2(16),
                        Masking = true,
                        CornerRadius = 8,
                    },
                    text = new LinkFlowContainer(t =>
                    {
                        t.Font = OsuFont.Default.With(weight: newScore.NewRank == null ? FontWeight.Medium : FontWeight.Bold);
                        t.Colour = newScore.NewRank < 10 ? colours.Orange1 : Colour4.White;
                    })
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Padding = new MarginPadding { Left = 21 },
                    }
                };

                text.AddUserLink(newScore.Score.User);
                text.AddText(" got ");
                text.AddLink($"{newScore.Score.TotalScore:N0} points", () => { }); // TODO: present the score here

                if (newScore.NewRank != null)
                    text.AddText($" and achieved rank #{newScore.NewRank.Value:N0}");

                text.AddText("!");
            }
        }
    }
}
