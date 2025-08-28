// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
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
    public class RulesetBeatmapAttribute
    {
        /// <summary>
        /// The long label for this beatmap attribute.
        /// </summary>
        public LocalisableString Label { get; }

        /// <summary>
        /// A two-letter acronym for this beatmap attribute.
        /// </summary>
        public string Acronym { get; }

        /// <summary>
        /// The value of this attribute before application of mods.
        /// </summary>
        public float OriginalValue { get; }

        /// <summary>
        /// The "effective" value of this attribute after application of mods.
        /// </summary>
        public float AdjustedValue { get; }

        /// <summary>
        /// The highest allowable value of this attribute.
        /// </summary>
        public float MaxValue { get; }

        /// <summary>
        /// An optional extended description of this attribute.
        /// </summary>
        public LocalisableString? Description { get; init; }

        /// <summary>
        /// Contains any and all additional metrics about how this attribute affects gameplay to show to the users.
        /// </summary>
        public AdditionalMetric[] AdditionalMetrics { get; init; } = [];

        public RulesetBeatmapAttribute(LocalisableString label, string acronym, float originalValue, float adjustedValue, float maxValue)
        {
            Label = label;
            Acronym = acronym;
            OriginalValue = originalValue;
            AdjustedValue = adjustedValue;
            MaxValue = maxValue;
        }

        public record AdditionalMetric(LocalisableString Name, LocalisableString Value, Colour4? Colour = null);
    }
}
