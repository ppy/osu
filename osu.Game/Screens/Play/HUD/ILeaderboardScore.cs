// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

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
        /// A function providing a display score. If a custom function is not provided, this defaults to using <see cref="TotalScore"/>.
        /// </summary>
        Func<ScoringMode, long> GetDisplayScore { set; }
    }
}
