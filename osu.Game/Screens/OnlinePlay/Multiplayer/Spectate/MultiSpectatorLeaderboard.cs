// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using JetBrains.Annotations;
using osu.Framework.Timing;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play.HUD;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Spectate
{
    public class MultiSpectatorLeaderboard : MultiplayerGameplayLeaderboard
    {
        public MultiSpectatorLeaderboard([NotNull] ScoreProcessor scoreProcessor, int[] userIds)
            : base(scoreProcessor, userIds)
        {
        }

        public void AddClock(int userId, IClock clock)
        {
            if (!UserScores.TryGetValue(userId, out var data))
                return;

            ((SpectatingTrackedUserData)data).Clock = clock;
        }

        public void RemoveClock(int userId)
        {
            if (!UserScores.TryGetValue(userId, out var data))
                return;

            ((SpectatingTrackedUserData)data).Clock = null;
        }

        protected override TrackedUserData CreateUserData(int userId, ScoreProcessor scoreProcessor) => new SpectatingTrackedUserData(userId, scoreProcessor);

        protected override void Update()
        {
            base.Update();

            foreach (var (_, data) in UserScores)
                data.UpdateScore();
        }

        private class SpectatingTrackedUserData : TrackedUserData
        {
            [CanBeNull]
            public IClock Clock;

            public SpectatingTrackedUserData(int userId, ScoreProcessor scoreProcessor)
                : base(userId, scoreProcessor)
            {
            }

            public override void UpdateScore()
            {
                if (Frames.Count == 0)
                    return;

                if (Clock == null)
                    return;

                int frameIndex = Frames.BinarySearch(new TimedFrame(Clock.CurrentTime));
                if (frameIndex < 0)
                    frameIndex = ~frameIndex;
                frameIndex = Math.Clamp(frameIndex - 1, 0, Frames.Count - 1);

                SetFrame(Frames[frameIndex]);
            }
        }
    }
}
