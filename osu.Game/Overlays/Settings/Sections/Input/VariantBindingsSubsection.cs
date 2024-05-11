// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Input.Bindings;
using osu.Framework.Localisation;
using osu.Game.Input.Bindings;
using osu.Game.Rulesets;
using Realms;

namespace osu.Game.Overlays.Settings.Sections.Input
{
    public partial class VariantBindingsSubsection : KeyBindingsSubsection
    {
        protected override bool AutoAdvanceTarget => true;

        protected override LocalisableString Header { get; }

        public RulesetInfo Ruleset { get; }
        private readonly int variant;

        public VariantBindingsSubsection(RulesetInfo ruleset, int variant)
        {
            Ruleset = ruleset;
            this.variant = variant;

            var rulesetInstance = ruleset.CreateInstance();

            Header = rulesetInstance.GetVariantName(variant);
            Defaults = rulesetInstance.GetDefaultKeyBindings(variant);
        }

        protected override IEnumerable<RealmKeyBinding> GetKeyBindings(Realm realm)
        {
            string rulesetName = Ruleset.ShortName;

            return realm.All<RealmKeyBinding>()
                        .Where(b => b.RulesetName == rulesetName && b.Variant == variant);
        }

        protected override KeyBindingRow CreateKeyBindingRow(object action, IEnumerable<KeyBinding> defaults)
            => new KeyBindingRow(action)
            {
                AllowMainMouseButtons = true,
                Defaults = defaults.Select(d => d.KeyCombination),
            };
    }
}
