// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Utils;
using osuTK;

namespace osu.Game.Graphics
{
    public class ParticleExplosion : CompositeDrawable
    {
        public ParticleExplosion(Texture texture, int particleCount, double duration)
        {
            for (int i = 0; i < particleCount; i++)
            {
                double rDuration = RNG.NextDouble(duration / 3, duration);

                AddInternal(new Particle(rDuration, RNG.NextSingle(0, MathF.PI * 2))
                {
                    Texture = texture
                });
            }
        }

        private class Particle : Sprite
        {
            private readonly double duration;
            private readonly float direction;

            private Vector2 positionForOffset(float offset) => new Vector2(
                (float)(offset * Math.Sin(direction)),
                (float)(offset * Math.Cos(direction))
            );

            public Particle(double duration, float direction)
            {
                this.duration = duration;
                this.direction = direction;
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
                RelativePositionAxes = Axes.Both;
                Position = positionForOffset(0);
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                this.MoveTo(positionForOffset(1), duration);
                this.FadeOut(duration);
                Expire();
            }
        }
    }
}
