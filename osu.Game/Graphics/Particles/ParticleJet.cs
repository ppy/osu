// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Textures;
using osu.Framework.Utils;
using osuTK;

namespace osu.Game.Graphics.Particles
{
    public class ParticleJet : ParticleSpewer
    {
        private const int particles_per_second = 80;
        private const double particle_lifetime = 500;
        private const float angular_velocity = 3f;
        private const int angle_spread = 10;
        private const float velocity_min = 1300f;
        private const float velocity_max = 1500f;

        private readonly int angle;

        protected override float ParticleGravity => 750f;

        public ParticleJet(Texture texture, int angle)
            : base(texture, particles_per_second, particle_lifetime)
        {
            this.angle = angle;
        }

        protected override FallingParticle SpawnParticle()
        {
            var p = base.SpawnParticle();

            var directionRads = MathUtils.DegreesToRadians(
                RNG.NextSingle(angle - angle_spread / 2, angle + angle_spread / 2)
            );
            var direction = new Vector2(MathF.Sin(directionRads), MathF.Cos(directionRads));

            p.StartPosition = OriginPosition;
            p.Duration = RNG.NextSingle((float)particle_lifetime * 0.8f, (float)particle_lifetime);
            p.Velocity = direction * new Vector2(RNG.NextSingle(velocity_min, velocity_max));
            p.AngularVelocity = RNG.NextSingle(-angular_velocity, angular_velocity);
            p.StartScale = 1f;
            p.EndScale = 2f;

            return p;
        }
    }
}
