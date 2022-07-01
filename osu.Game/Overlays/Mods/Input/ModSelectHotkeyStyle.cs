// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Mods;

namespace osu.Game.Overlays.Mods.Input
{
    /// <summary>
    /// The style of hotkey handling to use on the mod select screen.
    /// </summary>
    public enum ModSelectHotkeyStyle
    {
        /// <summary>
        /// Each letter row on the keyboard controls one of the three first <see cref="ModColumn"/>s.
        /// Individual letters in a row trigger the mods in a sequential fashion.
        /// Uses <see cref="SequentialModHotkeyHandler"/>.
        /// </summary>
        Sequential,

        /// <summary>
        /// Matches keybindings from stable 1:1.
        /// One keybinding can toggle between what used to be <see cref="MultiMod"/>s on stable,
        /// and some mods in a column may not have any hotkeys at all.
        /// </summary>
        Classic
    }
}
