// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Rulesets.UI
{
    public partial class DrawableRulesetDependenciesProvidingContainer : Container
    {
        private readonly Ruleset ruleset;

        private DrawableRulesetDependencies rulesetDependencies = null!;

        public DrawableRulesetDependenciesProvidingContainer(Ruleset ruleset)
        {
            this.ruleset = ruleset;
            RelativeSizeAxes = Axes.Both;
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            return rulesetDependencies = new DrawableRulesetDependencies(ruleset, base.CreateChildDependencies(parent));
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (rulesetDependencies.IsNotNull())
                rulesetDependencies.Dispose();
        }
    }
}
