// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Scoring;
using System.Collections.Generic;

namespace osu.Game.Online.Leaderboards
{
    public interface ILeaderboard
    {
        /// <summary>
        /// Contains a list of scores retrieved from the API or <see cref="ScoreManager"/>.
        /// </summary>
        IEnumerable<ScoreInfo> Scores { get; }

        /// <summary>
        /// Whether the current scope requires online access.
        /// </summary>
        bool IsOnlineScope { get; }

        /// <summary>
        /// Updates the scores shown on the leaderboard.
        /// </summary>
        void RefreshScores();
    }
}
