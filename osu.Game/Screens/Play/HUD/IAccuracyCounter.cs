// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;

namespace osu.Game.Screens.Play.HUD
{
    /// <summary>
    /// An interface providing a set of methods to update a accuracy counter.
    /// </summary>
    public interface IAccuracyCounter : IDrawable
    {
        /// <summary>
        /// The current accuracy to be displayed.
        /// </summary>
        Bindable<double> Current { get; }
    }
}
