// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Tests.Visual.Gameplay
{
    [TestFixture]
    public class TestSceneParticleSpewer : OsuTestScene
    {
        private const int max_particle_duration = 1500;

        private float particleMaxVelocity = 0.5f;
        private Vector2 particleSpawnPosition = new Vector2(0.5f);

        private ParticleSpewer spewer;

        [Resolved]
        private SkinManager skinManager { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = spewer = createSpewer();

            AddToggleStep("toggle spawning", value => spewer.Active.Value = value);
            AddSliderStep("particle velocity", 0f, 1f, 0.5f, value => particleMaxVelocity = value);
            AddSliderStep("particle gravity", 0f, 1f, 0f, value => spewer.ParticleGravity = value);
            AddStep("move to new location", () =>
            {
                this.TransformTo(nameof(particleSpawnPosition), new Vector2(RNG.NextSingle(), RNG.NextSingle()), 1000, Easing.Out);
            });
        }

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create spewer", () => Child = spewer = createSpewer());
        }

        [Test]
        public void TestPresence()
        {
            AddStep("start spewer", () => spewer.Active.Value = true);
            AddAssert("is present", () => spewer.IsPresent);

            AddWaitStep("wait for some particles", 3);
            AddStep("stop spewer", () => spewer.Active.Value = false);

            AddWaitStep("wait for clean screen", 8);
            AddAssert("is not present", () => !spewer.IsPresent);
        }

        private ParticleSpewer createSpewer() =>
            new ParticleSpewer(skinManager.DefaultLegacySkin.GetTexture("star2"), 1500, max_particle_duration)
            {
                Origin = Anchor.Centre,
                RelativePositionAxes = Axes.Both,
                RelativeSizeAxes = Axes.Both,
                Position = new Vector2(0.5f),
                Size = new Vector2(0.5f),
                CreateParticle = createParticle,
            };

        private ParticleSpewer.FallingParticle? createParticle() =>
            new ParticleSpewer.FallingParticle
            {
                Velocity = new Vector2(
                    RNG.NextSingle(-particleMaxVelocity, particleMaxVelocity),
                    RNG.NextSingle(-particleMaxVelocity, particleMaxVelocity)
                ),
                StartPosition = particleSpawnPosition,
                Duration = RNG.NextSingle(max_particle_duration),
                StartAngle = RNG.NextSingle(MathF.PI * 2),
                EndAngle = RNG.NextSingle(MathF.PI * 2),
                EndScale = RNG.NextSingle(0.5f, 1.5f)
            };
    }
}
