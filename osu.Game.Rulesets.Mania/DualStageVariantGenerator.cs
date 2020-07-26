// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Input.Bindings;

namespace osu.Game.Rulesets.Mania
{
    public class DualStageVariantGenerator
    {
        private readonly int singleStageVariant;
        private readonly InputKey[] stage1LeftKeys;
        private readonly InputKey[] stage1RightKeys;
        private readonly InputKey[] stage2LeftKeys;
        private readonly InputKey[] stage2RightKeys;

        public DualStageVariantGenerator(int singleStageVariant)
        {
            this.singleStageVariant = singleStageVariant;

            // 10K is special because it expands towards the centre of the keyboard (VM/BN), rather than towards the edges of the keyboard.
            if (singleStageVariant == 10)
            {
                stage1LeftKeys = new[] { InputKey.Q, InputKey.W, InputKey.E, InputKey.R, InputKey.V };
                stage1RightKeys = new[] { InputKey.M, InputKey.I, InputKey.O, InputKey.P, InputKey.BracketLeft };

                stage2LeftKeys = new[] { InputKey.S, InputKey.D, InputKey.F, InputKey.G, InputKey.B };
                stage2RightKeys = new[] { InputKey.N, InputKey.J, InputKey.K, InputKey.L, InputKey.Semicolon };
            }
            else
            {
                stage1LeftKeys = new[] { InputKey.Q, InputKey.W, InputKey.E, InputKey.R };
                stage1RightKeys = new[] { InputKey.I, InputKey.O, InputKey.P, InputKey.BracketLeft };

                stage2LeftKeys = new[] { InputKey.S, InputKey.D, InputKey.F, InputKey.G };
                stage2RightKeys = new[] { InputKey.J, InputKey.K, InputKey.L, InputKey.Semicolon };
            }
        }

        public IEnumerable<KeyBinding> GenerateMappings()
        {
            var stage1Bindings = new VariantMappingGenerator
            {
                LeftKeys = stage1LeftKeys,
                RightKeys = stage1RightKeys,
                SpecialKey = InputKey.V,
                SpecialAction = ManiaAction.Special1,
                NormalActionStart = ManiaAction.Key1
            }.GenerateKeyBindingsFor(singleStageVariant, out var nextNormal);

            var stage2Bindings = new VariantMappingGenerator
            {
                LeftKeys = stage2LeftKeys,
                RightKeys = stage2RightKeys,
                SpecialKey = InputKey.B,
                SpecialAction = ManiaAction.Special2,
                NormalActionStart = nextNormal
            }.GenerateKeyBindingsFor(singleStageVariant, out _);

            return stage1Bindings.Concat(stage2Bindings);
        }
    }
}
