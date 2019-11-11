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
using osu.Game.Rulesets;

namespace osu.Game.Overlays.BeatmapSet.Scores
{
    public class ScoresContainer : CompositeDrawable
    {
        private const int spacing = 15;
        private const int duration = 200;

        public readonly Bindable<BeatmapInfo> Beatmap = new Bindable<BeatmapInfo>();
        private readonly Bindable<RulesetInfo> ruleset = new Bindable<RulesetInfo>();

        private readonly Box background;
        private readonly ScoreTable scoreTable;
        private readonly FillFlowContainer topScoresContainer;
        private readonly LoadingAnimation loadingAnimation;
        private readonly LeaderboardScopeSelector scopeSelector;
        private readonly LeaderboardModSelector modSelector;
        private readonly Container modFilter;
        private readonly FillFlowContainer modFilterContent;

        private GetScoresRequest getScoresRequest;

        [Resolved]
        private IAPIProvider api { get; set; }

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
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, spacing),
                    Margin = new MarginPadding { Vertical = spacing },
                    Children = new Drawable[]
                    {
                        modFilter = new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeEasing = Easing.OutQuint,
                            AutoSizeDuration = duration,
                            Child = modFilterContent = new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical,
                                Spacing = new Vector2(0, spacing),
                                Alpha = 0,
                                AlwaysPresent = true,
                                Children = new Drawable[]
                                {
                                    scopeSelector = new LeaderboardScopeSelector
                                    {
                                        Anchor = Anchor.TopCentre,
                                        Origin = Anchor.TopCentre,
                                    },
                                    modSelector = new LeaderboardModSelector
                                    {
                                        Anchor = Anchor.TopCentre,
                                        Origin = Anchor.TopCentre,
                                        Ruleset = { BindTarget = ruleset }
                                    },
                                }
                            },
                        },
                        new Container
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
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
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            background.Colour = colours.Gray2;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            api.LocalUser.BindValueChanged(_ => updateModFilterVisibility());

            modSelector.SelectedMods.ItemsAdded += modsChanged;
            modSelector.SelectedMods.ItemsRemoved += modsChanged;

            scopeSelector.Current.BindValueChanged(scope => getScores(scope.NewValue, modSelector.SelectedMods));

            Beatmap.BindValueChanged(beatmap =>
            {
                scopeSelector.Current.Value = BeatmapLeaderboardScope.Global;

                var beatmapRuleset = beatmap.NewValue?.Ruleset;

                if (modSelector.Ruleset.Value?.Equals(beatmapRuleset) ?? false)
                    modSelector.DeselectAll();
                else
                    ruleset.Value = beatmapRuleset;

                getScores();
            }, true);
        }

        private void modsChanged(IEnumerable<Mod> mods) => getScores(scopeSelector.Current.Value, modSelector.SelectedMods);

        private void getScores(BeatmapLeaderboardScope scope = BeatmapLeaderboardScope.Global, IEnumerable<Mod> mods = null)
        {
            getScoresRequest?.Cancel();
            getScoresRequest = null;

            Scores = null;

            updateModFilterVisibility();

            if (!hasLeaderboard)
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

        private void updateModFilterVisibility()
        {
            if (api.IsLoggedIn && api.LocalUser.Value.IsSupporter && hasLeaderboard)
            {
                modFilter.AutoSizeAxes = Axes.Y;
                modFilterContent.FadeIn(duration, Easing.OutQuint);
            }
            else
            {
                modFilter.AutoSizeAxes = Axes.None;
                modFilter.ResizeHeightTo(0, duration, Easing.OutQuint);
                modFilterContent.FadeOut(duration, Easing.OutQuint);
            }
        }

        private bool hasLeaderboard => Beatmap.Value?.Status > BeatmapSetOnlineStatus.Pending;
    }
}
