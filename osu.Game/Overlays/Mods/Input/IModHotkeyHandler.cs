// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osuTK.Input;

namespace osu.Game.Overlays.Mods.Input
{
    /// <summary>
    /// Encapsulates strategies of handling mod hotkeys on the <see cref="ModSelectOverlay"/>.
    /// </summary>
    public interface IModHotkeyHandler
    {
        /// <summary>
        /// Attempt to handle a press of the supplied <paramref name="hotkey"/> as a selection of one of the mods in <paramref name="availableMods"/>.
        /// </summary>
        /// <param name="hotkey">The key that was pressed by the user.</param>
        /// <param name="availableMods">The list of currently available mods.</param>
        /// <returns>Whether the <paramref name="hotkey"/> was handled as a mod selection/deselection.</returns>
        bool HandleHotkeyPressed(Key hotkey, IEnumerable<ModState> availableMods);
    }
}
