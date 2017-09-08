// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Scoring;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;

namespace osu.Game.Screens.Select.Leaderboards
{
    public class Leaderboard : Container
    {
        private readonly ScrollContainer scrollContainer;
        private readonly FillFlowContainer<LeaderboardScore> scrollFlow;

        public Action<Score> ScoreSelected;

        private readonly LoadingAnimation loading;

        private IEnumerable<Score> scores;
        public IEnumerable<Score> Scores
        {
            get { return scores; }
            set
            {
                scores = value;
                getScoresRequest?.Cancel();

                int i = 150;
                if (scores == null)
                {
                    foreach (var c in scrollFlow.Children)
                        c.FadeOut(i += 10);

                    foreach (var c in scrollFlow.Children)
                        c.LifetimeEnd = Time.Current + i;

                    return;
                }

                scrollFlow.Clear();

                i = 0;
                foreach (var s in scores)
                {
                    var ls = new LeaderboardScore(s, 1 + i)
                    {
                        AlwaysPresent = true,
                        Action = () => ScoreSelected?.Invoke(s),
                        State = Visibility.Hidden,
                    };
                    scrollFlow.Add(ls);

                    using (BeginDelayedSequence(i++ * 50, true))
                        ls.Show();
                }

                scrollContainer.ScrollTo(0f, false);
            }
        }

        public Leaderboard()
        {
            Children = new Drawable[]
            {
                scrollContainer = new OsuScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    ScrollbarVisible = false,
                    Children = new Drawable[]
                    {
                        scrollFlow = new FillFlowContainer<LeaderboardScore>
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Spacing = new Vector2(0f, 5f),
                            Padding = new MarginPadding { Top = 10, Bottom = 5 },
                        },
                    },
                },
                loading = new LoadingAnimation()
            };
        }

        private APIAccess api;

        private BeatmapInfo beatmap;

        private ScheduledDelegate pendingBeatmapSwitch;

        public BeatmapInfo Beatmap
        {
            get { return beatmap; }
            set
            {
                if (beatmap == value) return;

                beatmap = value;
                Scores = null;

                pendingBeatmapSwitch?.Cancel();
                pendingBeatmapSwitch = Schedule(updateScores);
            }
        }

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(APIAccess api)
        {
            this.api = api;
        }

        private GetScoresRequest getScoresRequest;

        private void updateScores()
        {
            if (!IsLoaded) return;

            Scores = null;
            getScoresRequest?.Cancel();

            if (api == null || Beatmap == null) return;

            loading.Show();

            getScoresRequest = new GetScoresRequest(Beatmap);
            getScoresRequest.Success += r =>
            {
                Scores = r.Scores;
                loading.Hide();
            };
            api.Queue(getScoresRequest);
        }

        protected override void Update()
        {
            base.Update();

            var fadeStart = scrollContainer.Current + scrollContainer.DrawHeight;

            if (!scrollContainer.IsScrolledToEnd())
                fadeStart -= LeaderboardScore.HEIGHT;

            foreach (var c in scrollFlow.Children)
            {
                var topY = c.ToSpaceOfOtherDrawable(Vector2.Zero, scrollFlow).Y;
                var bottomY = topY + LeaderboardScore.HEIGHT;

                if (bottomY < fadeStart)
                    c.Colour = Color4.White;
                else if (topY > fadeStart + LeaderboardScore.HEIGHT)
                    c.Colour = Color4.Transparent;
                else
                {
                    c.Colour = ColourInfo.GradientVertical(
                        Color4.White.Opacity(Math.Min(1 - (topY - fadeStart) / LeaderboardScore.HEIGHT, 1)),
                        Color4.White.Opacity(Math.Min(1 - (bottomY - fadeStart) / LeaderboardScore.HEIGHT, 1)));
                }
            }
        }
    }
}
