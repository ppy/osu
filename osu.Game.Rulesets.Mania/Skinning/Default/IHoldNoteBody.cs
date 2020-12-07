// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Mania.Skinning.Default
{
    /// <summary>
    /// Interface for mania hold note bodies.
    /// </summary>
    public interface IHoldNoteBody
    {
        /// <summary>
        /// Recycles the contents of this <see cref="IHoldNoteBody"/> to free used resources.
        /// </summary>
        void Recycle();
    }
}
