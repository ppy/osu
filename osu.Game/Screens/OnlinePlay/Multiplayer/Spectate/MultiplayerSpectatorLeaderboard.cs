// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Timing;
using osu.Game.Online.Spectator;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play.HUD;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Spectate
{
    public class MultiplayerSpectatorLeaderboard : MultiplayerGameplayLeaderboard
    {
        private readonly Dictionary<int, TrackedUserData> trackedData = new Dictionary<int, TrackedUserData>();

        public MultiplayerSpectatorLeaderboard(ScoreProcessor scoreProcessor, int[] userIds)
            : base(scoreProcessor, userIds)
        {
        }

        public void AddClock(int userId, IClock source) => trackedData[userId] = new TrackedUserData(source);

        public void RemoveClock(int userId) => trackedData.Remove(userId);

        protected override void OnIncomingFrames(int userId, FrameDataBundle bundle)
        {
            if (!trackedData.TryGetValue(userId, out var data))
                return;

            data.Frames.Add(new TimedFrameHeader(bundle.Frames.First().Time, bundle.Header));
        }

        protected override void Update()
        {
            base.Update();

            foreach (var (userId, data) in trackedData)
            {
                var targetTime = data.Clock.CurrentTime;

                if (data.Frames.Count == 0)
                    continue;

                int frameIndex = data.Frames.BinarySearch(new TimedFrameHeader(targetTime));
                if (frameIndex < 0)
                    frameIndex = ~frameIndex;
                frameIndex = Math.Clamp(frameIndex - 1, 0, data.Frames.Count - 1);

                SetCurrentFrame(userId, data.Frames[frameIndex].Header);
            }
        }

        private class TrackedUserData
        {
            public readonly IClock Clock;
            public readonly List<TimedFrameHeader> Frames = new List<TimedFrameHeader>();

            public TrackedUserData(IClock clock)
            {
                Clock = clock;
            }
        }

        private class TimedFrameHeader : IComparable<TimedFrameHeader>
        {
            public readonly double Time;
            public readonly FrameHeader Header;

            public TimedFrameHeader(double time)
            {
                Time = time;
            }

            public TimedFrameHeader(double time, FrameHeader header)
            {
                Time = time;
                Header = header;
            }

            public int CompareTo(TimedFrameHeader other) => Time.CompareTo(other.Time);
        }
    }
}
