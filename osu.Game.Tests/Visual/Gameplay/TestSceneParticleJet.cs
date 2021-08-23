// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Graphics.Particles;
using osu.Game.Skinning;

namespace osu.Game.Tests.Visual.Gameplay
{
    [TestFixture]
    public class TestSceneParticleJet : OsuTestScene
    {
        private ParticleJet jet;

        [Resolved]
        private SkinManager skinManager { get; set; }

        public TestSceneParticleJet()
        {
            AddStep("create", () =>
            {
                Child = jet = createJet();
            });

            AddToggleStep("toggle spawning", value => jet.Active = value);
        }

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create jet", () => Child = jet = createJet());
        }

        [Test]
        public void TestPresence()
        {
            AddStep("start jet", () => jet.Active = true);
            AddAssert("is present", () => jet.IsPresent);

            AddWaitStep("wait for some particles", 3);
            AddStep("stop jet", () => jet.Active = false);

            AddWaitStep("wait for clean screen", 5);
            AddAssert("is not present", () => !jet.IsPresent);
        }

        private ParticleJet createJet()
        {
            return new ParticleJet(skinManager.DefaultLegacySkin.GetTexture("star2"), 180)
            {
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.BottomCentre,
                RelativePositionAxes = Axes.Y,
                Y = -0.1f,
            };
        }
    }
}
