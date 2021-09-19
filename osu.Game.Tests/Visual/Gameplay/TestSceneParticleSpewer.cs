// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
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

        private TestParticleSpewer createSpewer() =>
            new TestParticleSpewer(skinManager.DefaultLegacySkin.GetTexture("star2"))
            {
                Origin = Anchor.Centre,
                RelativePositionAxes = Axes.Both,
                RelativeSizeAxes = Axes.Both,
                Position = new Vector2(0.5f),
                Size = new Vector2(0.5f),
            };

        private class TestParticleSpewer : ParticleSpewer
        {
            private const int max_duration = 1500;
            private const int rate = 250;

            public float Gravity;

            public float MaxVelocity = 0.25f;

            public Vector2 SpawnPosition { get; set; } = new Vector2(0.5f);

            protected override float ParticleGravity => Gravity;

            public TestParticleSpewer(Texture texture)
                : base(texture, rate, max_duration)
            {
            }

            protected override FallingParticle CreateParticle() =>
                new FallingParticle
                {
                    Velocity = new Vector2(
                        RNG.NextSingle(-MaxVelocity, MaxVelocity),
                        RNG.NextSingle(-MaxVelocity, MaxVelocity)
                    ),
                    StartPosition = SpawnPosition,
                    Duration = RNG.NextSingle(max_duration),
                    StartAngle = RNG.NextSingle(MathF.PI * 2),
                    EndAngle = RNG.NextSingle(MathF.PI * 2),
                    EndScale = RNG.NextSingle(0.5f, 1.5f)
                };
        }
    }
}
