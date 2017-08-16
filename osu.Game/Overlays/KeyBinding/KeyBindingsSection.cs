// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Game.Input;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets;
using OpenTK;

namespace osu.Game.Overlays.KeyBinding
{
    public abstract class KeyBindingsSection : SettingsSection
    {
        protected IEnumerable<Framework.Input.Bindings.KeyBinding> Defaults;

        protected RulesetInfo Ruleset;

        protected KeyBindingsSection()
        {
            FlowContent.Spacing = new Vector2(0, 1);
        }

        [BackgroundDependencyLoader]
        private void load(KeyBindingStore store)
        {
            var enumType = Defaults?.FirstOrDefault()?.Action?.GetType();

            if (enumType == null) return;

            // for now let's just assume a variant of zero.
            // this will need to be implemented in a better way in the future.
            int? variant = null;
            if (Ruleset != null)
                variant = 0;

            var bindings = store.Query(Ruleset?.ID, variant);

            foreach (Enum v in Enum.GetValues(enumType))
                // one row per valid action.
                Add(new KeyBindingRow(v, bindings.Where(b => b.Action.Equals((int)(object)v))));
        }
    }
}