// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics.Containers;
using osu.Game.Extensions;

namespace osu.Game.Rulesets
{
    public partial class RulesetUpdater : CompositeDrawable
    {
        private IEnumerable<RulesetInfo> updatableRulesets;

        public RulesetUpdater(IEnumerable<RulesetInfo> rulesets)
        {
            updatableRulesets = rulesets.Where(r => !r.IsLegacyRuleset());
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            foreach (var ruleset in updatableRulesets)
            {
                var updateManager = ruleset.CreateInstance().CreateRulesetUpdateManager();

                if (updateManager == null)
                    continue;

                AddInternal(updateManager);
            }
        }
    }
}
