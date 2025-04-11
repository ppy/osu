// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Screens.Select;

namespace osu.Game.Rulesets.Filter
{
    /// <summary>
    /// Allows for changing which beatmap rulesets are displayed in song select (as implemented in <see cref="FilterCriteria"/>)
    /// with ruleset-specific criteria.
    /// </summary>
    public interface IRulesetConvertSupport
    {
        /// <summary>
        /// Checks whether maps from the supplied <paramref name="ruleset"/> may be played with this ruleset with or
        /// without beatmap conversion enabled.
        /// </summary>
        /// <param name="ruleset">The foreign ruleset to check if it may be played.</param>
        /// <param name="conversionEnabled">Indicates if the player wants converts or not.</param>
        /// <returns>
        /// <c>true</c> if the beatmap can be played and should be shown in the beatmap list,
        /// <c>false</c> otherwise.
        /// </returns>
        bool CanBePlayed(RulesetInfo ruleset, bool conversionEnabled);
    }
}
