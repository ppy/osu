// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.UserInterface;
using osu.Framework.Bindables;
using osu.Framework.Allocation;

namespace osu.Game.Rulesets
{
    public abstract class RulesetSelector : TabControl<RulesetInfo>
    {
        protected RulesetStore AvaliableRulesets;
        protected readonly Bindable<RulesetInfo> GlobalRuleset = new Bindable<RulesetInfo>();

        protected override Dropdown<RulesetInfo> CreateDropdown() => null;

        /// <summary>
        /// Whether we want to change a global ruleset when local one is changed.
        /// </summary>
        protected virtual bool AllowGlobalRulesetChange => true;

        /// <summary>
        /// Whether we want to change a local ruleset when global one is changed.
        /// /// </summary>
        protected virtual bool AllowLocalRulesetChange => true;

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets, Bindable<RulesetInfo> parentRuleset)
        {
            AvaliableRulesets = rulesets;
            GlobalRuleset.BindTo(parentRuleset);

            foreach (var r in rulesets.AvailableRulesets)
            {
                AddItem(r);
            }

            GlobalRuleset.BindValueChanged(globalRulesetChanged);
            Current.BindValueChanged(OnLocalRulesetChanged);
        }

        private void globalRulesetChanged(ValueChangedEvent<RulesetInfo> e)
        {
            if (AllowLocalRulesetChange)
            {
                OnGlobalRulesetChanged(e);
            }
        }

        protected virtual void OnGlobalRulesetChanged(ValueChangedEvent<RulesetInfo> e)
        {
            Current.Value = e.NewValue;
        }

        protected virtual void OnLocalRulesetChanged(ValueChangedEvent<RulesetInfo> e)
        {
            if (!GlobalRuleset.Disabled && AllowGlobalRulesetChange)
            {
                GlobalRuleset.Value = e.NewValue;
            }
        }
    }
}
