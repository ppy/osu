// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Graphics.Particles;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Tests.Visual.Gameplay
{
    [TestFixture]
    public class TestSceneParticleSpewer : OsuTestScene
    {
        private TestParticleSpewer spewer;

        [Resolved]
        private SkinManager skinManager { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = spewer = createSpewer();

            AddToggleStep("toggle spawning", value => spewer.Active.Value = value);
            AddSliderStep("particle gravity", 0f, 250f, 0f, value => spewer.Gravity = value);
            AddSliderStep("particle velocity", 0f, 500f, 250f, value => spewer.MaxVelocity = value);
        }

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create jet", () => Child = spewer = createSpewer());
        }

        [Test]
        public void TestPresence()
        {
            AddStep("start jet", () => spewer.Active.Value = true);
            AddAssert("is present", () => spewer.IsPresent);

            AddWaitStep("wait for some particles", 3);
            AddStep("stop jet", () => spewer.Active.Value = false);

            AddWaitStep("wait for clean screen", 8);
            AddAssert("is not present", () => !spewer.IsPresent);
        }

        private TestParticleSpewer createSpewer()
        {
            return new TestParticleSpewer(skinManager.DefaultLegacySkin.GetTexture("star2"))
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            };
        }

        private class TestParticleSpewer : ParticleSpewer
        {
            private const int lifetime = 1500;
            private const int rate = 250;

            public float Gravity = 0;
            public float MaxVelocity = 250;

            protected override float ParticleGravity => Gravity;

            public TestParticleSpewer(Texture texture)
                : base(texture, rate, lifetime)
            {
            }

            protected override FallingParticle SpawnParticle()
            {
                var p = base.SpawnParticle();
                p.Velocity = new Vector2(
                    RNG.NextSingle(-MaxVelocity, MaxVelocity),
                    RNG.NextSingle(-MaxVelocity, MaxVelocity)
                );
                p.Duration = RNG.NextSingle(lifetime);
                p.StartAngle = RNG.NextSingle(MathF.PI * 2);
                p.EndAngle = RNG.NextSingle(MathF.PI * 2);
                p.EndScale = RNG.NextSingle(0.5f, 1.5f);

                return p;
            }
        }
    }
}
