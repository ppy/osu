// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Scoring;
using osu.Game.Users;

namespace osu.Game.Scoring
{
    /// <summary>
    /// Represents an arbitrary score.
    /// This score can originate from an arbitrary source (local, online, multiplayer...)
    /// </summary>
    public interface IScoreInfo : IHasOnlineID<long>
    {
        /// <summary>
        /// The user who set this score.
        /// </summary>
        IUser User { get; }

        /// <summary>
        /// The standardised total score.
        /// </summary>
        long TotalScore { get; }

        /// <summary>
        /// The maximum combo length achieved during this score.
        /// </summary>
        int MaxCombo { get; }

        /// <summary>
        /// The accuracy percentage of this score from the range [0, 1].
        /// </summary>
        double Accuracy { get; }

        /// <summary>
        /// The online ID of this score in "legacy" tables.
        /// Will be -1 if this score does not possess such an ID.
        /// </summary>
        long LegacyOnlineID { get; }

        /// <summary>
        /// The date on which this score was set.
        /// </summary>
        DateTimeOffset Date { get; }

        /// <summary>
        /// The number of performance points awarded to this score.
        /// </summary>
        double? PP { get; }

        /// <summary>
        /// The beatmap which this score was set on.
        /// </summary>
        IBeatmapInfo? Beatmap { get; }

        /// <summary>
        /// The ruleset which this score was set on.
        /// </summary>
        IRulesetInfo Ruleset { get; }

        /// <summary>
        /// The letter rank (grade) awarded to this score.
        /// </summary>
        ScoreRank Rank { get; }

        /// <summary>
        /// The mods used to set this score.
        /// </summary>
        IEnumerable<IConfiguredMod> Mods { get; }

        /// <summary>
        /// The hit statistics achieved in this score.
        /// </summary>
        IReadOnlyDictionary<HitResult, int> Statistics { get; }

        /// <summary>
        /// The maximum possible statistics achievable given the <see cref="Beatmap"/>, <see cref="Ruleset"/>, and <see cref="Mods"/>.
        /// </summary>
        IReadOnlyDictionary<HitResult, int> MaximumStatistics { get; }
    }
}
