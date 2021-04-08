// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Online.Spectator;
using osu.Game.Screens.Play;
using osu.Game.Screens.Spectate;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Spectate
{
    public class MultiplayerSpectator : SpectatorScreen
    {
        private const double min_duration_to_allow_playback = 50;
        private const double max_sync_offset = 2;

        // Isolates beatmap/ruleset to this screen.
        public override bool DisallowExternalBeatmapRulesetChanges => true;

        public bool AllPlayersLoaded => instances.All(p => p?.PlayerLoaded == true);

        [Resolved]
        private SpectatorStreamingClient spectatorClient { get; set; }

        private readonly PlayerInstance[] instances;
        private PlayerGrid grid;

        public MultiplayerSpectator(int[] userIds)
            : base(userIds.AsSpan().Slice(0, Math.Min(16, userIds.Length)).ToArray())
        {
            instances = new PlayerInstance[userIds.Length];
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = grid = new PlayerGrid
            {
                RelativeSizeAxes = Axes.Both
            };
        }

        protected override void Update()
        {
            base.Update();
            updatePlayTime();
        }

        private bool gameplayStarted;

        private void updatePlayTime()
        {
            if (gameplayStarted)
            {
                ensurePlaying(instances.Select(i => i.Beatmap.Track.CurrentTime).Max());
                return;
            }

            // Make sure all players are loaded.
            if (!AllPlayersLoaded)
            {
                ensureAllStopped();
                return;
            }

            if (!instances.All(i => i.Score.Replay.Frames.Count > 0))
            {
                ensureAllStopped();
                return;
            }

            gameplayStarted = true;
        }

        private void ensureAllStopped()
        {
            foreach (var inst in instances)
                inst.ChildrenOfType<GameplayClockContainer>().SingleOrDefault()?.Stop();
        }

        private readonly BindableDouble catchupFrequencyAdjustment = new BindableDouble(2.0);

        private void ensurePlaying(double targetTime)
        {
            foreach (var inst in instances)
            {
                double lastFrameTime = inst.Score.Replay.Frames.Select(f => f.Time).Last();
                double currentTime = inst.Beatmap.Track.CurrentTime;

                // If we have enough frames to play back, start playback.
                if (Precision.DefinitelyBigger(lastFrameTime, currentTime, min_duration_to_allow_playback))
                {
                    inst.ChildrenOfType<GameplayClockContainer>().Single().Start();

                    if (targetTime < lastFrameTime && targetTime > currentTime + max_sync_offset)
                        inst.Beatmap.Track.AddAdjustment(AdjustableProperty.Frequency, catchupFrequencyAdjustment);
                    else
                        inst.Beatmap.Track.RemoveAdjustment(AdjustableProperty.Frequency, catchupFrequencyAdjustment);
                }
                else
                    inst.Beatmap.Track.RemoveAdjustment(AdjustableProperty.Frequency, catchupFrequencyAdjustment);
            }
        }

        protected override void OnUserStateChanged(int userId, SpectatorState spectatorState)
        {
        }

        protected override void StartGameplay(int userId, GameplayState gameplayState)
        {
            int userIndex = getIndexForUser(userId);
            var existingInstance = instances[userIndex];

            if (existingInstance != null)
                grid.Remove(existingInstance);

            LoadComponentAsync(instances[userIndex] = new PlayerInstance(gameplayState.Score), d =>
            {
                if (instances[userIndex] == d)
                    grid.Add(d);
            });
        }

        protected override void EndGameplay(int userId)
        {
            spectatorClient.StopWatchingUser(userId);
        }

        private int getIndexForUser(int userId) => Array.IndexOf(UserIds, userId);
    }
}
