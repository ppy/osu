// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// An interface for a mod which can temporarily override screen settings.
    /// </summary>
    public interface IApplicableToScreen : IApplicableMod
    {
        /// <summary>
        /// Whether to enable image, video and storyboard dimming
        /// </summary>
        bool EnableDim { get; }

        /// <summary>
        /// Weather to force the video (if present)
        /// </summary>
        bool ForceVideo { get; }

        /// <summary>
        /// Weather to force the storyboard (if present)
        /// </summary>
        bool ForceStoryboard { get; }
    }
}
