// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Utils;
using osu.Game.Online.API;
using osu.Game.Online.Spectator;
using osu.Game.Replays.Legacy;
using osu.Game.Scoring;

namespace osu.Game.Tests.Visual.Spectator
{
    public class TestSpectatorClient : SpectatorClient
    {
        public override IBindable<bool> IsConnected { get; } = new Bindable<bool>(true);

        private readonly Dictionary<int, int> userBeatmapDictionary = new Dictionary<int, int>();

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        /// <summary>
        /// Starts play for an arbitrary user.
        /// </summary>
        /// <param name="userId">The user to start play for.</param>
        /// <param name="beatmapId">The playing beatmap id.</param>
        public void StartPlay(int userId, int beatmapId)
        {
            userBeatmapDictionary[userId] = beatmapId;
            sendPlayingState(userId);
        }

        /// <summary>
        /// Ends play for an arbitrary user.
        /// </summary>
        /// <param name="userId">The user to end play for.</param>
        public void EndPlay(int userId)
        {
            if (!PlayingUsers.Contains(userId))
                return;

            ((ISpectatorClient)this).UserFinishedPlaying(userId, new SpectatorState
            {
                BeatmapID = userBeatmapDictionary[userId],
                RulesetID = 0,
            });
        }

        public new void Schedule(Action action) => base.Schedule(action);

        /// <summary>
        /// Sends frames for an arbitrary user.
        /// </summary>
        /// <param name="userId">The user to send frames for.</param>
        /// <param name="index">The frame index.</param>
        /// <param name="count">The number of frames to send.</param>
        public void SendFrames(int userId, int index, int count)
        {
            var frames = new List<LegacyReplayFrame>();

            for (int i = index; i < index + count; i++)
            {
                var buttonState = i == index + count - 1 ? ReplayButtonState.None : ReplayButtonState.Left1;

                frames.Add(new LegacyReplayFrame(i * 100, RNG.Next(0, 512), RNG.Next(0, 512), buttonState));
            }

            var bundle = new FrameDataBundle(new ScoreInfo { Combo = index + count }, frames);
            ((ISpectatorClient)this).UserSentFrames(userId, bundle);
        }

        protected override Task BeginPlayingInternal(SpectatorState state)
        {
            // Track the local user's playing beatmap ID.
            Debug.Assert(state.BeatmapID != null);
            userBeatmapDictionary[api.LocalUser.Value.Id] = state.BeatmapID.Value;

            return ((ISpectatorClient)this).UserBeganPlaying(api.LocalUser.Value.Id, state);
        }

        protected override Task SendFramesInternal(FrameDataBundle data) => ((ISpectatorClient)this).UserSentFrames(api.LocalUser.Value.Id, data);

        protected override Task EndPlayingInternal(SpectatorState state) => ((ISpectatorClient)this).UserFinishedPlaying(api.LocalUser.Value.Id, state);

        protected override Task WatchUserInternal(int userId)
        {
            // When newly watching a user, the server sends the playing state immediately.
            if (PlayingUsers.Contains(userId))
                sendPlayingState(userId);

            return Task.CompletedTask;
        }

        protected override Task StopWatchingUserInternal(int userId) => Task.CompletedTask;

        private void sendPlayingState(int userId)
        {
            ((ISpectatorClient)this).UserBeganPlaying(userId, new SpectatorState
            {
                BeatmapID = userBeatmapDictionary[userId],
                RulesetID = 0,
            });
        }
    }
}
