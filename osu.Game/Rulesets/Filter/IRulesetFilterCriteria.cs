// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Screens.Select;
using osu.Game.Screens.Select.Filter;

namespace osu.Game.Rulesets.Filter
{
    /// <summary>
    /// Allows for extending the beatmap filtering capabilities of song select (as implemented in <see cref="FilterCriteria"/>)
    /// with ruleset-specific criteria.
    /// </summary>
    public interface IRulesetFilterCriteria
    {
        /// <summary>
        /// Checks whether the supplied <paramref name="beatmapInfo"/> satisfies ruleset-specific custom criteria,
        /// in addition to the ones mandated by song select.
        /// </summary>
        /// <param name="beatmapInfo">The beatmap to test the criteria against.</param>
        /// <param name="criteria">The filter criteria.</param>
        /// <returns>
        /// <c>true</c> if the beatmap matches the ruleset-specific custom filtering criteria,
        /// <c>false</c> otherwise.
        /// </returns>
        bool Matches(BeatmapInfo beatmapInfo, FilterCriteria criteria);

        /// <summary>
        /// Attempts to parse a single custom keyword criterion, given by the user via the song select search box.
        /// The format of the criterion is:
        /// <code>
        /// {key}{op}{value}
        /// </code>
        /// </summary>
        /// <remarks>
        /// <para>
        /// For adding optional string criteria, <see cref="FilterCriteria.OptionalTextFilter"/> can be used for matching,
        /// along with <see cref="FilterQueryParser.TryUpdateCriteriaText"/> for parsing.
        /// </para>
        /// <para>
        /// For adding numerical-type range criteria, <see cref="FilterCriteria.OptionalRange{T}"/> can be used for matching,
        /// along with <see cref="FilterQueryParser.TryUpdateCriteriaRange{T}(ref osu.Game.Screens.Select.FilterCriteria.OptionalRange{T},osu.Game.Screens.Select.Filter.Operator,string,FilterQueryParser.TryParseFunction{T})"/>
        /// and <see cref="float"/>- and <see cref="double"/>-typed overloads for parsing.
        /// </para>
        /// </remarks>
        /// <param name="key">The key (name) of the criterion.</param>
        /// <param name="op">The operator in the criterion.</param>
        /// <param name="value">The value of the criterion.</param>
        /// <returns>
        /// <c>true</c> if the keyword criterion is valid, <c>false</c> if it has been ignored.
        /// Valid criteria are stripped from <see cref="FilterCriteria.SearchText"/>,
        /// while ignored criteria are included in <see cref="FilterCriteria.SearchText"/>.
        /// </returns>
        bool TryParseCustomKeywordCriteria(string key, Operator op, string value);
    }
}
