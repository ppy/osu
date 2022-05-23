// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Difficulty
{
    /// <summary>
    /// Data for displaying a performance attribute to user. Includes a display name for clarity.
    /// </summary>
    public class PerformanceDisplayAttribute
    {
        /// <summary>
        /// Name of the attribute property in <see cref="PerformanceAttributes"/>.
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// A custom display name for the attribute.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// The associated attribute value.
        /// </summary>
        public double Value { get; }

        public PerformanceDisplayAttribute(string propertyName, string displayName, double value)
        {
            PropertyName = propertyName;
            DisplayName = displayName;
            Value = value;
        }
    }
}
