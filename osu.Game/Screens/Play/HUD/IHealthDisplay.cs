// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Judgements;

namespace osu.Game.Screens.Play.HUD
{
    /// <summary>
    /// An interface providing a set of methods to update a health display.
    /// </summary>
    public interface IHealthDisplay : IDrawable
    {
        /// <summary>
        /// The current health to be displayed.
        /// </summary>
        Bindable<double> Current { get; }

        /// <summary>
        /// Flash the display for a specified result type.
        /// </summary>
        /// <param name="result">The result type.</param>
        void Flash(JudgementResult result);
    }
}
