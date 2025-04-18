// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Cursor;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Online.API;
using osu.Game.Online.Leaderboards;
using osu.Game.Online.Placeholders;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
using osu.Game.Screens.Select.Leaderboards;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapLeaderboardWedge : VisibilityContainer
    {
        private Container scoresContainer = null!;

        private OsuScrollContainer scoresScroll = null!;
        private Container personalBestDisplay = null!;
        private Container<BeatmapLeaderboardScore> personalBestScoreContainer = null!;
        private LoadingLayer loading = null!;

        private Container<Placeholder> placeholderContainer = null!;
        private Placeholder? placeholder;

        [Resolved]
        private LeaderboardManager leaderboardManager { get; set; } = null!;

        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; } = null!;

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

        [Resolved]
        private IBindable<IReadOnlyList<Mod>> mods { get; set; } = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        public IBindable<BeatmapLeaderboardScope> Scope { get; } = new Bindable<BeatmapLeaderboardScope>();

        private bool isOnlineScope => Scope.Value != BeatmapLeaderboardScope.Local;

        public IBindable<bool> FilterBySelectedMods { get; } = new BindableBool();

        private CancellationTokenSource? cancellationTokenSource;

        private readonly Bindable<LeaderboardScores?> fetchedScores = new Bindable<LeaderboardScores?>();

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Both;

            Child = new OsuContextMenuContainer
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    scoresScroll = new OsuScrollContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        ScrollbarVisible = false,
                        Shear = OsuGame.SHEAR,
                        Child = scoresContainer = new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Margin = new MarginPadding { Top = 4f, Bottom = 180f },
                        },
                    },
                    personalBestDisplay = new Container
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Shear = OsuGame.SHEAR,
                        Margin = new MarginPadding { Left = -60f },
                        CornerRadius = 10f,
                        Masking = true,
                        // push the personal best 1px down to hide masking issues
                        Y = 1f,
                        X = -100f,
                        Alpha = 0f,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = colourProvider.Background4,
                            },
                            new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Shear = -OsuGame.SHEAR,
                                Padding = new MarginPadding { Top = 5f, Bottom = 30f, Left = 100f, Right = 30f },
                                Children = new Drawable[]
                                {
                                    new OsuSpriteText
                                    {
                                        Colour = colourProvider.Content2,
                                        Text = "Personal Best",
                                        Font = OsuFont.Style.Caption1.With(weight: FontWeight.SemiBold),
                                    },
                                    personalBestScoreContainer = new Container<BeatmapLeaderboardScore>
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Margin = new MarginPadding { Top = 20f },
                                    },
                                }
                            },
                        },
                    },
                    placeholderContainer = new Container<Placeholder>
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                    },
                    loading = new LoadingLayer(),
                }
            };

            ((IBindable<LeaderboardScores?>)fetchedScores).BindTo(leaderboardManager.Scores);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Scope.BindValueChanged(_ => refetchScores());
            FilterBySelectedMods.BindValueChanged(_ => refetchScores());
            beatmap.BindValueChanged(_ => refetchScores());
            ruleset.BindValueChanged(_ => refetchScores());
            mods.BindValueChanged(_ => refetchScoresFromMods());

            refetchScores();
        }

        protected override void PopIn()
        {
            this.FadeIn(300, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            this.FadeOut(300, Easing.OutQuint);
        }

        private void refetchScoresFromMods()
        {
            if (FilterBySelectedMods.Value)
                refetchScores();
        }

        private void refetchScores()
        {
            SetScores(Array.Empty<ScoreInfo>(), null);
            SetState(LeaderboardState.Retrieving);

            if (beatmap.IsDefault)
            {
                SetState(LeaderboardState.NoneSelected);
                return;
            }

            var fetchBeatmapInfo = beatmap.Value.BeatmapInfo;
            var fetchRuleset = ruleset.Value ?? fetchBeatmapInfo.Ruleset;

            if (!api.IsLoggedIn)
            {
                SetState(LeaderboardState.NotLoggedIn);
                return;
            }

            if (!fetchRuleset.IsLegacyRuleset())
            {
                SetState(LeaderboardState.RulesetUnavailable);
                return;
            }

            if ((fetchBeatmapInfo.OnlineID <= 0 || fetchBeatmapInfo.Status <= BeatmapOnlineStatus.Pending) && isOnlineScope)
            {
                SetState(LeaderboardState.BeatmapUnavailable);
                return;
            }

            if (Scope.Value.RequiresSupporter(FilterBySelectedMods.Value) && !api.LocalUser.Value.IsSupporter)
            {
                SetState(LeaderboardState.NotSupporter);
                return;
            }

            if (Scope.Value == BeatmapLeaderboardScope.Team && api.LocalUser.Value.Team == null)
            {
                SetState(LeaderboardState.NoTeam);
                return;
            }

            leaderboardManager.FetchWithCriteriaAsync(new LeaderboardCriteria(fetchBeatmapInfo, fetchRuleset, Scope.Value, FilterBySelectedMods.Value ? mods.Value.ToArray() : null))
                              .ContinueWith(t =>
                              {
                                  if (t.Exception != null && !t.IsCanceled)
                                  {
                                      Schedule(() => SetState(LeaderboardState.NetworkFailure));
                                      return;
                                  }

                                  fetchedScores.UnbindEvents();
                                  fetchedScores.BindValueChanged(scores =>
                                  {
                                      if (scores.NewValue != null)
                                          Schedule(() => SetScores(scores.NewValue.TopScores, scores.NewValue.UserScore));
                                  }, true);
                              });
        }

        protected void SetScores(IEnumerable<ScoreInfo> scores, ScoreInfo? userScore)
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource = new CancellationTokenSource();

            clearScores();
            SetState(LeaderboardState.Success);

            if (!scores.Any())
            {
                SetState(LeaderboardState.NoScores);
                return;
            }

            LoadComponentsAsync(scores.Select((s, i) => new BeatmapLeaderboardScore(s)
            {
                Rank = i + 1,
                IsPersonalBest = s.OnlineID == userScore?.OnlineID,
                SelectedMods = { BindTarget = mods },
            }), loadedScores =>
            {
                int delay = 100;
                int accumulation = 1;
                int i = 0;

                foreach (var scoreDrawable in loadedScores)
                {
                    Container scoreDrawableContainer;

                    scoresContainer.Add(scoreDrawableContainer = new Container
                    {
                        Shear = -OsuGame.SHEAR,
                        Y = (BeatmapLeaderboardScore.HEIGHT + 4f) * i,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Alpha = 0f,
                        Padding = new MarginPadding { Left = 80f },
                        Child = scoreDrawable,
                    });

                    scoreDrawableContainer.Delay(delay).FadeIn(300, Easing.OutQuint);
                    scoreDrawableContainer.MoveToX(-100f).Delay(delay).MoveToX(0f, 300, Easing.OutQuint);

                    delay += Math.Max(0, 50 - accumulation);
                    accumulation *= 2;
                    i++;
                }
            }, cancellation: cancellationTokenSource.Token);

            if (userScore != null)
            {
                personalBestDisplay.MoveToX(0, 600, Easing.OutQuint);
                personalBestDisplay.FadeIn(600, Easing.OutQuint);
                personalBestScoreContainer.Child = new BeatmapLeaderboardScore(userScore)
                {
                    IsPersonalBest = true,
                    Rank = userScore.Position,
                    SelectedMods = { BindTarget = mods },
                };

                scoresScroll.TransformTo(nameof(scoresScroll.Padding), new MarginPadding { Bottom = 100 }, 300, Easing.OutQuint);
            }
        }

        private void clearScores()
        {
            foreach (var scoreDrawable in scoresContainer)
            {
                scoreDrawable.MoveToX(-50f, 200, Easing.OutQuint);
                scoreDrawable.FadeOut(200, Easing.OutQuint);
                scoreDrawable.Expire();
            }

            personalBestDisplay.MoveToX(-100, 300, Easing.OutQuint);
            personalBestDisplay.FadeOut(300, Easing.OutQuint);
            scoresScroll.TransformTo(nameof(scoresScroll.Padding), new MarginPadding(), 300, Easing.OutQuint);
        }

        private LeaderboardState displayedState;

        protected void SetState(LeaderboardState state)
        {
            if (state == displayedState)
                return;

            if (state == LeaderboardState.Retrieving)
                loading.Show();
            else
                loading.Hide();

            displayedState = state;

            placeholder?.FadeOut(150, Easing.OutQuint).Expire();
            placeholder = getPlaceholderFor(state);

            if (placeholder == null)
                return;

            placeholderContainer.Child = placeholder;

            placeholder.ScaleTo(0.8f).Then().ScaleTo(1, 900, Easing.OutQuint);
            placeholder.FadeInFromZero(300, Easing.OutQuint);
        }

        private Placeholder? getPlaceholderFor(LeaderboardState state)
        {
            switch (state)
            {
                case LeaderboardState.NetworkFailure:
                    return new ClickablePlaceholder(LeaderboardStrings.CouldntFetchScores, FontAwesome.Solid.Sync)
                    {
                        Action = refetchScores
                    };

                case LeaderboardState.NoneSelected:
                    return new MessagePlaceholder(LeaderboardStrings.PleaseSelectABeatmap);

                case LeaderboardState.RulesetUnavailable:
                    return new MessagePlaceholder(LeaderboardStrings.LeaderboardsAreNotAvailableForThisRuleset);

                case LeaderboardState.BeatmapUnavailable:
                    return new MessagePlaceholder(LeaderboardStrings.LeaderboardsAreNotAvailableForThisBeatmap);

                case LeaderboardState.NoScores:
                    return new MessagePlaceholder(LeaderboardStrings.NoRecordsYet);

                case LeaderboardState.NotLoggedIn:
                    return new LoginPlaceholder(LeaderboardStrings.PleaseSignInToViewOnlineLeaderboards);

                case LeaderboardState.NotSupporter:
                    return new MessagePlaceholder(LeaderboardStrings.PleaseInvestInAnOsuSupporterTagToViewThisLeaderboard);

                case LeaderboardState.NoTeam:
                    return new MessagePlaceholder(LeaderboardStrings.NoTeam);

                case LeaderboardState.Retrieving:
                    return null;

                case LeaderboardState.Success:
                    return null;

                default:
                    throw new ArgumentOutOfRangeException(nameof(state));
            }
        }
    }
}
