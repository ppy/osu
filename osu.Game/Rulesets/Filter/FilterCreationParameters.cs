// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Game.Collections;
using osu.Game.Screens.Select.Filter;

namespace osu.Game.Rulesets.Filter
{
    /// <summary>
    /// Structure used to pass data required to create a <see cref="FilterCriteria"/> instance
    /// for use in the song selection screen.
    /// </summary>
    public class FilterCreationParameters
    {
        /// <summary>
        /// The textual query, entered by the user in the song select search box.
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        /// The group mode to use.
        /// </summary>
        public GroupMode GroupMode { get; set; }

        /// <summary>
        /// The sort mode to use.
        /// </summary>
        public SortMode SortMode { get; set; }

        /// <summary>
        /// Whether converted beatmaps are allowed to be included in the filtering results.
        /// </summary>
        public bool AllowConvertedBeatmaps { get; set; }

        /// <summary>
        /// The currently-selected collection.
        /// </summary>
        [CanBeNull]
        public BeatmapCollection Collection { get; set; }

        /// <summary>
        /// The allowable star difficulty range, as set by the user in the game settings.
        /// </summary>
        public FilterCriteria.OptionalRange<double> UserStarDifficulty { get; set; } = new FilterCriteria.OptionalRange<double>
        {
            IsLowerInclusive = true,
            IsUpperInclusive = true
        };
    }
}
