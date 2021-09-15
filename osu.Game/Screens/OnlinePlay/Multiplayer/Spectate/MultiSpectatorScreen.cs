// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Spectator;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.HUD;
using osu.Game.Screens.Spectate;

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
        public override bool AllowTrackAdjustments => false;

        /// <summary>
        /// Whether all spectating players have finished loading.
        /// </summary>
        public bool AllPlayersLoaded => instances.All(p => p?.PlayerLoaded == true);

        [Resolved]
        private OsuColour colours { get; set; }

        [Resolved]
        private SpectatorClient spectatorClient { get; set; }

        [Resolved]
        private MultiplayerClient multiplayerClient { get; set; }

        private readonly PlayerArea[] instances;
        private MasterGameplayClockContainer masterClockContainer;
        private ISyncManager syncManager;
        private PlayerGrid grid;
        private MultiSpectatorLeaderboard leaderboard;
        private PlayerArea currentAudioSource;
        private bool canStartMasterClock;

        private readonly MultiplayerRoomUser[] users;

        /// <summary>
        /// Creates a new <see cref="MultiSpectatorScreen"/>.
        /// </summary>
        /// <param name="users">The players to spectate.</param>
        public MultiSpectatorScreen(MultiplayerRoomUser[] users)
            : base(users.Select(u => u.UserID).ToArray())
        {
            this.users = users;

            instances = new PlayerArea[Users.Count];
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Container leaderboardContainer;
            Container scoreDisplayContainer;

            masterClockContainer = new MasterGameplayClockContainer(Beatmap.Value, 0);

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
                                        leaderboardContainer = new Container
                                        {
                                            RelativeSizeAxes = Axes.Y,
                                            AutoSizeAxes = Axes.X
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

            // Todo: This is not quite correct - it should be per-user to adjust for other mod combinations.
            var playableBeatmap = Beatmap.Value.GetPlayableBeatmap(Ruleset.Value);
            var scoreProcessor = Ruleset.Value.CreateInstance().CreateScoreProcessor();
            scoreProcessor.ApplyBeatmap(playableBeatmap);

            LoadComponentAsync(leaderboard = new MultiSpectatorLeaderboard(scoreProcessor, users)
            {
                Expanded = { Value = true },
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
            }, l =>
            {
                foreach (var instance in instances)
                    leaderboard.AddClock(instance.UserId, instance.GameplayClock);

                leaderboardContainer.Add(leaderboard);

                if (leaderboard.TeamScores.Count == 2)
                {
                    LoadComponentAsync(new MatchScoreDisplay
                    {
                        Team1Score = { BindTarget = leaderboard.TeamScores.First().Value },
                        Team2Score = { BindTarget = leaderboard.TeamScores.Last().Value },
                    }, scoreDisplayContainer.Add);
                }
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            masterClockContainer.Reset();
            masterClockContainer.Stop();

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
            var startTime = instances.Where(i => i.Score != null)
                                     .SelectMany(i => i.Score.Replay.Frames)
                                     .Select(f => f.Time)
                                     .DefaultIfEmpty(0)
                                     .Min();

            masterClockContainer.Seek(startTime);
            masterClockContainer.Start();

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

        protected override void OnUserStateChanged(int userId, SpectatorState spectatorState)
        {
        }

        protected override void StartGameplay(int userId, GameplayState gameplayState)
            => instances.Single(i => i.UserId == userId).LoadScore(gameplayState.Score);

        protected override void EndGameplay(int userId)
        {
            RemoveUser(userId);

            var instance = instances.Single(i => i.UserId == userId);

            instance.FadeColour(colours.Gray4, 400, Easing.OutQuint);
            syncManager.RemovePlayerClock(instance.GameplayClock);
            leaderboard.RemoveClock(userId);
        }

        public override bool OnBackButton()
        {
            // On a manual exit, set the player state back to idle.
            multiplayerClient.ChangeState(MultiplayerUserState.Idle);
            return base.OnBackButton();
        }
    }
}
