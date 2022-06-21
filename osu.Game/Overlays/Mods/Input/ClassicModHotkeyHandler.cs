// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osuTK.Input;

namespace osu.Game.Overlays.Mods.Input
{
    /// <summary>
    /// Uses bindings from stable 1:1.
    /// </summary>
    public class ClassicModHotkeyHandler : IModHotkeyHandler
    {
        public bool HandleHotkeyPressed(Key hotkey, IEnumerable<ModState> availableMods) => false; // TODO
    }
}
