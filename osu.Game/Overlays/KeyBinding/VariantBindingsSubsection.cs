// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets;

namespace osu.Game.Overlays.KeyBinding
{
    public class VariantBindingsSubsection : KeyBindingsSubsection
    {
        protected override string Header => variant > 0 ? $"Variant: {variant}" : string.Empty;

        private readonly int variant;

        public VariantBindingsSubsection(RulesetInfo ruleset, int variant)
            : base(variant)
        {
            this.variant = variant;

            Ruleset = ruleset;
            Defaults = ruleset.CreateInstance().GetDefaultKeyBindings(variant);
        }
    }
}