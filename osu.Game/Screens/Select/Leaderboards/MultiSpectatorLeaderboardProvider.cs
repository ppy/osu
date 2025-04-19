// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Timing;
using osu.Game.Online.Multiplayer;

namespace osu.Game.Screens.Select.Leaderboards
{
    public partial class MultiSpectatorLeaderboardProvider : MultiplayerLeaderboardProvider
    {
        public MultiSpectatorLeaderboardProvider(MultiplayerRoomUser[] users)
            : base(users)
        {
        }

        public void AddClock(int userId, IClock clock)
        {
            if (!UserScores.TryGetValue(userId, out var data))
                throw new ArgumentException(@"Provided user is not tracked by this leaderboard", nameof(userId));

            data.ScoreProcessor.ReferenceClock = clock;
        }

        protected override void Update()
        {
            base.Update();

            foreach (var (_, data) in UserScores)
                data.ScoreProcessor.UpdateScore();
        }
    }
}
