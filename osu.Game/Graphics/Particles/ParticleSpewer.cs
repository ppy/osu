// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.OpenGL.Vertices;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osuTK;

namespace osu.Game.Graphics.Particles
{
    public abstract class ParticleSpewer : Sprite
    {
        private readonly FallingParticle[] particles;
        private int currentIndex;
        private double lastParticleAdded;

        private readonly double cooldown;
        private readonly double maxLifetime;

        /// <summary>
        /// Determines whether particles are being spawned.
        /// </summary>
        public readonly BindableBool Active = new BindableBool();

        /// <summary>
        /// <see cref="Drawable"/> whose DrawInfo will be used to draw each particle.
        /// Defaults to the <see cref="ParticleSpewer"/> itself.
        /// </summary>
        public IDrawable ParticleParent;

        public bool HasActiveParticles => Active.Value || (lastParticleAdded + maxLifetime) > Time.Current;
        public override bool IsPresent => base.IsPresent && HasActiveParticles;

        protected virtual float ParticleGravity => 0;

        protected ParticleSpewer(Texture texture, int perSecond, double maxLifetime)
        {
            Texture = texture;
            Blending = BlendingParameters.Additive;
            ParticleParent = this;

            particles = new FallingParticle[perSecond * (int)Math.Ceiling(maxLifetime / 1000)];

            cooldown = 1000f / perSecond;
            this.maxLifetime = maxLifetime;
        }

        protected override void Update()
        {
            base.Update();

            // reset cooldown if the clock was rewound.
            // this can happen when seeking in replays.
            if (lastParticleAdded > Time.Current) lastParticleAdded = 0;

            if (Active.Value && Time.Current > lastParticleAdded + cooldown)
            {
                addParticle(SpawnParticle());
            }

            Invalidate(Invalidation.DrawNode);
        }

        /// <summary>
        /// Called each time a new particle should be spawned.
        /// </summary>
        protected virtual FallingParticle SpawnParticle()
        {
            return new FallingParticle
            {
                StartTime = (float)Time.Current,
            };
        }

        private void addParticle(FallingParticle fallingParticle)
        {
            particles[currentIndex] = fallingParticle;

            currentIndex = (currentIndex + 1) % particles.Length;
            lastParticleAdded = Time.Current;
        }

        protected override DrawNode CreateDrawNode() => new ParticleSpewerDrawNode(this);

        # region DrawNode

        private class ParticleSpewerDrawNode : SpriteDrawNode
        {
            private readonly FallingParticle[] particles;

            protected new ParticleSpewer Source => (ParticleSpewer)base.Source;

            private float currentTime;
            private float gravity;
            private Matrix3 particleDrawMatrix;

            public ParticleSpewerDrawNode(Sprite source)
                : base(source)
            {
                particles = new FallingParticle[Source.particles.Length];
            }

            public override void ApplyState()
            {
                base.ApplyState();

                Source.particles.CopyTo(particles, 0);

                currentTime = (float)Source.Time.Current;
                gravity = Source.ParticleGravity;
                particleDrawMatrix = Source.ParticleParent.DrawInfo.Matrix;
            }

            protected override void Blit(Action<TexturedVertex2D> vertexAction)
            {
                foreach (var p in particles)
                {
                    // ignore particles that weren't initialized.
                    if (p.StartTime <= 0) continue;

                    var timeSinceStart = currentTime - p.StartTime;

                    // ignore particles from the future.
                    // these can appear when seeking in replays.
                    if (timeSinceStart < 0) continue;

                    var alpha = p.AlphaAtTime(timeSinceStart);
                    if (alpha <= 0) continue;

                    var scale = p.ScaleAtTime(timeSinceStart);
                    var pos = p.PositionAtTime(timeSinceStart, gravity);
                    var angle = p.AngleAtTime(timeSinceStart);

                    var width = Texture.DisplayWidth * scale;
                    var height = Texture.DisplayHeight * scale;

                    var rect = new RectangleF(
                        pos.X - width / 2,
                        pos.Y - height / 2,
                        width,
                        height);

                    var quad = new Quad(
                        rotatePosition(rect.TopLeft, rect.Centre, angle),
                        rotatePosition(rect.TopRight, rect.Centre, angle),
                        rotatePosition(rect.BottomLeft, rect.Centre, angle),
                        rotatePosition(rect.BottomRight, rect.Centre, angle)
                    );

                    DrawQuad(Texture, quad, DrawColourInfo.Colour.MultiplyAlpha(alpha), null, vertexAction,
                        new Vector2(InflationAmount.X / DrawRectangle.Width, InflationAmount.Y / DrawRectangle.Height),
                        null, TextureCoords);
                }
            }

            private Vector2 rotatePosition(Vector2 pos, Vector2 centre, float angle)
            {
                float cos = MathF.Cos(angle);
                float sin = MathF.Sin(angle);

                float x = centre.X + (pos.X - centre.X) * cos + (pos.Y - centre.Y) * sin;
                float y = centre.Y + (pos.Y - centre.Y) * cos - (pos.X - centre.X) * sin;

                return Vector2Extensions.Transform(new Vector2(x, y), particleDrawMatrix);
            }
        }

        #endregion

        protected struct FallingParticle
        {
            public float StartTime;
            public Vector2 StartPosition;
            public Vector2 Velocity;
            public float Duration;
            public float StartAngle;
            public float EndAngle;
            public float EndScale;

            public float AlphaAtTime(float timeSinceStart) => 1 - progressAtTime(timeSinceStart);

            public float ScaleAtTime(float timeSinceStart) => 1 + (EndScale - 1) * progressAtTime(timeSinceStart);

            public float AngleAtTime(float timeSinceStart) => StartAngle + (EndAngle - StartAngle) * progressAtTime(timeSinceStart);

            public Vector2 PositionAtTime(float timeSinceStart, float gravity)
            {
                var progress = progressAtTime(timeSinceStart);
                var currentGravity = new Vector2(0, gravity * Duration / 1000 * progress);

                return StartPosition + (Velocity + currentGravity) * timeSinceStart / 1000;
            }

            private float progressAtTime(float timeSinceStart) => Math.Clamp(timeSinceStart / Duration, 0, 1);
        }
    }
}
