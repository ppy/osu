// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Screens.Play.HUD
{
    public interface ILeaderboardScore
    {
        BindableLong TotalScore { get; }
        BindableDouble Accuracy { get; }
        BindableInt Combo { get; }

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
        Func<ScoringMode, long> GetDisplayScore { set; }
    }
}
