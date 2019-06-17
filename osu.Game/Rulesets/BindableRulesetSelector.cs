// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;

namespace osu.Game.Rulesets
{
    public abstract class BindableRulesetSelector : RulesetSelector
    {
        [BackgroundDependencyLoader]
        private void load(Bindable<RulesetInfo> parentRuleset)
        {
            Current.BindTo(parentRuleset);
            Current.BindValueChanged(OnRulesetChanged);
        }

        protected virtual void OnRulesetChanged(ValueChangedEvent<RulesetInfo> e)
        {
            if (Current.Disabled)
            {
                return;
            }
        }
    }
}
