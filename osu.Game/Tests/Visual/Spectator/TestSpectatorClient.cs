// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
using osu.Game.Rulesets;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace osu.Game.Tests.Visual.Spectator
{
    public partial class TestSpectatorClient : SpectatorClient
    {
        /// <summary>
        /// Maximum number of frames sent per bundle via <see cref="SendFramesFromUser"/>.
        /// </summary>
        public const int FRAME_BUNDLE_SIZE = 10;

        /// <summary>
        /// Whether to force send operations to fail (simulating a network issue).
        /// </summary>
        public bool ShouldFailSendingFrames { get; set; }

        public int FrameSendAttempts { get; private set; }

        public override IBindable<bool> IsConnected => isConnected;
        private readonly BindableBool isConnected = new BindableBool(true);

        public IReadOnlyDictionary<int, ReplayFrame> LastReceivedUserFrames => lastReceivedUserFrames;

        private readonly Dictionary<int, ReplayFrame> lastReceivedUserFrames = new Dictionary<int, ReplayFrame>();

        private readonly Dictionary<int, int> userBeatmapDictionary = new Dictionary<int, int>();
        private readonly Dictionary<int, APIMod[]> userModsDictionary = new Dictionary<int, APIMod[]>();
        private readonly Dictionary<int, int> userNextFrameDictionary = new Dictionary<int, int>();

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private RulesetStore rulesetStore { get; set; } = null!;

        public TestSpectatorClient()
        {
            OnNewFrames += (i, bundle) => lastReceivedUserFrames[i] = bundle.Frames[^1];
        }

        /// <summary>
        /// Starts play for an arbitrary user.
        /// </summary>
        /// <param name="userId">The user to start play for.</param>
        /// <param name="beatmapId">The playing beatmap id.</param>
        /// <param name="mods">The mods the user has applied.</param>
        public void SendStartPlay(int userId, int beatmapId, APIMod[]? mods = null)
        {
            userBeatmapDictionary[userId] = beatmapId;
            userModsDictionary[userId] = mods ?? Array.Empty<APIMod>();
            userNextFrameDictionary[userId] = 0;
            sendPlayingState(userId);
        }

        /// <summary>
        /// Ends play for an arbitrary user.
        /// </summary>
        /// <param name="userId">The user to end play for.</param>
        /// <param name="state">The spectator state to end play with.</param>
        public void SendEndPlay(int userId, SpectatedUserState state = SpectatedUserState.Quit)
        {
            if (!userBeatmapDictionary.ContainsKey(userId))
                return;

            ((ISpectatorClient)this).UserFinishedPlaying(userId, new SpectatorState
            {
                BeatmapID = userBeatmapDictionary[userId],
                RulesetID = 0,
                Mods = userModsDictionary[userId],
                State = state
            });

            userBeatmapDictionary.Remove(userId);
            userModsDictionary.Remove(userId);
        }

        /// <summary>
        /// Sends frames for an arbitrary user, in bundles containing 10 frames each.
        /// This bypasses the standard queueing mechanism completely and should only be used to test cases where multiple users need to be sending data.
        /// Importantly, <see cref="ShouldFailSendingFrames"/> will have no effect.
        /// </summary>
        /// <param name="userId">The user to send frames for.</param>
        /// <param name="count">The total number of frames to send.</param>
        /// <param name="startTime">The time to start gameplay frames from.</param>
        public void SendFramesFromUser(int userId, int count, double startTime = 0)
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
                frames.Add(new LegacyReplayFrame(currentFrameIndex * 100 + startTime, RNG.Next(0, 512), RNG.Next(0, 512), buttonState));
            }

            flush();

            userNextFrameDictionary[userId] = currentFrameIndex;

            void flush()
            {
                if (frames.Count == 0)
                    return;

                var bundle = new FrameDataBundle(new ScoreInfo
                {
                    Combo = currentFrameIndex,
                    TotalScore = (long)(currentFrameIndex * 123478 * RNG.NextDouble(0.99, 1.01)),
                    Accuracy = RNG.NextDouble(0.98, 1),
                }, new ScoreProcessor(rulesetStore.GetRuleset(0)!.CreateInstance()), frames.ToArray());
                ((ISpectatorClient)this).UserSentFrames(userId, bundle);

                frames.Clear();
            }
        }

        protected override Task BeginPlayingInternal(long? scoreToken, SpectatorState state)
        {
            // Track the local user's playing beatmap ID.
            Debug.Assert(state.BeatmapID != null);
            userBeatmapDictionary[api.LocalUser.Value.Id] = state.BeatmapID.Value;
            userModsDictionary[api.LocalUser.Value.Id] = state.Mods.ToArray();

            return ((ISpectatorClient)this).UserBeganPlaying(api.LocalUser.Value.Id, state);
        }

        protected override Task SendFramesInternal(FrameDataBundle bundle)
        {
            FrameSendAttempts++;

            if (ShouldFailSendingFrames)
                return Task.FromException(new InvalidOperationException($"Intentional fail via {nameof(ShouldFailSendingFrames)}"));

            return ((ISpectatorClient)this).UserSentFrames(api.LocalUser.Value.Id, bundle);
        }

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
                Mods = userModsDictionary[userId],
                State = SpectatedUserState.Playing
            });
        }

        protected override async Task DisconnectInternal()
        {
            await base.DisconnectInternal().ConfigureAwait(false);
            isConnected.Value = false;
        }
    }
}
