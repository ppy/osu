// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Testing;
using osu.Game.Rulesets;
using osu.Game.Skinning;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public abstract class LegacySkinPlayerTestScene : PlayerTestScene
    {
        protected LegacySkin LegacySkin { get; private set; }

        private ISkinSource legacySkinSource;

        protected override TestPlayer CreatePlayer(Ruleset ruleset) => new SkinProvidingPlayer(legacySkinSource);

        [BackgroundDependencyLoader]
        private void load(SkinManager skins)
        {
            LegacySkin = new DefaultLegacySkin(skins);
            legacySkinSource = new SkinProvidingContainer(LegacySkin);
        }

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();
            addResetTargetsStep();
        }

        [TearDownSteps]
        public override void TearDownSteps()
        {
            addResetTargetsStep();
            base.TearDownSteps();
        }

        private void addResetTargetsStep()
        {
            AddStep("reset targets", () => this.ChildrenOfType<SkinnableTargetContainer>().ForEach(t =>
            {
                LegacySkin.ResetDrawableTarget(t);
                t.Reload();
            }));

            AddUntilStep("wait for components to load", () => this.ChildrenOfType<SkinnableTargetContainer>().All(t => t.ComponentsLoaded));
        }

        public class SkinProvidingPlayer : TestPlayer
        {
            [Cached(typeof(ISkinSource))]
            private readonly ISkinSource skinSource;

            public SkinProvidingPlayer(ISkinSource skinSource)
            {
                this.skinSource = skinSource;
            }
        }
    }
}
