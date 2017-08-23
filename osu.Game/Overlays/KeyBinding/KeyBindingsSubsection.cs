// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Game.Input;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets;
using OpenTK;

namespace osu.Game.Overlays.KeyBinding
{
    public abstract class KeyBindingsSubsection : SettingsSubsection
    {
        protected IEnumerable<Framework.Input.Bindings.KeyBinding> Defaults;

        protected RulesetInfo Ruleset;

        private readonly int variant;

        protected KeyBindingsSubsection(int variant)
        {
            this.variant = variant;

            FlowContent.Spacing = new Vector2(0, 1);
        }

        [BackgroundDependencyLoader]
        private void load(KeyBindingStore store)
        {
            var bindings = store.Query(Ruleset?.ID, variant);

            foreach (var defaultBinding in Defaults)
            {
                // one row per valid action.
                Add(new KeyBindingRow(defaultBinding.Action, bindings.Where(b => b.Action.Equals((int)defaultBinding.Action)))
                {
                    AllowMainMouseButtons = Ruleset != null
                });
            }
        }
    }
}