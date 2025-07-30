// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Beatmaps;

namespace osu.Game.Rulesets.Difficulty
{
    /// <summary>
    /// A <see cref="RulesetBeatmapAttribute"/> is like a single property from <see cref="BeatmapDifficulty"/>,
    /// but adjusted for display in the context of a specific ruleset.
    /// The reason why this record exists is that rulesets use <see cref="BeatmapDifficulty"/> in different ways.
    /// Some rulesets completely ignore some fields from <see cref="BeatmapDifficulty"/>,
    /// some reuse fields in weird ways (like mania reusing <see cref="BeatmapDifficulty.CircleSize"/> to mean key count),
    /// some want to provide specific extended information for a <see cref="BeatmapDifficulty"/> field
    /// or adjust the "effective display" in different ways.
    /// </summary>
    /// <param name="Label">The long label for this beatmap attribute.</param>
    /// <param name="Acronym">A two-letter acronym for this beatmap attribute.</param>
    /// <param name="Value">The value of this attribute before application of mods.</param>
    /// <param name="AdjustedValue">The "effective" value of this attribute after application of mods.</param>
    /// <param name="MinValue">The lowest allowable value of this attribute.</param>
    /// <param name="MaxValue">The highest allowable value of this attribute.</param>
    public record RulesetBeatmapAttribute(
        LocalisableString Label,
        string Acronym,
        float Value,
        float AdjustedValue,
        float MinValue,
        float MaxValue);
}
