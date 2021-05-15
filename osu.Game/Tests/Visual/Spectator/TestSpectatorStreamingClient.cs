// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Concurrent;
using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Utils;
using osu.Game.Online;
using osu.Game.Online.Spectator;
using osu.Game.Replays.Legacy;
using osu.Game.Scoring;

namespace osu.Game.Tests.Visual.Spectator
{
    public class TestSpectatorStreamingClient : SpectatorStreamingClient
    {
        public new BindableList<int> PlayingUsers => (BindableList<int>)base.PlayingUsers;
        private readonly ConcurrentDictionary<int, byte> watchingUsers = new ConcurrentDictionary<int, byte>();

        private readonly Dictionary<int, int> userBeatmapDictionary = new Dictionary<int, int>();
        private readonly Dictionary<int, bool> userSentStateDictionary = new Dictionary<int, bool>();

        public TestSpectatorStreamingClient()
            : base(new DevelopmentEndpointConfiguration())
        {
        }

        public void StartPlay(int userId, int beatmapId)
        {
            userBeatmapDictionary[userId] = beatmapId;
            sendState(userId, beatmapId);
        }

        public void EndPlay(int userId, int beatmapId)
        {
            ((ISpectatorClient)this).UserFinishedPlaying(userId, new SpectatorState
            {
                BeatmapID = beatmapId,
                RulesetID = 0,
            });

            userBeatmapDictionary.Remove(userId);
            userSentStateDictionary.Remove(userId);
        }

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

            if (!userSentStateDictionary[userId])
                sendState(userId, userBeatmapDictionary[userId]);
        }

        public override void WatchUser(int userId)
        {
            base.WatchUser(userId);

            // When newly watching a user, the server sends the playing state immediately.
            if (watchingUsers.TryAdd(userId, 0) && PlayingUsers.Contains(userId))
                sendState(userId, userBeatmapDictionary[userId]);
        }

        public override void StopWatchingUser(int userId)
        {
            base.StopWatchingUser(userId);
            watchingUsers.TryRemove(userId, out _);
        }

        private void sendState(int userId, int beatmapId)
        {
            ((ISpectatorClient)this).UserBeganPlaying(userId, new SpectatorState
            {
                BeatmapID = beatmapId,
                RulesetID = 0,
            });

            userSentStateDictionary[userId] = true;
        }
    }
}
