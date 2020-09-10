using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Tau.UI
{
    public class KiaiHitExplosion : CompositeDrawable
    {
        public override bool RemoveWhenNotAlive => true;
        private readonly List<Drawable> particles;
        private readonly bool circular;

        /// <summary>
        /// Used whenever circular isn't set to True.
        /// </summary>
        public float Angle;

        public KiaiHitExplosion(Color4 colour, bool circular = false)
        {
            this.circular = circular;
            var rng = new Random();
            particles = new List<Drawable>();

            RelativePositionAxes = Axes.Both;
            RelativeSizeAxes = Axes.Both;

            if (circular)
            {
                const int particle_count = 50;

                for (int i = 0; i < particle_count; i++)
                {
                    particles.Add(new Box
                    {
                        RelativePositionAxes = Axes.Both,
                        Position = Extensions.GetCircularPosition(((float)(rng.NextDouble() * 0.15f) * 0.15f) + 0.5f, (float)rng.NextDouble() * 360f),
                        Rotation = (float)rng.NextDouble() * 360f,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.BottomCentre,
                        Size = new Vector2(rng.Next(1, 15))
                    });
                }
            }
            else
            {
                const int particle_count = 10;

                for (int i = 0; i < particle_count; i++)
                {
                    particles.Add(new Box
                    {
                        RelativePositionAxes = Axes.Both,
                        Position = Extensions.GetCircularPosition((float)(rng.NextDouble() * 0.15f) * 0.15f, ((float)rng.NextDouble() / 10 * 10) + (Angle - 20)),
                        Rotation = (float)rng.NextDouble() * 360f,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.BottomCentre,
                        Size = new Vector2(rng.Next(1, 15))
                    });
                }
            }

            AddRangeInternal(particles.ToArray());
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            foreach (var particle in particles)
            {
                const double duration = 1500;
                var rng = new Random();

                if (circular)
                {
                    particle.MoveTo(Extensions.GetCircularPosition(((float)(rng.NextDouble() * 0.15f) * 2f) + 0.5f, Vector2.Zero.GetDegreesFromPosition(particle.Position)), duration, Easing.OutQuint)
                            .ScaleTo(new Vector2(rng.Next(1, 2)), duration, Easing.OutQuint)
                            .FadeOut(duration, Easing.OutQuint);
                }
                else
                {
                    particle.MoveTo(Extensions.GetCircularPosition((float)(rng.NextDouble() * 0.15f) * 1f, randomBetween(Angle - 40, Angle + 40)), duration, Easing.OutQuint)
                            .ResizeTo(new Vector2(rng.Next(0, 5)), duration, Easing.OutQuint)
                            .FadeOut(duration, Easing.OutQuint);

                    float randomBetween(float smallNumber, float bigNumber)
                    {
                        float diff = bigNumber - smallNumber;

                        return ((float)rng.NextDouble() * diff) + smallNumber;
                    }
                }

                particle.Expire(true);
                this.Delay(duration).Expire(true);
            }
        }
    }
}
