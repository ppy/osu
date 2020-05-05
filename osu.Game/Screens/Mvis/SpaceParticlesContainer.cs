using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osuTK;
using osuTK.Graphics;
using System;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Utils;

namespace osu.Game.Screens.Mvis.UI.Objects
{
    public class SpaceParticlesContainer : ParticlesContainer
    {
        /// <summary>
        /// Adjusts the speed of all the particles.
        /// </summary>
        private const int absolute_time = 5000;

        /// <summary>
        /// The maximum scale of a single particle.
        /// </summary>
        private const float particle_max_scale = 3;

        protected override Drawable CreateParticle(bool firstLoad) => new Particle();

        private class Particle : Circle
        {
            private Vector2 finalPosition;
            private double lifeTime;
            private float finalScale;

            public Particle()
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
                RelativePositionAxes = Axes.Both;
                Colour = Color4.White.Opacity(200);
                Position = new Vector2(RNG.NextSingle(-0.5f, 0.5f), RNG.NextSingle(-0.5f, 0.5f));
                Size = new Vector2(2);
                Alpha = 0;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                calculateValues();

                this.FadeIn(500);
                this.MoveTo(finalPosition, lifeTime);
                this.ScaleTo(finalScale, lifeTime);
                Expire();
            }

            private void calculateValues()
            {
                float finalX;
                float finalY;
                float ratio;

                if (Math.Abs(X) > Math.Abs(Y))
                {
                    ratio = Math.Abs(X) / 0.5f;
                    finalX = X > 0 ? 0.5f : -0.5f;
                    finalY = Y / ratio;
                }
                else
                {
                    ratio = Math.Abs(Y) / 0.5f;
                    finalY = Y > 0 ? 0.5f : -0.5f;
                    finalX = X / ratio;
                }

                finalPosition = new Vector2(finalX, finalY);

                float depth = RNG.NextSingle(0.25f, 1);
                Scale = new Vector2(depth);

                lifeTime = absolute_time * (1 - ratio) / depth;
                finalScale = 1 + ((particle_max_scale - 1) * depth * (1 - ratio));
            }
        }
    }
}
