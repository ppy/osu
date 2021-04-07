// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Screens.Edit.Verify
{
    public class CheckMetadata
    {
        /// <summary>
        /// The category of an issue.
        /// </summary>
        public enum CheckCategory
        {
            /// <summary> Anything to do with control points. </summary>
            Timing,

            /// <summary> Anything to do with artist, title, creator, etc. </summary>
            Metadata,

            /// <summary> Anything to do with non-audio files, e.g. background, skin, sprites, and video. </summary>
            Resources,

            /// <summary> Anything to do with audio files, e.g. song and hitsounds. </summary>
            Audio,

            /// <summary> Anything to do with files that don't fit into the above, e.g. unused, osu, or osb. </summary>
            Files,

            /// <summary> Anything to do with hitobjects unrelated to spread. </summary>
            Compose,

            /// <summary> Anything to do with difficulty levels or their progression. </summary>
            Spread,

            /// <summary> Anything to do with variables like CS, OD, AR, HP, and global SV. </summary>
            Settings,

            /// <summary> Anything to do with hitobject feedback. </summary>
            Hitsounds,

            /// <summary> Anything to do with storyboarding, breaks, video offset, etc. </summary>
            Events
        }

        /// <summary>
        /// The category this check belongs to. E.g. <see cref="CheckCategory.Metadata"/>,
        /// <see cref="CheckCategory.Timing"/>, or <see cref="CheckCategory.Compose"/>.
        /// </summary>
        public readonly CheckCategory Category;

        /// <summary>
        /// Describes the issue(s) that this check looks for. Keep this brief, such that
        /// it fits into "No {description}". E.g. "Offscreen objects" / "Too short sliders".
        /// </summary>
        public readonly string Description;

        public CheckMetadata(CheckCategory category, string description)
        {
            Category = category;
            Description = description;
        }
    }
}
