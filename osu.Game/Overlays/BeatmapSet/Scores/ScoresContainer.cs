// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Overlays.BeatmapSet.Scores
{
    public class ScoresContainer : Container
    {
        private const int spacing = 15;
        private const int fade_duration = 200;

        private readonly FillFlowContainer flow;
        private readonly DrawableTopScore topScore;
        private readonly LoadingAnimation loadingAnimation;
        private readonly Box foreground;

        private bool isLoading;
        public bool IsLoading
        {
            set
            {
                if (isLoading == value) return;
                isLoading = value;

                foreground.FadeTo(isLoading ? 1 : 0, fade_duration);
                loadingAnimation.FadeTo(isLoading ? 1 : 0, fade_duration);
            }
            get { return isLoading; }
        }

        private IEnumerable<OnlineScore> scores;
        public IEnumerable<OnlineScore> Scores
        {
            set
            {
                if (scores == value)
                {
                    IsLoading = false;
                    return;
                }
                scores = value;

                updateScores();
            }
            get { return scores; }
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
                    Margin = new MarginPadding { Top = spacing },
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
                foreground = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black.Opacity(0.7f),
                    Alpha = 0,
                },
                loadingAnimation = new LoadingAnimation
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Size = new Vector2(20),
                    Margin = new MarginPadding { Top = 10 },
                    Alpha = 0,
                },
            };
        }

        private void updateScores()
        {
            var scoresAmount = scores.Count();
            if (scoresAmount == 0)
            {
                CleanAllScores();
                return;
            }

            topScore.Score = scores.FirstOrDefault();
            topScore.Show();

            flow.Clear();

            if (scoresAmount < 2)
            {
                IsLoading = false;
                return;
            }

            for (int i = 1; i < scoresAmount; i++)
                flow.Add(new DrawableScore(i, scores.ElementAt(i)));

            IsLoading = false;
        }

        public void CleanAllScores()
        {
            topScore.Hide();
            flow.Clear();
            IsLoading = false;
        }
    }
}
