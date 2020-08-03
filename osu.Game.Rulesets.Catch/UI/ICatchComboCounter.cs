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
        /// Updates the counter to display the provided <paramref name="combo"/> as initial value.
        /// The value should be immediately displayed without any animation.
        /// </summary>
        /// <remarks>
        /// This is required for when instantiating a combo counter in middle of accumulating combo (via skin change).
        /// </remarks>
        /// <param name="combo">The combo value to be displayed as initial.</param>
        void DisplayInitialCombo(int combo);

        /// <summary>
        /// Updates the counter to animate a transition from the old combo value it had to the current provided one.
        /// </summary>
        /// <remarks>
        /// This is called regardless of whether the clock is rewinding.
        /// </remarks>
        /// <param name="combo">The new combo value.</param>
        /// <param name="hitObjectColour">The colour of the object if hit, null on miss.</param>
        void UpdateCombo(int combo, Color4? hitObjectColour);
    }
}
