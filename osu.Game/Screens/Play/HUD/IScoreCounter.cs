// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;

namespace osu.Game.Screens.Play.HUD
{
    /// <summary>
    /// An interface providing a set of methods to update a score counter.
    /// </summary>
    public interface IScoreCounter : IDrawable
    {
        /// <summary>
        /// The current score to be displayed.
        /// </summary>
        Bindable<double> Current { get; }

        /// <summary>
        /// The number of digits required to display most sane scores.
        /// This may be exceeded in very rare cases, but is useful to pad or space the display to avoid it jumping around.
        /// </summary>
        Bindable<int> RequiredDisplayDigits { get; }
    }
}
