// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;

namespace osu.Game.Screens.Play.HUD
{
    /// <summary>
    /// An interface providing a set of methods to update a combo counter.
    /// </summary>
    public interface IComboCounter : IDrawable
    {
        /// <summary>
        /// The current combo to be displayed.
        /// </summary>
        Bindable<int> Current { get; }
    }
}
