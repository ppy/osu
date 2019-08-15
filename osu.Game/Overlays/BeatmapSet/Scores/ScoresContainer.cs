// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osuTK;
using System.Linq;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;

namespace osu.Game.Overlays.BeatmapSet.Scores
{
    public class ScoresContainer : CompositeDrawable
    {
        private const int spacing = 15;

        private readonly Box background;
        private readonly ScoreTable scoreTable;
        private readonly FillFlowContainer topScoresContainer;
        private readonly LoadingAnimation loadingAnimation;

        [Resolved]
        private IAPIProvider api { get; set; }

        private GetScoresRequest getScoresRequest;

        private BeatmapInfo beatmap;

        public BeatmapInfo Beatmap
        {
            get => beatmap;
            set
            {
                if (beatmap == value)
                    return;

                beatmap = value;

                getScores(beatmap);
            }
        }

        protected APILegacyScores Scores
        {
            set
            {
                Schedule(() =>
                {
                    topScoresContainer.Clear();

                    if (value?.Scores.Any() != true)
                    {
                        scoreTable.Scores = null;
                        scoreTable.Hide();
                        return;
                    }

                    scoreTable.Scores = value.Scores;
                    scoreTable.Show();

                    var topScore = value.Scores.First();
                    var userScore = value.UserScore;

                    topScoresContainer.Add(new DrawableTopScore(topScore));

                    if (userScore != null && userScore.Score.OnlineScoreID != topScore.OnlineScoreID)
                        topScoresContainer.Add(new DrawableTopScore(userScore.Score, userScore.Position));
                });
            }
        }

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
        }

        private void getScores(BeatmapInfo beatmap)
        {
            getScoresRequest?.Cancel();
            getScoresRequest = null;

            Scores = null;

            if (beatmap?.OnlineBeatmapID.HasValue != true || beatmap.Status <= BeatmapSetOnlineStatus.Pending)
                return;

            loadingAnimation.Show();
            getScoresRequest = new GetScoresRequest(beatmap, beatmap.Ruleset);
            getScoresRequest.Success += scores =>
            {
                loadingAnimation.Hide();
                Scores = scores;
            };
            api.Queue(getScoresRequest);
        }
    }
}
