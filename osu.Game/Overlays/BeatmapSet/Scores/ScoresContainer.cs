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
using osu.Game.Screens.Select.Leaderboards;
using osu.Framework.Bindables;
using System.Collections.Generic;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Overlays.BeatmapSet.Scores
{
    public class ScoresContainer : Container
    {
        private const int spacing = 15;

        public Bindable<BeatmapInfo> Beatmap = new Bindable<BeatmapInfo>();

        private ScoreTable scoreTable;
        private FillFlowContainer content;
        private FillFlowContainer topScoresContainer;
        private LoadingAnimation loadingAnimation;
        private LeaderboardScopeSelector scopeSelector;
        private LeaderboardModSelector modSelector;

        private GetScoresRequest getScoresRequest;

        [Resolved]
        private IAPIProvider api { get; set; }

        protected override Container<Drawable> Content => content;

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
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            AddRangeInternal(new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colours.Gray2
                },
                content = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, spacing),
                    Margin = new MarginPadding { Vertical = spacing },
                }
            });

            if (api.LocalUser.Value.IsSupporter)
                AddRange(new Drawable[]
                {
                    scopeSelector = new LeaderboardScopeSelector(),
                    modSelector = new LeaderboardModSelector()
                });

            Add(new Container
            {
                AutoSizeAxes = Axes.Y,
                RelativeSizeAxes = Axes.X,
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
                    }
                }
            });

            scopeSelector?.Current.BindValueChanged(scope => getScores(scope.NewValue, modSelector.SelectedMods.Value));
            modSelector?.SelectedMods.BindValueChanged(mods => getScores(scopeSelector.Current.Value, mods.NewValue));
            Beatmap.BindValueChanged(beatmap =>
            {
                if (scopeSelector != null)
                    scopeSelector.Current.Value = BeatmapLeaderboardScope.Global;

                modSelector?.ResetRuleset(beatmap.NewValue?.Ruleset);

                getScores();
            });
        }

        private void getScores(BeatmapLeaderboardScope scope = BeatmapLeaderboardScope.Global, IEnumerable<Mod> mods = null)
        {
            getScoresRequest?.Cancel();
            getScoresRequest = null;

            Scores = null;

            if (Beatmap.Value?.OnlineBeatmapID.HasValue != true)
                return;

            var status = Beatmap.Value?.Status;

            bool hasNoLeaderboard = status == BeatmapSetOnlineStatus.Graveyard
                || status == BeatmapSetOnlineStatus.None
                || status == BeatmapSetOnlineStatus.Pending
                || status == BeatmapSetOnlineStatus.WIP;

            if (hasNoLeaderboard)
                return;

            loadingAnimation.Show();
            getScoresRequest = new GetScoresRequest(Beatmap.Value, Beatmap.Value.Ruleset, scope, mods);
            getScoresRequest.Success += scores =>
            {
                loadingAnimation.Hide();
                Scores = scores;
            };
            api.Queue(getScoresRequest);
        }
    }
}
