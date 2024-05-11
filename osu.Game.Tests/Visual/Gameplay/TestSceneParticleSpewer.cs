// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Tests.Visual.Gameplay
{
    [TestFixture]
    public partial class TestSceneParticleSpewer : OsuTestScene
    {
        private TestParticleSpewer spewer;

        [Resolved]
        private SkinManager skinManager { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = spewer = createSpewer();

            AddToggleStep("toggle spawning", value => spewer.Active.Value = value);
            AddSliderStep("particle gravity", 0f, 1f, 0f, value => spewer.Gravity = value);
            AddSliderStep("particle velocity", 0f, 1f, 0.5f, value => spewer.MaxVelocity = value);
            AddStep("move to new location", () =>
            {
                spewer.TransformTo(nameof(spewer.SpawnPosition), new Vector2(RNG.NextSingle(), RNG.NextSingle()), 1000, Easing.Out);
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

        [Test]
        public void TestTimeJumps()
        {
            ManualClock testClock = new ManualClock();

            AddStep("prepare clock", () =>
            {
                testClock.CurrentTime = TestParticleSpewer.MAX_DURATION * -3;
                spewer.Clock = new FramedClock(testClock);
            });
            AddStep("start spewer", () => spewer.Active.Value = true);
            AddAssert("spawned first particle", () => spewer.TotalCreatedParticles, () => Is.EqualTo(1));

            AddStep("move clock forward", () => testClock.CurrentTime = TestParticleSpewer.MAX_DURATION * 3);
            AddAssert("spawned second particle", () => spewer.TotalCreatedParticles, () => Is.EqualTo(2));

            AddStep("move clock backwards", () => testClock.CurrentTime = TestParticleSpewer.MAX_DURATION * -1);
            AddAssert("spawned third particle", () => spewer.TotalCreatedParticles, () => Is.EqualTo(3));
        }

        private TestParticleSpewer createSpewer() =>
            new TestParticleSpewer(skinManager.DefaultClassicSkin.GetTexture("star2"))
            {
                Origin = Anchor.Centre,
                RelativePositionAxes = Axes.Both,
                RelativeSizeAxes = Axes.Both,
                Position = new Vector2(0.5f),
                Size = new Vector2(0.5f),
            };

        private partial class TestParticleSpewer : ParticleSpewer
        {
            public const int MAX_DURATION = 1500;
            private const int rate = 250;

            public int TotalCreatedParticles { get; private set; }

            public float Gravity;

            public float MaxVelocity = 0.25f;

            public Vector2 SpawnPosition { get; set; } = new Vector2(0.5f);

            protected override float ParticleGravity => Gravity;

            public TestParticleSpewer(Texture texture)
                : base(texture, rate, MAX_DURATION)
            {
            }

            protected override FallingParticle CreateParticle()
            {
                TotalCreatedParticles++;

                return new FallingParticle
                {
                    Velocity = new Vector2(
                        RNG.NextSingle(-MaxVelocity, MaxVelocity),
                        RNG.NextSingle(-MaxVelocity, MaxVelocity)
                    ),
                    StartPosition = SpawnPosition,
                    Duration = RNG.NextSingle(MAX_DURATION),
                    StartAngle = RNG.NextSingle(MathF.PI * 2),
                    EndAngle = RNG.NextSingle(MathF.PI * 2),
                    EndScale = RNG.NextSingle(0.5f, 1.5f)
                };
            }
        }
    }
}
