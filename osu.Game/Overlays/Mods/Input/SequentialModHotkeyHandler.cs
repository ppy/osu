// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Mods;
using osuTK.Input;

namespace osu.Game.Overlays.Mods.Input
{
    /// <summary>
    /// This implementation of <see cref="IModHotkeyHandler"/> receives a sequence of <see cref="Key"/>s,
    /// and maps the sequence of keys onto the items it is provided in <see cref="HandleHotkeyPressed"/>.
    /// In this case, particular mods are not bound to particular keys, the hotkeys are a byproduct of mod ordering.
    /// </summary>
    public class SequentialModHotkeyHandler : IModHotkeyHandler
    {
        public static SequentialModHotkeyHandler Create(ModType modType)
        {
            switch (modType)
            {
                case ModType.DifficultyReduction:
                    return new SequentialModHotkeyHandler(new[] { Key.Q, Key.W, Key.E, Key.R, Key.T, Key.Y, Key.U, Key.I, Key.O, Key.P });

                case ModType.DifficultyIncrease:
                    return new SequentialModHotkeyHandler(new[] { Key.A, Key.S, Key.D, Key.F, Key.G, Key.H, Key.J, Key.K, Key.L });

                case ModType.Automation:
                    return new SequentialModHotkeyHandler(new[] { Key.Z, Key.X, Key.C, Key.V, Key.B, Key.N, Key.M });

                default:
                    throw new ArgumentOutOfRangeException(nameof(modType), modType, $"Cannot create {nameof(SequentialModHotkeyHandler)} for provided mod type");
            }
        }

        private readonly Key[] toggleKeys;

        private SequentialModHotkeyHandler(Key[] keys)
        {
            toggleKeys = keys;
        }

        public bool HandleHotkeyPressed(KeyDownEvent e, IEnumerable<ModState> availableMods)
        {
            int index = Array.IndexOf(toggleKeys, e.Key);
            if (index < 0)
                return false;

            var modState = availableMods.Where(modState => modState.Visible).ElementAtOrDefault(index);
            if (modState == null)
                return false;

            modState.Active.Toggle();
            return true;
        }
    }
}
