// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Rulesets;

namespace osu.Game.Online.Broadcasts
{
    public partial class RulesetStateBroadcaster : GameStateBroadcaster<RulesetInfo>
    {
        public override string Type => @"Ruleset";
        public override RulesetInfo Message => ruleset.Value;

        private IBindable<RulesetInfo> ruleset = null!;

        [BackgroundDependencyLoader]
        private void load(IBindable<RulesetInfo> ruleset)
        {
            this.ruleset = ruleset.GetBoundCopy();
            this.ruleset.ValueChanged += _ => Broadcast();
        }
    }
}
