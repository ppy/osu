// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Utils;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Cards
{
    public partial class SongPreviewParticleContainer : CompositeDrawable
    {
        public SongPreviewParticleContainer()
        {
            RelativeSizeAxes = Axes.Both;
        }

        private Vector2 lastPosition;

        private Texture[] particleTextures = null!;

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            const int texture_count = 3;

            particleTextures = new Texture[texture_count];

            for (int i = 0; i < texture_count; i++)
            {
                particleTextures[i] = textures.Get($"Online/RankedPlay/note-particle-{i}");
                Debug.Assert(particleTextures[i] != null);
            }
        }

        public void AddParticles(Drawable source, Color4 seedColour)
        {
            var drawQuad = ToLocalSpace(source.ScreenSpaceDrawQuad);

            var position = sampleRandomPosition(drawQuad);

            for (int i = 0; i < 10; i++)
            {
                if (Vector2.Distance(position, lastPosition) > 100)
                    break;

                position = sampleRandomPosition(drawQuad);
            }

            lastPosition = position;

            var texture = particleTextures[RNG.Next(particleTextures.Length)];

            var particle = new Particle(texture)
            {
                Position = position,
                Rotation = RNG.NextSingle(-3, 3),
                Colour = seedColour,
                Blending = BlendingParameters.Additive,
            };

            AddInternal(particle);

            particle.ScaleTo(0)
                    .ScaleTo(RNG.NextSingle(0.75f, 1f), 1000, Easing.OutElasticHalf)
                    .Then()
                    .FadeOut(1800, Easing.OutCubic)
                    .Expire();
        }

        private static Vector2 sampleRandomPosition(Quad quad)
        {
            static float remap(float value, float fromLower, float fromHigher, float toLower, float toHigher) =>
                (value - fromLower) / (fromHigher - fromLower) * (toHigher - toLower) + toLower;

            static float randomValue()
            {
                float x = RNG.NextSingle();
                // using quadratic rational smoothstep to increase the likelihood that particles spawn at the edge of the card
                float smoothStep = x * x / (2f * x * x - 2f * x + 1f);

                if (smoothStep < 0.5f)
                    return remap(smoothStep, 0, 0.5f, -0.05f, 0.15f);
                else
                    return remap(smoothStep, 0.5f, 1f, 0.85f, 1.05f);
            }

            var top = Vector2.Lerp(quad.TopLeft, quad.TopRight, randomValue());
            var bottom = Vector2.Lerp(quad.BottomLeft, quad.BottomRight, randomValue());

            return Vector2.Lerp(top, bottom, randomValue());
        }

        private partial class Particle : Sprite
        {
            public Particle(Texture texture)
            {
                Size = new Vector2(40);
                Texture = texture;
                Origin = Anchor.Centre;
            }

            private float initialX;
            private readonly float seed = RNG.NextSingle() * MathF.PI * 2;

            protected override void LoadComplete()
            {
                base.LoadComplete();

                initialX = X;
            }

            protected override void Update()
            {
                base.Update();

                X = initialX + (float)Math.Cos(Time.Current * 0.002 + seed) * 5;
                Y -= (float)(Time.Elapsed * 0.04f);
            }
        }
    }
}
