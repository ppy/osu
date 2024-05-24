// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
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
    public partial class MultiSpectatorScreen : SpectatorScreen
    {
        // Isolates beatmap/ruleset to this screen.
        public override bool DisallowExternalBeatmapRulesetChanges => true;

        // We are managing our own adjustments. For now, this happens inside the Player instances themselves.
        public override bool? ApplyModTrackAdjustments => false;

        public override bool HideOverlaysOnEnter => true;

        /// <summary>
        /// Whether all spectating players have finished loading.
        /// </summary>
        public bool AllPlayersLoaded => instances.All(p => p.PlayerLoaded);

        protected override UserActivity InitialActivity => new UserActivity.SpectatingMultiplayerGame(Beatmap.Value.BeatmapInfo, Ruleset.Value);

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved]
        private MultiplayerClient multiplayerClient { get; set; } = null!;

        private IAggregateAudioAdjustment? boundAdjustments;

        private readonly PlayerArea[] instances;
        private MasterGameplayClockContainer masterClockContainer = null!;
        private SpectatorSyncManager syncManager = null!;
        private PlayerGrid grid = null!;
        private MultiSpectatorLeaderboard leaderboard = null!;
        private PlayerArea? currentAudioSource;

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

            InternalChildren = new Drawable[]
            {
                masterClockContainer = new MasterGameplayClockContainer(Beatmap.Value, 0)
                {
                    Child = new GridContainer
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
                    }
                },
                syncManager = new SpectatorSyncManager(masterClockContainer)
                {
                    ReadyToStart = performInitialSeek,
                }
            };

            for (int i = 0; i < Users.Count; i++)
                grid.Add(instances[i] = new PlayerArea(Users[i], syncManager.CreateManagedClock()));

            LoadComponentAsync(leaderboard = new MultiSpectatorLeaderboard(users)
            {
                Expanded = { Value = true },
            }, _ =>
            {
                foreach (var instance in instances)
                    leaderboard.AddClock(instance.UserId, instance.SpectatorPlayerClock);

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

            // Start with adjustments from the first player to keep a sane state.
            bindAudioAdjustments(instances.First());
        }

        protected override void Update()
        {
            base.Update();

            if (!isCandidateAudioSource(currentAudioSource?.SpectatorPlayerClock))
            {
                currentAudioSource = instances.Where(i => isCandidateAudioSource(i.SpectatorPlayerClock)).MinBy(i => Math.Abs(i.SpectatorPlayerClock.CurrentTime - syncManager.CurrentMasterTime));

                // Only bind adjustments if there's actually a valid source, else just use the previous ones to ensure no sudden changes to audio.
                if (currentAudioSource != null)
                    bindAudioAdjustments(currentAudioSource);

                foreach (var instance in instances)
                    instance.Mute = instance != currentAudioSource;
            }
        }

        private void bindAudioAdjustments(PlayerArea first)
        {
            if (boundAdjustments != null)
                masterClockContainer.AdjustmentsFromMods.UnbindAdjustments(boundAdjustments);

            boundAdjustments = first.ClockAdjustmentsFromMods;
            masterClockContainer.AdjustmentsFromMods.BindAdjustments(boundAdjustments);
        }

        private bool isCandidateAudioSource(SpectatorPlayerClock? clock)
            => clock?.IsRunning == true && !clock.IsCatchingUp && !clock.WaitingOnFrames;

        private void performInitialSeek()
        {
            // We want to start showing gameplay as soon as possible.
            // Each client may be in a different place in the beatmap, so we need to do our best to find a common
            // starting point.
            //
            // Preferring a lower value ensures that we don't have some clients stuttering to keep up.
            List<double> minFrameTimes = new List<double>();

            foreach (var instance in instances)
            {
                if (instance.Score == null)
                    continue;

                minFrameTimes.Add(instance.Score.Replay.Frames.MinBy(f => f.Time)?.Time ?? 0);
            }

            // Remove any outliers (only need to worry about removing those lower than the mean since we will take a Min() after).
            double mean = minFrameTimes.Average();
            minFrameTimes.RemoveAll(t => mean - t > 1000);

            double startTime = minFrameTimes.Min();

            masterClockContainer.Reset(startTime, true);
            Logger.Log($"Multiplayer spectator seeking to initial time of {startTime}");
        }

        protected override void OnNewPlayingUserState(int userId, SpectatorState spectatorState)
        {
        }

        protected override void StartGameplay(int userId, SpectatorGameplayState spectatorGameplayState) => Schedule(() =>
        {
            var playerArea = instances.Single(i => i.UserId == userId);

            // The multiplayer spectator flow requires the client to return to a higher level screen
            // (ie. StartGameplay should only be called once per player).
            //
            // Meanwhile, the solo spectator flow supports multiple `StartGameplay` calls.
            // To ensure we don't crash out in an edge case where this is called more than once in multiplayer,
            // guard against re-entry for the same player.
            if (playerArea.Score != null)
                return;

            playerArea.LoadScore(spectatorGameplayState.Score);
        });

        protected override void FailGameplay(int userId) => Schedule(() =>
        {
            // We probably want to visualise this in the future.

            var instance = instances.Single(i => i.UserId == userId);
            syncManager.RemoveManagedClock(instance.SpectatorPlayerClock);
        });

        protected override void PassGameplay(int userId) => Schedule(() =>
        {
            var instance = instances.Single(i => i.UserId == userId);
            syncManager.RemoveManagedClock(instance.SpectatorPlayerClock);
        });

        protected override void QuitGameplay(int userId) => Schedule(() =>
        {
            RemoveUser(userId);

            var instance = instances.Single(i => i.UserId == userId);

            instance.FadeColour(colours.Gray4, 400, Easing.OutQuint);
            syncManager.RemoveManagedClock(instance.SpectatorPlayerClock);
        });

        public override bool OnBackButton()
        {
            if (multiplayerClient.Room == null)
                return base.OnBackButton();

            // On a manual exit, set the player back to idle unless gameplay has finished.
            // Of note, this doesn't cover exiting using alt-f4 or menu home option.
            if (multiplayerClient.Room.State != MultiplayerRoomState.Open)
                multiplayerClient.ChangeState(MultiplayerUserState.Idle);

            return base.OnBackButton();
        }
    }
}
