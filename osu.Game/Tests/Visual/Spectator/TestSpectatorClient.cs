// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Utils;
using osu.Game.Online.API;
using osu.Game.Online.Spectator;
using osu.Game.Replays.Legacy;
using osu.Game.Rulesets.Replays;
using osu.Game.Scoring;

namespace osu.Game.Tests.Visual.Spectator
{
    public class TestSpectatorClient : SpectatorClient
    {
        /// <summary>
        /// Maximum number of frames sent per bundle via <see cref="SendFrames"/>.
        /// </summary>
        public const int FRAME_BUNDLE_SIZE = 10;

        public override IBindable<bool> IsConnected { get; } = new Bindable<bool>(true);

        public IReadOnlyDictionary<int, ReplayFrame> LastReceivedUserFrames => lastReceivedUserFrames;

        private readonly Dictionary<int, ReplayFrame> lastReceivedUserFrames = new Dictionary<int, ReplayFrame>();

        private readonly Dictionary<int, int> userBeatmapDictionary = new Dictionary<int, int>();
        private readonly Dictionary<int, int> userNextFrameDictionary = new Dictionary<int, int>();

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        public TestSpectatorClient()
        {
            OnNewFrames += (i, bundle) => lastReceivedUserFrames[i] = bundle.Frames[^1];
        }

        /// <summary>
        /// Starts play for an arbitrary user.
        /// </summary>
        /// <param name="userId">The user to start play for.</param>
        /// <param name="beatmapId">The playing beatmap id.</param>
        public void StartPlay(int userId, int beatmapId)
        {
            userBeatmapDictionary[userId] = beatmapId;
            userNextFrameDictionary[userId] = 0;
            sendPlayingState(userId);
        }

        /// <summary>
        /// Ends play for an arbitrary user.
        /// </summary>
        /// <param name="userId">The user to end play for.</param>
        public void EndPlay(int userId)
        {
            if (!userBeatmapDictionary.ContainsKey(userId))
                return;

            ((ISpectatorClient)this).UserFinishedPlaying(userId, new SpectatorState
            {
                BeatmapID = userBeatmapDictionary[userId],
                RulesetID = 0,
            });

            userBeatmapDictionary.Remove(userId);
        }

        public new void Schedule(Action action) => base.Schedule(action);

        /// <summary>
        /// Sends frames for an arbitrary user, in bundles containing 10 frames each.
        /// </summary>
        /// <param name="userId">The user to send frames for.</param>
        /// <param name="count">The total number of frames to send.</param>
        public void SendFrames(int userId, int count)
        {
            var frames = new List<LegacyReplayFrame>();

            int currentFrameIndex = userNextFrameDictionary[userId];
            int lastFrameIndex = currentFrameIndex + count - 1;

            for (; currentFrameIndex <= lastFrameIndex; currentFrameIndex++)
            {
                // This is done in the next frame so that currentFrameIndex is updated to the correct value.
                if (frames.Count == FRAME_BUNDLE_SIZE)
                    flush();

                var buttonState = currentFrameIndex == lastFrameIndex ? ReplayButtonState.None : ReplayButtonState.Left1;
                frames.Add(new LegacyReplayFrame(currentFrameIndex * 100, RNG.Next(0, 512), RNG.Next(0, 512), buttonState));
            }

            flush();

            userNextFrameDictionary[userId] = currentFrameIndex;

            void flush()
            {
                if (frames.Count == 0)
                    return;

                var bundle = new FrameDataBundle(new ScoreInfo { Combo = currentFrameIndex }, frames.ToArray());
                ((ISpectatorClient)this).UserSentFrames(userId, bundle);

                frames.Clear();
            }
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
            if (userBeatmapDictionary.ContainsKey(userId))
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
