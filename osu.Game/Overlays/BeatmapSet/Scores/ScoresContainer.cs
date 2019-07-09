// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osuTK;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Overlays.BeatmapSet.Scores
{
    public class ScoresContainer : CompositeDrawable
    {
        private const int spacing = 15;
        private const int fade_duration = 200;

        private readonly Box background;
        private readonly ScoreTable scoreTable;

        private readonly FillFlowContainer topScoresContainer;
        private readonly LoadingAnimation loadingAnimation;

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
                        topScoresContainer = new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(0, 5),
                        },
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
                    Margin = new MarginPadding(20),
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            background.Colour = colours.Gray2;
            updateDisplay();
        }

        public bool Loading
        {
            set
            {
                loadingAnimation.FadeTo(value ? 1 : 0, fade_duration);

                if (value)
                    Scores = null;
            }
        }

        private APILegacyScores scores;

        public APILegacyScores Scores
        {
            get => scores;
            set
            {
                scores = value;

                updateDisplay();
            }
        }

        private void updateDisplay()
        {
            topScoresContainer.Clear();

            scoreTable.Scores = scores?.Scores.Count > 1 ? scores.Scores : new List<APILegacyScoreInfo>();
            scoreTable.FadeTo(scores?.Scores.Count > 1 ? 1 : 0);

            if (scores?.Scores.Any() == true)
            {
                topScoresContainer.Add(new DrawableTopScore(scores.Scores.FirstOrDefault()));

                var userScore = scores.UserScore;

                if (userScore != null && userScore.Position != 1)
                    topScoresContainer.Add(new DrawableTopScore(userScore.Score, userScore.Position));
            }
        }
    }
}
