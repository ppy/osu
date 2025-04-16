// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Scoring;
using osu.Game.Users;

namespace osu.Game.Screens.Play.HUD
{
    /// <summary>
    /// Represents a score shown on a gameplay leaderboard.
    /// The score is expected to update itself as gameplay progresses.
    /// </summary>
    public interface IGameplayLeaderboardScore
    {
        /// <summary>
        /// The user playing.
        /// </summary>
        IUser User { get; }

        /// <summary>
        /// Whether the score is being tracked.
        /// Generally understood as true when this score is the score of the local user currently playing.
        /// </summary>
        bool Tracked { get; }

        /// <summary>
        /// The current total of the score.
        /// </summary>
        BindableLong TotalScore { get; }

        /// <summary>
        /// The current accuracy of the score.
        /// </summary>
        BindableDouble Accuracy { get; }

        /// <summary>
        /// The current combo of the score.
        /// </summary>
        BindableInt Combo { get; }

        /// <summary>
        /// Whether the user playing has quit.
        /// </summary>
        BindableBool HasQuit { get; }

        /// <summary>
        /// An optional value to guarantee stable ordering.
        /// Lower numbers will appear higher in cases of <see cref="TotalScore"/> ties.
        /// </summary>
        Bindable<long> DisplayOrder { get; }

        /// <summary>
        /// A custom function which handles converting a score to a display score using a provide <see cref="ScoringMode"/>.
        /// </summary>
        /// <remarks>
        /// If no function is provided, <see cref="TotalScore"/> will be used verbatim.</remarks>
        Func<ScoringMode, long> GetDisplayScore { get; set; }

        /// <summary>
        /// The colour of the team that the user playing is on, if any.
        /// </summary>
        Colour4? TeamColour { get; }
    }
}
