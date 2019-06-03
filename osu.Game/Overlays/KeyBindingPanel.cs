// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Input.Bindings;
using osu.Game.Overlays.KeyBinding;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets;

namespace osu.Game.Overlays
{
    public class KeyBindingPanel : SettingsSubPanel
    {
        protected override Drawable CreateHeader() => new SettingsHeader("key configuration", "Customise your keys!");

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(RulesetStore rulesets, GlobalActionContainer global)
        {
            AddSection(new GlobalKeyBindingsSection(global));

            foreach (var ruleset in rulesets.AvailableRulesets)
                AddSection(new RulesetBindingsSection(ruleset));
        }
    }
}
