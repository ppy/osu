// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Testing;
using osu.Game.Skinning;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public abstract class LegacySkinPlayerTestScene : PlayerTestScene
    {
        protected LegacySkin LegacySkin { get; private set; }

        protected override ISkin GetPlayerSkin() => LegacySkin;

        [BackgroundDependencyLoader]
        private void load(SkinManager skins)
        {
            LegacySkin = new DefaultLegacySkin(skins);
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
        }
    }
}
