// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Input.Events;

namespace osu.Game.Overlays.Mods.Input
{
    public interface IPresetHotkeyHandler
    {
        /// <summary>
        /// Attempt to handle the supplied <see cref="KeyDownEvent"/> as a selection of one of the presets in <paramref name="availablePresets"/>.
        /// </summary>
        /// <param name="e">The event representing the user's keypress.</param>
        /// <param name="availablePresets">The list of currently available presets.</param>
        /// <returns>Whether the supplied event was handled as a preset selection/deselection.</returns>
        bool HandlePresetHotkeyPressed(KeyDownEvent e, IEnumerable<ModPresetPanel> availablePresets);
    }
}
