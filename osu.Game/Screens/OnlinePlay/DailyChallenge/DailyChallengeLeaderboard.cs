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
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
using osu.Game.Screens.SelectV2;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.DailyChallenge
{
    public partial class DailyChallengeLeaderboard : CompositeDrawable
    {
        public IBindable<MultiplayerScore?> UserBestScore => userBestScore;
        private readonly Bindable<MultiplayerScore?> userBestScore = new Bindable<MultiplayerScore?>();
        public Bindable<IReadOnlyList<Mod>> SelectedMods = new Bindable<IReadOnlyList<Mod>>();

        /// <summary>
        /// A function determining whether each mod in the score can be selected.
        /// A return value of <see langword="true"/> means that the mod can be selected in the current context.
        /// A return value of <see langword="false"/> means that the mod cannot be selected in the current context.
        /// </summary>
        public Func<Mod, bool> IsValidMod { get; set; } = _ => true;

        public Action<long>? PresentScore { get; init; }

        private readonly Room room;
        private readonly PlaylistItem playlistItem;

        private FillFlowContainer<BeatmapLeaderboardScore> scoreFlow = null!;
        private Container userBestContainer = null!;
        private SectionHeader userBestHeader = null!;
        private LoadingLayer loadingLayer = null!;

        private CancellationTokenSource? cancellationTokenSource;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private ScoreManager scoreManager { get; set; } = null!;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; } = null!;

        public DailyChallengeLeaderboard(Room room, PlaylistItem playlistItem)
        {
            this.room = room;
            this.playlistItem = playlistItem;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                RowDimensions =
                [
                    new Dimension(GridSizeMode.AutoSize),
                    new Dimension(),
                    new Dimension(GridSizeMode.AutoSize),
                    new Dimension(GridSizeMode.AutoSize)
                ],
                Content = new[]
                {
                    new Drawable[] { new SectionHeader("Leaderboard") },
                    new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                new OsuScrollContainer
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Child = scoreFlow = new FillFlowContainer<BeatmapLeaderboardScore>
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Padding = new MarginPadding { Right = 20, },
                                        Direction = FillDirection.Vertical,
                                        Spacing = new Vector2(5),
                                        Scale = new Vector2(0.8f),
                                        Width = 1 / 0.8f,
                                    }
                                },
                                loadingLayer = new LoadingLayer
                                {
                                    RelativeSizeAxes = Axes.Both,
                                },
                            }
                        }
                    },
                    new Drawable[] { userBestHeader = new SectionHeader("Personal best") { Alpha = 0, } },
                    new Drawable[]
                    {
                        userBestContainer = new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Padding = new MarginPadding { Right = 20, },
                            Scale = new Vector2(0.8f),
                            Width = 1 / 0.8f,
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            RefetchScores();
        }

        private IndexPlaylistScoresRequest? request;

        public void RefetchScores()
        {
            if (request?.CompletionState == APIRequestCompletionState.Waiting)
                return;

            request = new IndexPlaylistScoresRequest(room.RoomID!.Value, playlistItem.ID);

            request.Success += req => Schedule(() =>
            {
                var best = req.Scores.Select(s => s.CreateScoreInfo(scoreManager, rulesets, beatmap.Value.BeatmapInfo)).ToArray();

                userBestScore.Value = req.UserScore;
                var userBest = userBestScore.Value?.CreateScoreInfo(scoreManager, rulesets, beatmap.Value.BeatmapInfo);

                cancellationTokenSource?.Cancel();
                cancellationTokenSource = null;
                cancellationTokenSource ??= new CancellationTokenSource();

                if (best.Length == 0)
                {
                    scoreFlow.Clear();
                    loadingLayer.Hide();
                }
                else
                {
                    LoadComponentsAsync(best.Select((s, index) =>
                    {
                        BeatmapLeaderboardScore.HighlightType? highlightType = null;

                        if (s.UserID == api.LocalUser.Value.Id)
                            highlightType = BeatmapLeaderboardScore.HighlightType.Own;
                        else if (api.LocalUserState.Friends.Any(r => r.TargetID == s.UserID))
                            highlightType = BeatmapLeaderboardScore.HighlightType.Friend;

                        return new BeatmapLeaderboardScore(s, sheared: false)
                        {
                            Rank = index + 1,
                            Highlight = highlightType,
                            Action = () => PresentScore?.Invoke(s.OnlineID),
                            SelectedMods = { BindTarget = SelectedMods },
                            IsValidMod = IsValidMod,
                        };
                    }), loaded =>
                    {
                        scoreFlow.Clear();
                        scoreFlow.AddRange(loaded);
                        scoreFlow.FadeTo(1, 400, Easing.OutQuint);
                        loadingLayer.Hide();
                    }, cancellationTokenSource.Token);
                }

                userBestContainer.Clear();

                if (userBest != null)
                {
                    userBestContainer.Add(new BeatmapLeaderboardScore(userBest, sheared: false)
                    {
                        Rank = userBest.Position,
                        Highlight = BeatmapLeaderboardScore.HighlightType.Own,
                        Action = () => PresentScore?.Invoke(userBest.OnlineID),
                        SelectedMods = { BindTarget = SelectedMods },
                        IsValidMod = IsValidMod,
                    });
                }

                userBestHeader.FadeTo(userBest == null ? 0 : 1);
            });

            loadingLayer.Show();
            scoreFlow.FadeTo(0.5f, 400, Easing.OutQuint);
            api.Queue(request);
        }
    }
}
