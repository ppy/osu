// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Input.Events;

namespace osu.Game.Overlays.Mods.Input
{
    /// <summary>
    /// A no-op implementation of <see cref="IModHotkeyHandler"/>.
    /// Used when a column is not handling any hotkeys at all.
    /// </summary>
    public class NoopModHotkeyHandler : IModHotkeyHandler
    {
        public bool HandleHotkeyPressed(KeyDownEvent e, IEnumerable<ModState> availableMods) => false;
    }
}
