// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Input.Bindings;

namespace osu.Game.Rulesets.Mania
{
    public class VariantMappingGenerator
    {
        /// <summary>
        /// All the <see cref="InputKey"/>s available to the left hand.
        /// </summary>
        public InputKey[] LeftKeys;

        /// <summary>
        /// All the <see cref="InputKey"/>s available to the right hand.
        /// </summary>
        public InputKey[] RightKeys;

        /// <summary>
        /// The <see cref="InputKey"/> for the special key.
        /// </summary>
        public InputKey SpecialKey;

        /// <summary>
        /// The <see cref="ManiaAction"/> at which the normal columns should begin.
        /// </summary>
        public ManiaAction NormalActionStart;

        /// <summary>
        /// The <see cref="ManiaAction"/> for the special column.
        /// </summary>
        public ManiaAction SpecialAction;

        /// <summary>
        /// Generates a list of <see cref="KeyBinding"/>s for a specific number of columns.
        /// </summary>
        /// <param name="columns">The number of columns that need to be bound.</param>
        /// <param name="nextNormalAction">The next <see cref="ManiaAction"/> to use for normal columns.</param>
        /// <returns>The keybindings.</returns>
        public IEnumerable<KeyBinding> GenerateKeyBindingsFor(int columns, out ManiaAction nextNormalAction)
        {
            ManiaAction currentNormalAction = NormalActionStart;

            var bindings = new List<KeyBinding>();

            for (int i = LeftKeys.Length - columns / 2; i < LeftKeys.Length; i++)
                bindings.Add(new KeyBinding(LeftKeys[i], currentNormalAction++));

            if (columns % 2 == 1)
                bindings.Add(new KeyBinding(SpecialKey, SpecialAction));

            for (int i = 0; i < columns / 2; i++)
                bindings.Add(new KeyBinding(RightKeys[i], currentNormalAction++));

            nextNormalAction = currentNormalAction;
            return bindings;
        }
    }
}
