// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.OpenGL.Vertices;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Utils;
using osuTK;

namespace osu.Game.Graphics
{
    /// <summary>
    /// An explosion of textured particles based on how osu-stable randomises the explosion pattern.
    /// </summary>
    public class ParticleExplosion : Sprite
    {
        private readonly int particleCount;
        private readonly double duration;
        private double startTime;

        private readonly List<ParticlePart> parts = new List<ParticlePart>();

        public ParticleExplosion(Texture texture, int particleCount, double duration)
        {
            Texture = texture;
            this.particleCount = particleCount;
            this.duration = duration;
            Blending = BlendingParameters.Additive;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Restart();
        }

        /// <summary>
        /// Restart the animation from the current point in time.
        /// Supports transform time offset chaining.
        /// </summary>
        public void Restart()
        {
            startTime = TransformStartTime;
            this.FadeOutFromOne(duration);

            parts.Clear();
            for (int i = 0; i < particleCount; i++)
                parts.Add(new ParticlePart(duration));
        }

        protected override void Update()
        {
            base.Update();
            Invalidate(Invalidation.DrawNode);
        }

        protected override DrawNode CreateDrawNode() => new ParticleExplosionDrawNode(this);

        private class ParticleExplosionDrawNode : SpriteDrawNode
        {
            private readonly List<ParticlePart> parts = new List<ParticlePart>();

            private ParticleExplosion source => (ParticleExplosion)Source;

            private double startTime;
            private double currentTime;
            private Vector2 sourceSize;

            public ParticleExplosionDrawNode(Sprite source)
                : base(source)
            {
            }

            public override void ApplyState()
            {
                base.ApplyState();

                parts.Clear();
                parts.AddRange(source.parts);

                sourceSize = source.Size;
                startTime = source.startTime;
                currentTime = source.Time.Current;
            }

            protected override void Blit(Action<TexturedVertex2D> vertexAction)
            {
                var time = currentTime - startTime;

                foreach (var p in parts)
                {
                    Vector2 pos = p.PositionAtTime(time);
                    float alpha = p.AlphaAtTime(time);

                    var rect = new RectangleF(
                        pos.X * sourceSize.X - Texture.DisplayWidth / 2,
                        pos.Y * sourceSize.Y - Texture.DisplayHeight / 2,
                        Texture.DisplayWidth,
                        Texture.DisplayHeight);

                    // convert to screen space.
                    var quad = new Quad(
                        Vector2Extensions.Transform(rect.TopLeft, DrawInfo.Matrix),
                        Vector2Extensions.Transform(rect.TopRight, DrawInfo.Matrix),
                        Vector2Extensions.Transform(rect.BottomLeft, DrawInfo.Matrix),
                        Vector2Extensions.Transform(rect.BottomRight, DrawInfo.Matrix)
                    );

                    DrawQuad(Texture, quad, DrawColourInfo.Colour.MultiplyAlpha(alpha), null, vertexAction,
                        new Vector2(InflationAmount.X / DrawRectangle.Width, InflationAmount.Y / DrawRectangle.Height),
                        null, TextureCoords);
                }
            }
        }

        private readonly struct ParticlePart
        {
            private readonly double duration;
            private readonly float direction;
            private readonly float distance;

            public ParticlePart(double availableDuration)
            {
                distance = RNG.NextSingle(0.5f);
                duration = RNG.NextDouble(availableDuration / 3, availableDuration);
                direction = RNG.NextSingle(0, MathF.PI * 2);
            }

            public float AlphaAtTime(double time) => 1 - progressAtTime(time);

            public Vector2 PositionAtTime(double time)
            {
                var travelledDistance = distance * progressAtTime(time);
                return new Vector2(0.5f) + travelledDistance * new Vector2(MathF.Sin(direction), MathF.Cos(direction));
            }

            private float progressAtTime(double time) => (float)Math.Clamp(time / duration, 0, 1);
        }
    }
}
