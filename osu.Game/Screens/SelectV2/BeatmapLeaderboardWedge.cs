// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.PolygonExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Threading;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
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
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapLeaderboardWedge : VisibilityContainer
    {
        public const float SPACING_BETWEEN_SCORES = 4;

        public IBindable<BeatmapLeaderboardScope> Scope { get; } = new Bindable<BeatmapLeaderboardScope>();

        public IBindable<LeaderboardSortMode> Sorting { get; } = new Bindable<LeaderboardSortMode>();

        public IBindable<bool> FilterBySelectedMods { get; } = new BindableBool();

        [Resolved]
        private LeaderboardManager leaderboardManager { get; set; } = null!;

        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; } = null!;

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

        [Resolved]
        private IBindable<IReadOnlyList<Mod>> mods { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [Resolved]
        private ISongSelect? songSelect { get; set; }

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        private Container<Placeholder> placeholderContainer = null!;
        private Placeholder? placeholder;

        private Container scoresContainer = null!;

        private OsuScrollContainer scoresScroll = null!;
        private Container personalBestDisplay = null!;

        private Container<BeatmapLeaderboardScore> personalBestScoreContainer = null!;
        private OsuSpriteText personalBestText = null!;
        private LoadingLayer loading = null!;

        private CancellationTokenSource? cancellationTokenSource;

        private readonly IBindable<LeaderboardScores?> fetchedScores = new Bindable<LeaderboardScores?>();

        private const float personal_best_height = 112;

        // Blocking mouse down is required to avoid song select's background reveal logic happening while hovering scores.
        // Our horizontal alignment doesn't really align with the rest of the sheared components (protrudes a touch to the right) which makes
        // it complicated to handle this at a higher level.
        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => scoresScroll.ReceivePositionalInputAt(screenSpacePos);

        protected override bool OnMouseDown(MouseDownEvent e) => true;

        private Sample? swishSample;

        private readonly List<ScheduledDelegate> scoreSfxDelegates = new List<ScheduledDelegate>();

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
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
                            Padding = new MarginPadding
                            {
                                Top = 5,
                                // Left padding offsets the shear to create a visually appealing list display.
                                Left = 80f,
                                // Bottom padding ensures the last entry's full width is displayed
                                // (ie it is fully on screen after shear is considered).
                                Bottom = BeatmapLeaderboardScore.HEIGHT * 3
                            },
                        },
                    },
                    personalBestDisplay = new Container
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        RelativeSizeAxes = Axes.X,
                        Height = personal_best_height,
                        Shear = OsuGame.SHEAR,
                        Margin = new MarginPadding
                        {
                            Left = -40f,
                        },
                        CornerRadius = 10f,
                        Masking = true,
                        // push the personal best 1px down to hide masking issues
                        Y = 1f,
                        X = -100f,
                        Alpha = 0f,
                        Children = new Drawable[]
                        {
                            new WedgeBackground(),
                            // Required because wedge background blocks input from passing through
                            // to the main context menu container above.
                            new OsuContextMenuContainer
                            {
                                Shear = -OsuGame.SHEAR,
                                RelativeSizeAxes = Axes.Both,
                                Child = new Container
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Padding = new MarginPadding { Top = 5f, Bottom = 5f, Left = 70f, Right = 10f },
                                    Children = new Drawable[]
                                    {
                                        personalBestText = new OsuSpriteText
                                        {
                                            Colour = colourProvider.Content2,
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
                            }
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

            swishSample = audio.Samples.Get(@"SongSelect/leaderboard-score");
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Scope.BindValueChanged(_ => RefetchScores());
            Sorting.BindValueChanged(_ => RefetchScores());
            FilterBySelectedMods.BindValueChanged(_ => RefetchScores());
            beatmap.BindValueChanged(_ => RefetchScores());
            ruleset.BindValueChanged(_ => RefetchScores());
            mods.BindValueChanged(_ => refetchScoresFromMods());

            RefetchScores();
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
                RefetchScores();
        }

        private bool initialFetchComplete;

        private ScheduledDelegate? refetchOperation;

        public void RefetchScores()
        {
            SetScores(Array.Empty<ScoreInfo>());

            if (beatmap.IsDefault)
            {
                SetState(LeaderboardState.NoneSelected);
                return;
            }

            SetState(LeaderboardState.Retrieving);

            refetchOperation?.Cancel();
            refetchOperation = Scheduler.AddDelayed(() =>
            {
                var fetchBeatmapInfo = beatmap.Value.BeatmapInfo;
                var fetchRuleset = ruleset.Value ?? fetchBeatmapInfo.Ruleset;
                var fetchSorting = Scope.Value == BeatmapLeaderboardScope.Local ? Sorting.Value : LeaderboardSortMode.Score;

                // For now, we forcefully refresh to keep things simple.
                // In the future, removing this requirement may be deemed useful, but will need ample testing of edge case scenarios
                // (like returning from gameplay after setting a new score, returning to song select after main menu).
                leaderboardManager.FetchWithCriteria(new LeaderboardCriteria(fetchBeatmapInfo, fetchRuleset, Scope.Value, FilterBySelectedMods.Value ? mods.Value.ToArray() : null, fetchSorting),
                    forceRefresh: true);

                if (!initialFetchComplete)
                {
                    // only bind this after the first fetch to avoid reading stale scores.
                    fetchedScores.BindTo(leaderboardManager.Scores);
                    fetchedScores.BindValueChanged(_ => updateScores(), true);
                    initialFetchComplete = true;
                }
            }, initialFetchComplete ? 300 : 0);
        }

        private void updateScores()
        {
            var scores = fetchedScores.Value;

            if (scores == null) return;

            // because leaderboard refetches are debounced, it is technically possible for the global leaderboard manager
            // to contain scores for a different beatmap than the ones the wedge is currently on.
            // in this case, ignore the incoming scores to avoid briefly flashing the wrong leaderboard.
            if (leaderboardManager.CurrentCriteria?.Beatmap?.Equals(beatmap.Value.BeatmapInfo) != true)
                return;

            if (scores.FailState != null)
                SetState((LeaderboardState)scores.FailState);
            else
                SetScores(scores.TopScores, scores.UserScore, scores.TotalScores);
        }

        protected void SetScores(IEnumerable<ScoreInfo> scores, ScoreInfo? userScore = null, int? totalCount = null)
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

            LoadComponentsAsync(scores.Select((s, i) =>
            {
                BeatmapLeaderboardScore.HighlightType? highlightType = null;

                if (s.OnlineID == userScore?.OnlineID)
                    highlightType = BeatmapLeaderboardScore.HighlightType.Own;
                else if (api.LocalUserState.Friends.Any(r => r.TargetID == s.UserID) && Scope.Value != BeatmapLeaderboardScope.Friend)
                    highlightType = BeatmapLeaderboardScore.HighlightType.Friend;

                return new BeatmapLeaderboardScore(s)
                {
                    Rank = i + 1,
                    Highlight = highlightType,
                    SelectedMods = { BindTarget = mods },
                    Action = () => onLeaderboardScoreClicked(s),
                };
            }), loadedScores =>
            {
                int delay = 200;
                int i = 0;

                foreach (var d in loadedScores)
                {
                    d.Y = (BeatmapLeaderboardScore.HEIGHT + SPACING_BETWEEN_SCORES) * i;

                    // This is a bit of a weird one. We're already in a sheared state and don't want top-level
                    // shear applied, but still need the `BeatmapLeaderboardScore` to be in "sheared" mode (see ctor).
                    d.Shear = Vector2.Zero;

                    scoresContainer.Add(d);

                    d.FadeOut()
                     .MoveToX(-20f)
                     .Delay(delay)
                     .FadeIn(300, Easing.OutQuint)
                     .MoveToX(0f, 300, Easing.OutQuint);

                    bool visible = d.ScreenSpaceDrawQuad.TopLeft.Y < d.Parent!.ChildMaskingBounds.BottomLeft.Y;

                    if (visible)
                    {
                        var del = Scheduler.AddDelayed(() =>
                        {
                            var chan = swishSample?.GetChannel();
                            if (chan == null) return;

                            chan.Balance.Value = -OsuGameBase.SFX_STEREO_STRENGTH / 2;
                            chan.Frequency.Value = 0.98f + RNG.NextDouble(0.04f);
                            chan.Play();
                        }, delay);

                        scoreSfxDelegates.Add(del);
                    }

                    delay += 30;
                    i++;
                }
            }, cancellation: cancellationTokenSource.Token);

            if (userScore != null)
            {
                personalBestDisplay.MoveToX(0, 600, Easing.OutQuint);
                personalBestDisplay.FadeIn(600, Easing.OutQuint);
                personalBestScoreContainer.Child = new BeatmapLeaderboardScore(userScore)
                {
                    Highlight = BeatmapLeaderboardScore.HighlightType.Own,
                    Rank = userScore.Position,
                    SelectedMods = { BindTarget = mods },
                    Action = () => onLeaderboardScoreClicked(userScore),
                };

                scoresScroll.TransformTo(nameof(scoresScroll.Padding), new MarginPadding { Bottom = personal_best_height }, 300, Easing.OutQuint);

                if (totalCount != null && userScore.Position != null)
                    personalBestText.Text = $"Personal Best (#{userScore.Position:N0} of {totalCount.Value:N0})";
                else
                    personalBestText.Text = "Personal Best";
            }
        }

        private void clearScores()
        {
            float delay = 0;

            foreach (var d in scoresContainer)
            {
                // Avoid applying animations a second time to drawables which are already fading out.
                if (d.LifetimeEnd != double.MaxValue)
                    continue;

                d.Delay(delay)
                 .MoveToX(-10f, 120, Easing.Out)
                 .FadeOut(120, Easing.Out)
                 .Expire();

                // If the user is scrolled down in the list, start delaying only from the current visible range to
                // avoid the perceived transition from taking longer than expected.
                if (d.ScreenSpaceDrawQuad.Intersects(scoresScroll.ScreenSpaceDrawQuad))
                    delay += 20;
            }

            personalBestDisplay.MoveToX(-100, 300, Easing.OutQuint);
            personalBestDisplay.FadeOut(300, Easing.OutQuint);
            scoresScroll.TransformTo(nameof(scoresScroll.Padding), new MarginPadding(), 300, Easing.OutQuint);

            scoreSfxDelegates.ForEach(d => d.Cancel());
            scoreSfxDelegates.Clear();
        }

        private void onLeaderboardScoreClicked(ScoreInfo score) => songSelect?.PresentScore(score);

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

            clearScores();

            placeholderContainer.Child = placeholder;

            placeholder.ScaleTo(0.8f).Then().ScaleTo(1, 900, Easing.OutQuint);
            placeholder.FadeInFromZero(300, Easing.OutQuint);
        }

        #region Fade handling

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            const int height = BeatmapLeaderboardScore.HEIGHT;

            float fadeBottom = (float)(scoresScroll.Current + scoresScroll.DrawHeight);
            float fadeTop = (float)(scoresScroll.Current);

            fadeTop += (float)Math.Min(height, Math.Log10(Math.Max(fadeTop, 0) + 1) * height);

            foreach (var c in scoresContainer)
            {
                float topY = c.ToSpaceOfOtherDrawable(Vector2.Zero, scoresContainer).Y;
                float bottomY = topY + height;

                bool requireBottomFade = bottomY >= fadeBottom;
                bool requireTopFade = topY < fadeTop;

                if (!requireBottomFade && !requireTopFade)
                {
                    c.Colour = Color4.White;
                    continue;
                }

                if (topY > fadeBottom + height || bottomY < fadeTop - height)
                {
                    c.Colour = Color4.Transparent;
                    continue;
                }

                if (requireBottomFade)
                {
                    c.Colour = ColourInfo.GradientVertical(
                        Color4.White.Opacity(Math.Min(1 - (topY - fadeBottom) / height, 1)),
                        Color4.White.Opacity(Math.Min(1 - (bottomY - fadeBottom) / height, 1)));
                }
                else
                {
                    Debug.Assert(requireTopFade);

                    c.Colour = ColourInfo.GradientVertical(
                        Color4.White.Opacity(Math.Min(1 - (fadeTop - topY) / height, 1)),
                        Color4.White.Opacity(Math.Min(1 - (fadeTop - bottomY) / height, 1)));
                }
            }
        }

        #endregion

        private Placeholder? getPlaceholderFor(LeaderboardState state)
        {
            switch (state)
            {
                case LeaderboardState.NetworkFailure:
                    return new ClickablePlaceholder(LeaderboardStrings.CouldntFetchScores, FontAwesome.Solid.Sync)
                    {
                        Action = RefetchScores
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
