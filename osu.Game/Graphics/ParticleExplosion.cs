// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
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

                AddInternal(new Particle(rDuration)
                {
                    Texture = texture
                });
            }
        }

        public void Restart()
        {
            foreach (var p in InternalChildren.OfType<Particle>())
                p.Play();
        }

        private class Particle : Sprite
        {
            private readonly double duration;

            public override bool RemoveWhenNotAlive => false;

            public Particle(double duration)
            {
                this.duration = duration;

                Origin = Anchor.Centre;
                Blending = BlendingParameters.Additive;

                RelativePositionAxes = Axes.Both;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                Play();
            }

            public void Play()
            {
                double direction = RNG.NextSingle(0, MathF.PI * 2);

                this.MoveTo(new Vector2(0.5f));
                this.MoveTo(new Vector2(0.5f) + positionForOffset(RNG.NextSingle(0.5f)), duration);

                this.FadeOutFromOne(duration);
                Expire();

                Vector2 positionForOffset(float offset) => new Vector2(
                    (float)(offset * Math.Sin(direction)),
                    (float)(offset * Math.Cos(direction))
                );
            }
        }
    }
}
