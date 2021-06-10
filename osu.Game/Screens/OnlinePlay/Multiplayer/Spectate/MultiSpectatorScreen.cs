// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Spectator;
using osu.Game.Screens.Play;
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
        public override bool AllowRateAdjustments => false;

        /// <summary>
        /// Whether all spectating players have finished loading.
        /// </summary>
        public bool AllPlayersLoaded => instances.All(p => p?.PlayerLoaded == true);

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

        /// <summary>
        /// Creates a new <see cref="MultiSpectatorScreen"/>.
        /// </summary>
        /// <param name="userIds">The players to spectate.</param>
        public MultiSpectatorScreen(int[] userIds)
            : base(userIds.Take(PlayerGrid.MAX_PLAYERS).ToArray())
        {
            instances = new PlayerArea[UserIds.Count];
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Container leaderboardContainer;
            masterClockContainer = new MasterGameplayClockContainer(Beatmap.Value, 0);

            InternalChildren = new[]
            {
                (Drawable)(syncManager = new CatchUpSyncManager(masterClockContainer)),
                masterClockContainer.WithChild(new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    ColumnDimensions = new[]
                    {
                        new Dimension(GridSizeMode.AutoSize)
                    },
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
                })
            };

            for (int i = 0; i < UserIds.Count; i++)
            {
                grid.Add(instances[i] = new PlayerArea(UserIds[i], masterClockContainer.GameplayClock));
                syncManager.AddPlayerClock(instances[i].GameplayClock);
            }

            // Todo: This is not quite correct - it should be per-user to adjust for other mod combinations.
            var playableBeatmap = Beatmap.Value.GetPlayableBeatmap(Ruleset.Value);
            var scoreProcessor = Ruleset.Value.CreateInstance().CreateScoreProcessor();
            scoreProcessor.ApplyBeatmap(playableBeatmap);

            LoadComponentAsync(leaderboard = new MultiSpectatorLeaderboard(scoreProcessor, UserIds.ToArray())
            {
                Expanded = { Value = true },
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
            }, leaderboardContainer.Add);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            masterClockContainer.Stop();
            masterClockContainer.Reset();
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

        protected override void OnUserStateChanged(int userId, SpectatorState spectatorState)
        {
        }

        protected override void StartGameplay(int userId, GameplayState gameplayState)
        {
            var instance = instances.Single(i => i.UserId == userId);

            instance.LoadScore(gameplayState.Score);

            syncManager.AddPlayerClock(instance.GameplayClock);
            leaderboard.AddClock(instance.UserId, instance.GameplayClock);
        }

        protected override void EndGameplay(int userId)
        {
            RemoveUser(userId);
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
