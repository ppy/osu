// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Edit.Checks.Components
{
    /// <summary>
    /// The category of an issue.
    /// </summary>
    public enum CheckCategory
    {
        /// <summary>
        /// Anything to do with control points.
        /// </summary>
        Timing,

        /// <summary>
        /// Anything to do with artist, title, creator, etc.
        /// </summary>
        Metadata,

        /// <summary>
        /// Anything to do with non-audio files, e.g. background, skin, sprites, and video.
        /// </summary>
        Resources,

        /// <summary>
        /// Anything to do with audio files, e.g. song and hitsounds.
        /// </summary>
        Audio,

        /// <summary>
        /// Anything to do with files that don't fit into the above, e.g. unused, osu, or osb.
        /// </summary>
        Files,

        /// <summary>
        /// Anything to do with hitobjects unrelated to spread.
        /// </summary>
        Compose,

        /// <summary>
        /// Anything to do with difficulty levels or their progression.
        /// </summary>
        Spread,

        /// <summary>
        /// Anything to do with variables like CS, OD, AR, HP, and global SV.
        /// </summary>
        Settings,

        /// <summary>
        /// Anything to do with hitobject feedback.
        /// </summary>
        HitObjects,

        /// <summary>
        /// Anything to do with storyboarding, breaks, video offset, etc.
        /// </summary>
        Events
    }
}
