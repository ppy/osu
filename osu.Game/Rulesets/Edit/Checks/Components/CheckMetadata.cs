// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Edit.Checks.Components
{
    public class CheckMetadata
    {
        /// <summary>
        /// The category this check belongs to. E.g. <see cref="CheckCategory.Metadata"/>, <see cref="CheckCategory.Timing"/>, or <see cref="CheckCategory.Compose"/>.
        /// </summary>
        public readonly CheckCategory Category;

        /// <summary>
        /// Describes the issue(s) that this check looks for. Keep this brief, such that it fits into "No {description}". E.g. "Offscreen objects" / "Too short sliders".
        /// </summary>
        public readonly string Description;

        public CheckMetadata(CheckCategory category, string description)
        {
            Category = category;
            Description = description;
        }
    }
}
