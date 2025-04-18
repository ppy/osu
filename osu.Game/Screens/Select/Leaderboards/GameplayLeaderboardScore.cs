// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Online.Spectator;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Scoring.Legacy;
using osu.Game.Users;

namespace osu.Game.Screens.Select.Leaderboards
{
    /// <summary>
    /// Represents a score shown on a gameplay leaderboard.
    /// The score is expected to update itself as gameplay progresses.
    /// </summary>
    public class GameplayLeaderboardScore
    {
        /// <summary>
        /// The user playing.
        /// </summary>
        public IUser User { get; }

        /// <summary>
        /// Whether the score is being tracked.
        /// Generally understood as true when this score is the score of the local user currently playing.
        /// </summary>
        public bool Tracked { get; }

        /// <summary>
        /// The current total of the score.
        /// </summary>
        public BindableLong TotalScore { get; } = new BindableLong();

        /// <summary>
        /// The current accuracy of the score.
        /// </summary>
        public BindableDouble Accuracy { get; } = new BindableDouble();

        /// <summary>
        /// The current combo of the score.
        /// </summary>
        public BindableInt Combo { get; } = new BindableInt();

        /// <summary>
        /// Whether the user playing has quit.
        /// </summary>
        public BindableBool HasQuit { get; } = new BindableBool();

        /// <summary>
        /// An optional value to guarantee stable ordering.
        /// Lower numbers will appear higher in cases of <see cref="TotalScore"/> ties.
        /// </summary>
        public Bindable<long> DisplayOrder { get; } = new BindableLong();

        /// <summary>
        /// A custom function which handles converting a score to a display score using a provided <see cref="ScoringMode"/>.
        /// </summary>
        /// <remarks>
        /// If no function is provided, <see cref="TotalScore"/> will be used verbatim.
        /// </remarks>
        public Func<ScoringMode, long> GetDisplayScore { get; set; }

        /// <summary>
        /// The colour of the team that the user playing is on, if any.
        /// </summary>
        public Colour4? TeamColour { get; init; }

        public GameplayLeaderboardScore(IUser user, ScoreProcessor scoreProcessor, bool tracked)
        {
            User = user;
            Tracked = tracked;
            TotalScore.BindTarget = scoreProcessor.TotalScore;
            Accuracy.BindTarget = scoreProcessor.Accuracy;
            Combo.BindTarget = scoreProcessor.Combo;
            GetDisplayScore = scoreProcessor.GetDisplayScore;
        }

        public GameplayLeaderboardScore(IUser user, SpectatorScoreProcessor scoreProcessor, bool tracked)
        {
            User = user;
            Tracked = tracked;
            TotalScore.BindTarget = scoreProcessor.TotalScore;
            Accuracy.BindTarget = scoreProcessor.Accuracy;
            Combo.BindTarget = scoreProcessor.Combo;
            GetDisplayScore = scoreProcessor.GetDisplayScore;
        }

        public GameplayLeaderboardScore(ScoreInfo scoreInfo, bool tracked)
        {
            User = scoreInfo.User;
            Tracked = tracked;
            TotalScore.Value = scoreInfo.TotalScore;
            Accuracy.Value = scoreInfo.Accuracy;
            Combo.Value = scoreInfo.Combo;
            DisplayOrder.Value = scoreInfo.OnlineID > 0 ? scoreInfo.OnlineID : scoreInfo.Date.ToUnixTimeSeconds();
            GetDisplayScore = scoreInfo.GetDisplayScore;
        }

        /// <remarks>
        /// Used for testing.
        /// </remarks>
        internal GameplayLeaderboardScore(IUser user, bool tracked, Bindable<long> displayScore)
        {
            User = user;
            Tracked = tracked;
            TotalScore.BindTarget = displayScore;
            GetDisplayScore = _ => displayScore.Value;
        }
    }
}
