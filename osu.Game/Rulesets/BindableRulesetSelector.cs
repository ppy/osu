// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;

namespace osu.Game.Rulesets
{
    public abstract class BindableRulesetSelector : RulesetSelector
    {
        protected readonly Bindable<RulesetInfo> GlobalRuleset = new Bindable<RulesetInfo>();

        [BackgroundDependencyLoader]
        private void load(Bindable<RulesetInfo> parentRuleset)
        {
            GlobalRuleset.BindTo(parentRuleset);

            GlobalRuleset.BindValueChanged(globalRulesetChanged);
            Current.BindValueChanged(OnLocalRulesetChanged);
        }

        private void globalRulesetChanged(ValueChangedEvent<RulesetInfo> e) => OnGlobalRulesetChanged(e);

        protected virtual void OnGlobalRulesetChanged(ValueChangedEvent<RulesetInfo> e)
        {
            Current.Value = e.NewValue;
        }

        protected virtual void OnLocalRulesetChanged(ValueChangedEvent<RulesetInfo> e)
        {
            if (!GlobalRuleset.Disabled)
            {
                GlobalRuleset.Value = e.NewValue;
            }
        }
    }
}
