// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Online.Spectator;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.HUD;
using osu.Game.Screens.Spectate;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Spectate
{
    /// <summary>
    /// A <see cref="SpectatorScreen"/> that spectates multiple users in a match.
    /// </summary>
    public class MultiSpectatorScreen : SpectatorScreen
    {
        // Isolates beatmap/ruleset to this screen.
        public override bool DisallowExternalBeatmapRulesetChanges => true;

        // We are managing our own adjustments. For now, this happens inside the Player instances themselves.
        public override bool? AllowTrackAdjustments => false;

        /// <summary>
        /// Whether all spectating players have finished loading.
        /// </summary>
        public bool AllPlayersLoaded => instances.All(p => p?.PlayerLoaded == true);

        protected override UserActivity InitialActivity => new UserActivity.SpectatingMultiplayerGame(Beatmap.Value.BeatmapInfo, Ruleset.Value);

        [Resolved]
        private OsuColour colours { get; set; }

        [Resolved]
        private MultiplayerClient multiplayerClient { get; set; }

        private readonly PlayerArea[] instances;
        private MasterGameplayClockContainer masterClockContainer;
        private ISyncManager syncManager;
        private PlayerGrid grid;
        private MultiSpectatorLeaderboard leaderboard;
        private PlayerArea currentAudioSource;
        private bool canStartMasterClock;

        private readonly Room room;
        private readonly MultiplayerRoomUser[] users;

        /// <summary>
        /// Creates a new <see cref="MultiSpectatorScreen"/>.
        /// </summary>
        /// <param name="room">The room.</param>
        /// <param name="users">The players to spectate.</param>
        public MultiSpectatorScreen(Room room, MultiplayerRoomUser[] users)
            : base(users.Select(u => u.UserID).ToArray())
        {
            this.room = room;
            this.users = users;

            instances = new PlayerArea[Users.Count];
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            FillFlowContainer leaderboardFlow;
            Container scoreDisplayContainer;

            masterClockContainer = CreateMasterGameplayClockContainer(Beatmap.Value);

            InternalChildren = new[]
            {
                (Drawable)(syncManager = new CatchUpSyncManager(masterClockContainer)),
                masterClockContainer.WithChild(new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            scoreDisplayContainer = new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y
                            },
                        },
                        new Drawable[]
                        {
                            new GridContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                ColumnDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
                                Content = new[]
                                {
                                    new Drawable[]
                                    {
                                        leaderboardFlow = new FillFlowContainer
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            AutoSizeAxes = Axes.Both,
                                            Direction = FillDirection.Vertical,
                                            Spacing = new Vector2(5)
                                        },
                                        grid = new PlayerGrid { RelativeSizeAxes = Axes.Both }
                                    }
                                }
                            }
                        }
                    }
                })
            };

            for (int i = 0; i < Users.Count; i++)
            {
                grid.Add(instances[i] = new PlayerArea(Users[i], masterClockContainer.GameplayClock));
                syncManager.AddPlayerClock(instances[i].GameplayClock);
            }

            LoadComponentAsync(leaderboard = new MultiSpectatorLeaderboard(users)
            {
                Expanded = { Value = true },
            }, l =>
            {
                foreach (var instance in instances)
                    leaderboard.AddClock(instance.UserId, instance.GameplayClock);

                leaderboardFlow.Insert(0, leaderboard);

                if (leaderboard.TeamScores.Count == 2)
                {
                    LoadComponentAsync(new MatchScoreDisplay
                    {
                        Team1Score = { BindTarget = leaderboard.TeamScores.First().Value },
                        Team2Score = { BindTarget = leaderboard.TeamScores.Last().Value },
                    }, scoreDisplayContainer.Add);
                }
            });

            LoadComponentAsync(new GameplayChatDisplay(room)
            {
                Expanded = { Value = true },
            }, chat => leaderboardFlow.Insert(1, chat));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            masterClockContainer.Reset();

            syncManager.ReadyToStart += onReadyToStart;
            syncManager.MasterState.BindValueChanged(onMasterStateChanged, true);
        }

        protected override void Update()
        {
            base.Update();

            if (!isCandidateAudioSource(currentAudioSource?.GameplayClock))
            {
                currentAudioSource = instances.Where(i => isCandidateAudioSource(i.GameplayClock))
                                              .OrderBy(i => Math.Abs(i.GameplayClock.CurrentTime - syncManager.MasterClock.CurrentTime))
                                              .FirstOrDefault();

                foreach (var instance in instances)
                    instance.Mute = instance != currentAudioSource;
            }
        }

        private bool isCandidateAudioSource([CanBeNull] ISpectatorPlayerClock clock)
            => clock?.IsRunning == true && !clock.IsCatchingUp && !clock.WaitingOnFrames.Value;

        private void onReadyToStart()
        {
            // Seek the master clock to the gameplay time.
            // This is chosen as the first available frame in the players' replays, which matches the seek by each individual SpectatorPlayer.
            double startTime = instances.Where(i => i.Score != null)
                                        .SelectMany(i => i.Score.Replay.Frames)
                                        .Select(f => f.Time)
                                        .DefaultIfEmpty(0)
                                        .Min();

            masterClockContainer.StartTime = startTime;
            masterClockContainer.Reset(true);

            // Although the clock has been started, this flag is set to allow for later synchronisation state changes to also be able to start it.
            canStartMasterClock = true;
        }

        private void onMasterStateChanged(ValueChangedEvent<MasterClockState> state)
        {
            switch (state.NewValue)
            {
                case MasterClockState.Synchronised:
                    if (canStartMasterClock)
                        masterClockContainer.Start();

                    break;

                case MasterClockState.TooFarAhead:
                    masterClockContainer.Stop();
                    break;
            }
        }

        protected override void OnNewPlayingUserState(int userId, SpectatorState spectatorState)
        {
        }

        protected override void StartGameplay(int userId, SpectatorGameplayState spectatorGameplayState)
            => instances.Single(i => i.UserId == userId).LoadScore(spectatorGameplayState.Score);

        protected override void EndGameplay(int userId, SpectatorState state)
        {
            // Allowed passed/failed users to complete their remaining replay frames.
            // The failed state isn't really possible in multiplayer (yet?) but is added here just for safety in case it starts being used.
            if (state.State == SpectatedUserState.Passed || state.State == SpectatedUserState.Failed)
                return;

            RemoveUser(userId);

            var instance = instances.Single(i => i.UserId == userId);

            instance.FadeColour(colours.Gray4, 400, Easing.OutQuint);
            syncManager.RemovePlayerClock(instance.GameplayClock);
        }

        public override bool OnBackButton()
        {
            if (multiplayerClient.Room == null)
                return base.OnBackButton();

            // On a manual exit, set the player back to idle unless gameplay has finished.
            if (multiplayerClient.Room.State != MultiplayerRoomState.Open)
                multiplayerClient.ChangeState(MultiplayerUserState.Idle);

            return base.OnBackButton();
        }

        protected virtual MasterGameplayClockContainer CreateMasterGameplayClockContainer(WorkingBeatmap beatmap) => new MasterGameplayClockContainer(beatmap, 0);
    }
}
