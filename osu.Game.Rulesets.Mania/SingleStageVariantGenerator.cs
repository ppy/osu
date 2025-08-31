// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Input.Bindings;

namespace osu.Game.Rulesets.Mania
{
    public class SingleStageVariantGenerator
    {
        private readonly int variant;
        private readonly InputKey[] leftKeys;
        private readonly InputKey[] secondaryLeftKeys;
        private readonly InputKey[] rightKeys;
        private readonly InputKey[] secondaryRightKeys;

        public SingleStageVariantGenerator(int variant)
        {
            this.variant = variant;

            // 10K is special because it expands towards the centre of the keyboard (V/N), rather than towards the edges of the keyboard.
            if (variant == 10)
            {
                leftKeys = new[] { InputKey.A, InputKey.S, InputKey.D, InputKey.F, InputKey.V };
                rightKeys = new[] { InputKey.N, InputKey.J, InputKey.K, InputKey.L, InputKey.Semicolon };
                secondaryLeftKeys = new[] { InputKey.Q, InputKey.W, InputKey.E, InputKey.R, InputKey.B };
                secondaryRightKeys = new[] { InputKey.M, InputKey.I, InputKey.O, InputKey.P, InputKey.BracketLeft };
            }
            else
            {
                leftKeys = new[] { InputKey.A, InputKey.S, InputKey.D, InputKey.F };
                rightKeys = new[] { InputKey.J, InputKey.K, InputKey.L, InputKey.Semicolon };
                secondaryLeftKeys = new[] { InputKey.Q, InputKey.W, InputKey.E, InputKey.R };
                secondaryRightKeys = new[] { InputKey.I, InputKey.O, InputKey.P, InputKey.BracketLeft };
            }
        }

        public IEnumerable<KeyBinding> GenerateMappings() => new VariantMappingGenerator
        {
            LeftKeys = leftKeys,
            SecondaryLeftKeys = secondaryLeftKeys,
            RightKeys = rightKeys,
            SecondaryRightKeys = secondaryRightKeys,
            SpecialKey = InputKey.Space,
            SecondarySpecialKey = InputKey.Enter
        }.GenerateKeyBindingsFor(variant);
    }
}
