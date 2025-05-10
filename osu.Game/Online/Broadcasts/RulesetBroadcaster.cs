// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Rulesets;

namespace osu.Game.Online.Broadcasts
{
    public partial class RulesetBroadcaster : Broadcaster<RulesetInfo>
    {
        private IBindable<RulesetInfo>? ruleset;

        public RulesetBroadcaster()
            : base(@"ruleset")
        {
        }

        [BackgroundDependencyLoader]
        private void load(IBindable<RulesetInfo> ruleset)
        {
            this.ruleset = ruleset.GetBoundCopy();
            this.ruleset.BindValueChanged(value => Broadcast(value.NewValue), true);
        }
    }
}
