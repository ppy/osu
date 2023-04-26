// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Input.Events;

namespace osu.Game.Overlays.Mods.Input
{
    /// <summary>
    /// Encapsulates strategies of handling mod hotkeys on the <see cref="ModSelectOverlay"/>.
    /// </summary>
    public interface IModHotkeyHandler
    {
        /// <summary>
        /// Attempt to handle the supplied <see cref="KeyDownEvent"/> as a selection of one of the mods in <paramref name="availableMods"/>.
        /// </summary>
        /// <param name="e">The event representing the user's keypress.</param>
        /// <param name="availableMods">The list of currently available mods.</param>
        /// <returns>Whether the supplied event was handled as a mod selection/deselection.</returns>
        bool HandleHotkeyPressed(KeyDownEvent e, IEnumerable<ModState> availableMods);
    }
}
