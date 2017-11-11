// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
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
        private GetScoresRequest request;
        private APIAccess api;

        private bool isLoading
        {
            set
            {
                foreground.FadeTo(value ? 1 : 0, fade_duration);
                loadingAnimation.FadeTo(value ? 1 : 0, fade_duration);
            }
        }

        private BeatmapInfo beatmap;
        public BeatmapInfo Beatmap
        {
            set
            {
                if (beatmap == value) return;
                beatmap = value;

                updateScores();
            }
            get { return beatmap; }
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

        [BackgroundDependencyLoader]
        private void load(APIAccess api)
        {
            this.api = api;
        }

        private void updateScores()
        {
            request?.Cancel();

            if (beatmap == null)
            {
                clearAllScores();
                return;
            }

            isLoading = true;

            request = new GetScoresRequest(beatmap);
            request.Success += scores =>
            {
                var scoresAmount = scores.Scores.Count();
                if (scoresAmount == 0)
                {
                    clearAllScores();
                    return;
                }

                topScore.Score = scores.Scores.FirstOrDefault();
                topScore.Show();

                flow.Clear();

                if (scoresAmount < 2)
                {
                    isLoading = false;
                    return;
                }

                for (int i = 1; i < scoresAmount; i++)
                    flow.Add(new DrawableScore(i, scores.Scores.ElementAt(i)));

                isLoading = false;
            };
            api.Queue(request);
        }

        private void clearAllScores()
        {
            topScore.Hide();
            flow.Clear();
            isLoading = false;
        }
    }
}
