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
    public class ParticleExplosion : Sprite
    {
        private readonly double duration;
        private double startTime;

        private readonly List<ParticlePart> parts = new List<ParticlePart>();

        public ParticleExplosion(Texture texture, int particleCount, double duration)
        {
            Texture = texture;
            this.duration = duration;
            Blending = BlendingParameters.Additive;

            for (int i = 0; i < particleCount; i++)
                parts.Add(new ParticlePart(duration));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Restart();
        }

        public void Restart()
        {
            startTime = TransformStartTime;

            this.FadeOutFromOne(duration);

            foreach (var p in parts)
                p.Randomise();
        }

        protected override void Update()
        {
            base.Update();

            Invalidate(Invalidation.DrawNode);
        }

        protected override DrawNode CreateDrawNode() => new ParticleExplosionDrawNode(this);

        private class ParticleExplosionDrawNode : SpriteDrawNode
        {
            private List<ParticlePart> parts = new List<ParticlePart>();

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

                parts = source.parts;
                sourceSize = source.Size;
                startTime = source.startTime;
                currentTime = source.Time.Current;
            }

            protected override void Blit(Action<TexturedVertex2D> vertexAction)
            {
                foreach (var p in parts)
                {
                    var pos = p.PositionAtTime(currentTime - startTime);

                    // todo: implement per particle.
                    var rect = new RectangleF(pos.X * sourceSize.X, pos.Y * sourceSize.Y, Texture.DisplayWidth, Texture.DisplayHeight);

                    var quad = new Quad(
                        Vector2Extensions.Transform(rect.TopLeft, DrawInfo.Matrix),
                        Vector2Extensions.Transform(rect.TopRight, DrawInfo.Matrix),
                        Vector2Extensions.Transform(rect.BottomLeft, DrawInfo.Matrix),
                        Vector2Extensions.Transform(rect.BottomRight, DrawInfo.Matrix)
                    );

                    DrawQuad(Texture, quad, DrawColourInfo.Colour, null, vertexAction,
                        new Vector2(InflationAmount.X / DrawRectangle.Width, InflationAmount.Y / DrawRectangle.Height),
                        null, TextureCoords);
                }
            }
        }

        private class ParticlePart
        {
            private readonly double totalDuration;

            private double duration;
            private double direction;
            private float distance;

            public ParticlePart(double totalDuration)
            {
                this.totalDuration = totalDuration;

                Randomise();
            }

            public Vector2 PositionAtTime(double time)
            {
                return new Vector2(0.5f) + positionForOffset(distance * (float)(time / duration));

                Vector2 positionForOffset(float offset) => new Vector2(
                    (float)(offset * Math.Sin(direction)),
                    (float)(offset * Math.Cos(direction))
                );
            }

            public void Randomise()
            {
                distance = RNG.NextSingle(0.5f);
                duration = RNG.NextDouble(totalDuration / 3, totalDuration);
                direction = RNG.NextSingle(0, MathF.PI * 2);
            }
        }
    }
}
