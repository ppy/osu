// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Scoring
{
    /// <summary>
    /// Represents a score with replay backing.
    /// Such replays expose additional data for display.
    /// </summary>
    public interface IScoreWithReplayData : IScoreInfo
    {
        /// <summary>
        /// A list of <see cref="HitEvent"/>s which occurred during the play.
        /// </summary>
        IReadOnlyList<HitEvent> HitEvents { get; }

        /// <summary>
        /// A list of <see cref="HealthValue"/>s registered during the play.
        /// </summary>
        IReadOnlyList<HealthValue> HealthGraph { get; }
    }
}
