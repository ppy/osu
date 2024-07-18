// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Platform;
using osu.Game.Input;
using osu.Game.Input.Bindings;

namespace osu.Game.Graphics.UserInterface
{
    public struct Hotkey
    {
        public KeyCombination[]? KeyCombinations { get; }
        public GlobalAction? GlobalAction { get; }
        public PlatformAction? PlatformAction { get; }

        public Hotkey(params KeyCombination[] keyCombinations)
        {
            KeyCombinations = keyCombinations;
        }

        public Hotkey(GlobalAction globalAction)
        {
            GlobalAction = globalAction;
        }

        public Hotkey(PlatformAction platformAction)
        {
            PlatformAction = platformAction;
        }

        public IEnumerable<string> ResolveKeyCombination(ReadableKeyCombinationProvider keyCombinationProvider, RealmKeyBindingStore keyBindingStore, GameHost gameHost)
        {
            if (KeyCombinations != null)
                return KeyCombinations.Select(keyCombinationProvider.GetReadableString);

            if (GlobalAction != null)
                return keyBindingStore.GetReadableKeyCombinationsFor(GlobalAction.Value);

            if (PlatformAction != null)
            {
                var action = PlatformAction.Value;
                var bindings = gameHost.PlatformKeyBindings.Where(kb => (PlatformAction)kb.Action == action);
                return bindings.Select(b => keyCombinationProvider.GetReadableString(b.KeyCombination));
            }

            return Enumerable.Empty<string>();
        }
    }
}
