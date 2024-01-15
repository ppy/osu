// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Mods;
using osuTK.Input;

namespace osu.Game.Overlays.Mods.Input
{
    /// <summary>
    /// Uses bindings from stable 1:1.
    /// </summary>
    public class ClassicModHotkeyHandler : IModHotkeyHandler
    {
        private static readonly Dictionary<Key, Type[]> mod_type_lookup = new Dictionary<Key, Type[]>
        {
            [Key.Q] = new[] { typeof(ModEasy) },
            [Key.W] = new[] { typeof(ModNoFail) },
            [Key.E] = new[] { typeof(ModHalfTime), typeof(ModDaycore) },
            [Key.A] = new[] { typeof(ModHardRock) },
            [Key.S] = new[] { typeof(ModSuddenDeath), typeof(ModPerfect) },
            [Key.D] = new[] { typeof(ModDoubleTime), typeof(ModNightcore) },
            [Key.F] = new[] { typeof(ModHidden) },
            [Key.G] = new[] { typeof(ModFlashlight) },
            [Key.Z] = new[] { typeof(ModRelax) },
            [Key.V] = new[] { typeof(ModAutoplay), typeof(ModCinema) }
        };

        private readonly bool allowIncompatibleSelection;

        public ClassicModHotkeyHandler(bool allowIncompatibleSelection)
        {
            this.allowIncompatibleSelection = allowIncompatibleSelection;
        }

        public bool HandleHotkeyPressed(KeyDownEvent e, IEnumerable<ModState> availableMods)
        {
            if (!mod_type_lookup.TryGetValue(e.Key, out var typesToMatch))
                return false;

            var matchingMods = availableMods.Where(modState => matches(modState, typesToMatch) && modState.Visible).ToArray();

            if (matchingMods.Length == 0)
                return false;

            if (matchingMods.Length == 1)
            {
                matchingMods.Single().Active.Toggle();
                return true;
            }

            if (allowIncompatibleSelection)
            {
                // easier path - multiple incompatible mods can be active at a time.
                // this is used in the free mod select overlay.
                // in this case, just toggle everything.
                bool anyActive = matchingMods.Any(mod => mod.Active.Value);
                foreach (var mod in matchingMods)
                    mod.Active.Value = !anyActive;
                return true;
            }

            // we now know there are multiple possible mods to handle, and only one of them can be active at a time.
            // let's make sure of this just in case.
            Debug.Assert(matchingMods.Count(modState => modState.Active.Value) <= 1);
            int currentSelectedIndex = Array.FindIndex(matchingMods, modState => modState.Active.Value);

            // `FindIndex` will return -1 if it doesn't find the item.
            // this is convenient in the forward direction, since if we add 1 then we'll end up at the first item,
            // but less so in the backwards direction.
            // for convenience, detect this situation and set the index to one index past the last item.
            // this makes it so that if we subtract 1 then we'll end up at the last item again.
            if (currentSelectedIndex < 0 && e.ShiftPressed)
                currentSelectedIndex = matchingMods.Length;

            int indexToSelect = e.ShiftPressed ? currentSelectedIndex - 1 : currentSelectedIndex + 1;

            // `currentSelectedIndex` and `indexToSelect` can both be equal to -1 or `matchingMods.Length`.
            // if the former is beyond array range, it means nothing was previously selected and so there's nothing to deselect.
            // if the latter is beyond array range, it means that either the previous selection was first and we're going backwards,
            // or it was last and we're going forwards.
            // in either case there is nothing to select.
            if (currentSelectedIndex >= 0 && currentSelectedIndex <= matchingMods.Length - 1)
                matchingMods[currentSelectedIndex].Active.Value = false;
            if (indexToSelect >= 0 && indexToSelect <= matchingMods.Length - 1)
                matchingMods[indexToSelect].Active.Value = true;

            return true;
        }

        private static bool matches(ModState modState, Type[] typesToMatch)
            => typesToMatch.Any(typeToMatch => typeToMatch.IsInstanceOfType(modState.Mod));
    }
}
