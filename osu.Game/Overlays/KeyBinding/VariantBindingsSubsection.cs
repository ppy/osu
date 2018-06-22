// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets;

namespace osu.Game.Overlays.KeyBinding
{
    public class VariantBindingsSubsection : KeyBindingsSubsection
    {
        protected override string Header => variantName;
        private readonly string variantName;

        public VariantBindingsSubsection(RulesetInfo ruleset, int variant)
            : base(variant)
        {
            Ruleset = ruleset;

            var rulesetInstance = ruleset.CreateInstance();

            variantName = rulesetInstance.GetVariantName(variant);
            Defaults = rulesetInstance.GetDefaultKeyBindings(variant);
        }
    }
}
