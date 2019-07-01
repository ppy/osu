// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osuTK;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Scoring;

namespace osu.Game.Overlays.BeatmapSet.Scores
{
    public class ScoresContainer : CompositeDrawable
    {
        private const int spacing = 15;
        private const int fade_duration = 200;

        private readonly Box background;
        private readonly ScoreTable scoreTable;

        private readonly DrawableTopScore topScore;
        private readonly LoadingAnimation loadingAnimation;

        [Resolved]
        private IAPIProvider api { get; set; }

        public ScoresContainer()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChildren = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                },
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
        private void load(OsuColour colours)
        {
            background.Colour = colours.Gray2;
            updateDisplay();
        }

        private bool loading
        {
            set => loadingAnimation.FadeTo(value ? 1 : 0, fade_duration);
        }

        private GetScoresRequest getScoresRequest;
        private IReadOnlyList<ScoreInfo> scores;

        public IReadOnlyList<ScoreInfo> Scores
        {
            get => scores;
            set
            {
                getScoresRequest?.Cancel();
                scores = value;

                updateDisplay();
            }
        }

        private BeatmapInfo beatmap;

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

            scoreTable.Scores = scores?.Count > 1 ? scores : new List<ScoreInfo>();
            scoreTable.FadeTo(scores?.Count > 1 ? 1 : 0);

            if (scores?.Any() == true)
            {
                topScore.Score = scores.FirstOrDefault();
                topScore.Show();
            }
            else
                topScore.Hide();
        }

        protected override void Dispose(bool isDisposing)
        {
            getScoresRequest?.Cancel();
        }
    }
}
