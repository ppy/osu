// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.HUD;
using osu.Game.Screens.Ranking;
using osu.Game.Users;

namespace osu.Game.Screens.OnlinePlay.Multiplayer
{
    public partial class MultiplayerPlayer : RoomSubmittingPlayer
    {
        protected override bool PauseOnFocusLost => false;

        protected override UserActivity InitialActivity => new UserActivity.InMultiplayerGame(Beatmap.Value.BeatmapInfo, Ruleset.Value);

        [Resolved]
        private MultiplayerClient client { get; set; }

        private IBindable<bool> isConnected;

        private readonly TaskCompletionSource<bool> resultsReady = new TaskCompletionSource<bool>();

        private readonly MultiplayerRoomUser[] users;

        private LoadingLayer loadingDisplay;

        private MultiplayerGameplayLeaderboard multiplayerLeaderboard;

        /// <summary>
        /// Construct a multiplayer player.
        /// </summary>
        /// <param name="room">The room.</param>
        /// <param name="playlistItem">The playlist item to be played.</param>
        /// <param name="users">The users which are participating in this game.</param>
        public MultiplayerPlayer(Room room, PlaylistItem playlistItem, MultiplayerRoomUser[] users)
            : base(room, playlistItem, new PlayerConfiguration
            {
                AllowPause = false,
                AllowRestart = false,
                AllowFailAnimation = false,
                AllowSkipping = room.AutoSkip.Value,
                AutomaticallySkipIntro = room.AutoSkip.Value,
                AlwaysShowLeaderboard = true,
            })
        {
            this.users = users;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (!LoadedBeatmapSuccessfully)
                return;

            LoadComponentAsync(new GameplayChatDisplay(Room)
            {
                Expanded = { BindTarget = LeaderboardExpandedState },
            }, chat => HUDOverlay.LeaderboardFlow.Insert(2, chat));

            HUDOverlay.Add(loadingDisplay = new LoadingLayer(true) { Depth = float.MaxValue });
        }

        protected override GameplayLeaderboard CreateGameplayLeaderboard() => multiplayerLeaderboard = new MultiplayerGameplayLeaderboard(users);

        protected override void AddLeaderboardToHUD(GameplayLeaderboard leaderboard)
        {
            Debug.Assert(leaderboard == multiplayerLeaderboard);

            HUDOverlay.LeaderboardFlow.Insert(0, leaderboard);

            if (multiplayerLeaderboard.TeamScores.Count >= 2)
            {
                LoadComponentAsync(new GameplayMatchScoreDisplay
                {
                    Team1Score = { BindTarget = multiplayerLeaderboard.TeamScores.First().Value },
                    Team2Score = { BindTarget = multiplayerLeaderboard.TeamScores.Last().Value },
                    Expanded = { BindTarget = HUDOverlay.ShowHud },
                }, scoreDisplay => HUDOverlay.LeaderboardFlow.Insert(1, scoreDisplay));
            }
        }

        protected override void LoadAsyncComplete()
        {
            base.LoadAsyncComplete();

            if (!LoadedBeatmapSuccessfully)
                return;

            if (!ValidForResume)
                return; // token retrieval may have failed.

            client.GameplayStarted += onGameplayStarted;
            client.ResultsReady += onResultsReady;

            ScoreProcessor.HasCompleted.BindValueChanged(_ =>
            {
                // wait for server to tell us that results are ready (see SubmitScore implementation)
                loadingDisplay.Show();
            });

            isConnected = client.IsConnected.GetBoundCopy();
            isConnected.BindValueChanged(connected => Schedule(() =>
            {
                if (!connected.NewValue)
                {
                    // messaging to the user about this disconnect will be provided by the MultiplayerMatchSubScreen.
                    failAndBail();
                }
            }), true);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Debug.Assert(client.Room != null);
        }

        protected override void StartGameplay()
        {
            // We can enter this screen one of two ways:
            // 1. Via the automatic natural progression of PlayerLoader into Player.
            //    We'll arrive here in a Loaded state, and we need to let the server know that we're ready to start.
            // 2. Via the server forcefully starting gameplay because players have been hanging out in PlayerLoader for too long.
            //    We'll arrive here in a Playing state, and we should neither show the loading spinner nor tell the server that we're ready to start (gameplay has already started).
            //
            // The base call is blocked here because in both cases gameplay is started only when the server says so via onGameplayStarted().

            if (client.LocalUser?.State == MultiplayerUserState.Loaded)
            {
                loadingDisplay.Show();
                client.ChangeState(MultiplayerUserState.ReadyForGameplay);
            }

            // This will pause the clock, pending the gameplay started callback from the server.
            GameplayClockContainer.Reset();
        }

        private void failAndBail(string message = null)
        {
            if (!string.IsNullOrEmpty(message))
                Logger.Log(message, LoggingTarget.Runtime, LogLevel.Important);

            Schedule(() => PerformExit(false));
        }

        private void onGameplayStarted() => Scheduler.Add(() =>
        {
            if (!this.IsCurrentScreen())
                return;

            loadingDisplay.Hide();
            base.StartGameplay();
        });

        private void onResultsReady()
        {
            // Schedule is required to ensure that `TaskCompletionSource.SetResult` is not called more than once.
            // A scenario where this can occur is if this instance is not immediately disposed (ie. async disposal queue).
            Schedule(() =>
            {
                if (!this.IsCurrentScreen())
                    return;

                resultsReady.SetResult(true);
            });
        }

        protected override async Task PrepareScoreForResultsAsync(Score score)
        {
            await base.PrepareScoreForResultsAsync(score).ConfigureAwait(false);

            await client.ChangeState(MultiplayerUserState.FinishedPlay).ConfigureAwait(false);

            // Await up to 60 seconds for results to become available (6 api request timeouts).
            // This is arbitrary just to not leave the player in an essentially deadlocked state if any connection issues occur.
            await Task.WhenAny(resultsReady.Task, Task.Delay(TimeSpan.FromSeconds(60))).ConfigureAwait(false);
        }

        protected override ResultsScreen CreateResults(ScoreInfo score)
        {
            Debug.Assert(Room.RoomID.Value != null);

            return multiplayerLeaderboard.TeamScores.Count == 2
                ? new MultiplayerTeamResultsScreen(score, Room.RoomID.Value.Value, PlaylistItem, multiplayerLeaderboard.TeamScores)
                : new MultiplayerResultsScreen(score, Room.RoomID.Value.Value, PlaylistItem);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (client != null)
            {
                client.GameplayStarted -= onGameplayStarted;
                client.ResultsReady -= onResultsReady;
            }
        }
    }
}
