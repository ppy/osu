// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections;
using System.Linq;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Game.Input.Bindings;

namespace osu.Game.Graphics.UserInterface
{
    public readonly record struct Hotkey
    {
        public KeyCombination[]? KeyCombinations { get; init; }
        public GlobalAction? GlobalAction { get; init; }
        public PlatformAction? PlatformAction { get; init; }
        public (string ruleset, int variant, int action)? RulesetAction { get; init; }

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

        public Hotkey(string ruleset, int variant, int action)
        {
            RulesetAction = (ruleset, variant, action);
        }

        public bool Equals(Hotkey other)
        {
            if (KeyCombinations == null && other.KeyCombinations != null)
                return false;

            if (KeyCombinations != null && other.KeyCombinations == null)
                return false;

            bool result = (KeyCombinations == null && other.KeyCombinations == null) || KeyCombinations!.SequenceEqual(other.KeyCombinations!);
            result &= GlobalAction == other.GlobalAction;
            result &= PlatformAction == other.PlatformAction;
            result &= RulesetAction == other.RulesetAction;
            return result;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(StructuralComparisons.StructuralEqualityComparer.GetHashCode(KeyCombinations ?? []), GlobalAction, PlatformAction);
        }
    }
}
