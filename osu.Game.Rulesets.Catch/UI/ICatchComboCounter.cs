// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.UI
{
    /// <summary>
    /// An interface providing a set of methods to update the combo counter.
    /// </summary>
    public interface ICatchComboCounter : IDrawable
    {
        /// <summary>
        /// Updates the counter to animate a transition from the old combo value it had to the current provided one.
        /// </summary>
        /// <remarks>
        /// This is called regardless of whether the clock is rewinding.
        /// </remarks>
        /// <param name="combo">The new combo value.</param>
        /// <param name="hitObjectColour">The colour of the object if hit, null on miss.</param>
        void UpdateCombo(int combo, Color4? hitObjectColour = null);
    }
}
