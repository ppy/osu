// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Objects.Types
{
    /// <summary>
    /// A HitObject which has its hitsound at a specific time along its duration. Will be used for editor timeline display.
    /// </summary>
    public interface IHasEditorSamplePosition
    {
        /// <summary>
        /// The current position of hitsound samples on the timeline.
        /// Valued in range [0;1] so 0 is at the start of the object, 1 is at the end of the object.
        /// </summary>
        float EditorSamplePosition { get; }
    }
}
