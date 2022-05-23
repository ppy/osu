// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Screens;
using osu.Game.Screens.Menu;

namespace osu.Game.Screens
{
    /// <summary>
    /// Manages a global screen stack to allow nested components a guarantee of where work is executed.
    /// </summary>
    [Cached]
    public interface IPerformFromScreenRunner
    {
        /// <summary>
        /// Perform an action only after returning to a specific screen as indicated by <paramref name="validScreens"/>.
        /// Eagerly tries to exit the current screen until it succeeds.
        /// </summary>
        /// <param name="action">The action to perform once we are in the correct state.</param>
        /// <param name="validScreens">An optional collection of valid screen types. If any of these screens are already current we can perform the action immediately, else the first valid parent will be made current before performing the action. <see cref="MainMenu"/> is used if not specified.</param>
        void PerformFromScreen(Action<IScreen> action, IEnumerable<Type> validScreens = null);
    }
}
