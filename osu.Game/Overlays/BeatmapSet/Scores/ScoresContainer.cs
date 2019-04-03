// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Scoring;

namespace osu.Game.Overlays.BeatmapSet.Scores
{
    public class ScoresContainer : Container
    {
        private const int spacing = 15;
        private const int fade_duration = 200;

        private readonly FillFlowContainer flow;
        private readonly DrawableTopScore topScore;
        private readonly LoadingAnimation loadingAnimation;

        private bool loading
        {
            set => loadingAnimation.FadeTo(value ? 1 : 0, fade_duration);
        }

        private IEnumerable<ScoreInfo> scores;
        private BeatmapInfo beatmap;

        public IEnumerable<ScoreInfo> Scores
        {
            get => scores;
            set
            {
                getScoresRequest?.Cancel();
                scores = value;

                updateDisplay();
            }
        }

        private GetScoresRequest getScoresRequest;
        private IAPIProvider api;

        public BeatmapInfo Beatmap
        {
            get => beatmap;
            set
            {
                beatmap = value;

                Scores = null;

                if (beatmap?.OnlineBeatmapID.HasValue != true)
                    return;

                loading = true;

                getScoresRequest = new GetScoresRequest(beatmap, beatmap.Ruleset);
                getScoresRequest.Success += r => Schedule(() => Scores = r.Scores);
                api.Queue(getScoresRequest);
            }
        }

        private void updateDisplay()
        {
            loading = false;

            var scoreCount = scores?.Count() ?? 0;

            if (scoreCount == 0)
            {
                topScore.Hide();
                flow.Clear();
                return;
            }

            topScore.Score = scores.FirstOrDefault();
            topScore.Show();

            flow.Clear();

            if (scoreCount < 2)
                return;

            for (int i = 1; i < scoreCount; i++)
                flow.Add(new DrawableScore(i, scores.ElementAt(i)));
        }

        public ScoresContainer()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Width = 0.95f,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, spacing),
                    Margin = new MarginPadding { Vertical = spacing },
                    Children = new Drawable[]
                    {
                        topScore = new DrawableTopScore(),
                        flow = new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(0, 1),
                        },
                    }
                },
                loadingAnimation = new LoadingAnimation
                {
                    Alpha = 0,
                    Margin = new MarginPadding(20)
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(IAPIProvider api)
        {
            this.api = api;
            updateDisplay();
        }

        protected override void Dispose(bool isDisposing)
        {
            getScoresRequest?.Cancel();
        }
    }
}
