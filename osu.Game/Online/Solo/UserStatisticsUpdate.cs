// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Scoring;
using osu.Game.Users;

namespace osu.Game.Online.Solo
{
    /// <summary>
    /// Contains data about the change in a user's profile statistics after completing a score.
    /// </summary>
    public class UserStatisticsUpdate
    {
        /// <summary>
        /// The score set by the user that triggered the update.
        /// </summary>
        public ScoreInfo Score { get; }

        /// <summary>
        /// The user's profile statistics prior to the score being set.
        /// </summary>
        public UserStatistics Before { get; }

        /// <summary>
        /// The user's profile statistics after the score was set.
        /// </summary>
        public UserStatistics After { get; }

        /// <summary>
        /// Creates a new <see cref="UserStatisticsUpdate"/>.
        /// </summary>
        /// <param name="score">The score set by the user that triggered the update.</param>
        /// <param name="before">The user's profile statistics prior to the score being set.</param>
        /// <param name="after">The user's profile statistics after the score was set.</param>
        public UserStatisticsUpdate(ScoreInfo score, UserStatistics before, UserStatistics after)
        {
            Score = score;
            Before = before;
            After = after;
        }
    }
}
