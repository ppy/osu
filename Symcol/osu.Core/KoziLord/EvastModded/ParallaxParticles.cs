// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.MathUtils;
using OpenTK;
using OpenTK.Graphics;

namespace Symcol.osu.Core.KoziLord.EvastModded
{
    //TODO: Actually implement the Parallax, right now it's basically a straight copy of Evast's SpaceParticles.
    public class ParallaxSpaceParticles : Container//, IRequireHighFrequencyMousePosition
    { 
        //public const float DEFAULT_PARALLAX_AMOUNT = 0.02f;

        //public float ParallaxAmount = DEFAULT_PARALLAX_AMOUNT;


        /// <summary>
        /// Number of milliseconds between addition of a new particle.
        /// </summary>
        private const float time_between_updates = 10;

        /// <summary>
        /// Adjusts the speed of all the particles.
        /// </summary>
        private const int absolute_time = 5000;

        /// <summary>
        /// Maximum allowed amount of particles which can be shown at once.
        /// </summary>
        private const int max_particles_amount = 350;

        /// <summary>
        /// The size of a single particle.
        /// </summary>
        private const float particle_size = 2;

        /// <summary>
        /// The maximum scale of a single particle.
        /// </summary>
        private const float particle_max_scale = 5;

        public ParallaxSpaceParticles()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            generateParticles();
        }

        private void generateParticles()
        {
            if (Children.Count < max_particles_amount)
            {
                Add(new Particle
                {
                    Position = new Vector2(RNG.NextSingle(-0.5f, 0.5f), RNG.NextSingle(-0.5f, 0.5f)),
                    Depth = RNG.NextSingle(0.25f, 1),
                    Size = new Vector2(particle_size),
                });

                Scheduler.AddDelayed(generateParticles, time_between_updates);
            }
            else
                Scheduler.AddDelayed(generateParticles, time_between_updates * 2);
        }

        private class Particle : CircularContainer
        {
            private Vector2 finalPosition;
            private double lifeTime;
            private float finalScale;

            public Particle()
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
                RelativePositionAxes = Axes.Both;
                Masking = true;
                Child = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.White.Opacity(200),
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                calculateValues();

                this.MoveTo(finalPosition, lifeTime, Easing.In);
                this.ScaleTo(finalScale, lifeTime, Easing.In);
                Expire();
            }

            private void calculateValues()
            {
                float distanceFromZero = distance(Vector2.Zero, Position);

                float x = Position.X;
                float y = Position.Y;

                float fullDistance;

                if ((y < 0 && x + y < 0 && y < x) || (y > 0 && x + y > 0 && x < y))
                    fullDistance = Math.Abs((distance(Vector2.Zero, Position) * 0.5f) / distance(Vector2.Zero, new Vector2(0, y)));
                else
                    fullDistance = Math.Abs((distance(Vector2.Zero, Position) * 0.5f) / distance(Vector2.Zero, new Vector2(x, 0)));



                float xFinal = (Position.X * fullDistance) / distanceFromZero;
                float yFinal = (Position.Y * fullDistance) / distanceFromZero;

                finalPosition = new Vector2(xFinal, yFinal);

                float elapsedDistance = fullDistance - distanceFromZero;

                lifeTime = ((absolute_time * elapsedDistance) / fullDistance) / Depth;
                finalScale = 1 + ((particle_max_scale - 1) * Depth * (elapsedDistance / fullDistance));

                Scale = new Vector2(Depth);
            }

            private float distance(Vector2 pointFirst, Vector2 pointSecond)
            {
                float widthDiff = Math.Abs(pointFirst.X - pointSecond.X);
                float heightDiff = Math.Abs(pointFirst.Y - pointSecond.Y);

                return (float)Math.Sqrt((widthDiff * widthDiff) + (heightDiff * heightDiff));
            }
        }
    }
}
