// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osuTK;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Overlays.BeatmapSet.Scores
{
    public class ScoresContainer : Container
    {
        private const int spacing = 15;
        private const int fade_duration = 200;

        private readonly ScoreTable scoreTable;

        private readonly DrawableTopScore topScore;
        private readonly LoadingAnimation loadingAnimation;

        private bool loading
        {
            set => loadingAnimation.FadeTo(value ? 1 : 0, fade_duration);
        }

        private IEnumerable<APIScoreInfo> scores;
        private BeatmapInfo beatmap;

        public IEnumerable<APIScoreInfo> Scores
        {
            get { return scores; }
            set
            {
                getScoresRequest?.Cancel();
                scores = value;

                updateDisplay();
            }
        }

        private GetScoresRequest getScoresRequest;
        private APIAccess api;

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
                scoreTable.ClearScores();
                return;
            }

            topScore.Score = scores.FirstOrDefault();
            topScore.Show();

            scoreTable.ClearScores();

            if (scoreCount < 2)
                return;

            scoreTable.Scores = scores;
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
                        scoreTable = new ScoreTable
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                        }
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
        private void load(APIAccess api)
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
