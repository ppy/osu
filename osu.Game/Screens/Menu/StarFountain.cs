// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Textures;
using osu.Framework.Threading;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.Menu
{
    public partial class StarFountain : SkinReloadableDrawable
    {
        private StarFountainSpewer spewer = null!;

        [Resolved]
        private TextureStore textures { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = spewer = CreateSpewer();
        }

        protected virtual StarFountainSpewer CreateSpewer() => new StarFountainSpewer();

        public void Shoot(int direction) => spewer.Shoot(direction);

        protected override void SkinChanged(ISkinSource skin)
        {
            base.SkinChanged(skin);
            spewer.Texture = skin.GetTexture("Menu/fountain-star") ?? textures.Get("Menu/fountain-star");
        }

        public partial class StarFountainSpewer : ParticleSpewer
        {
            private const int particle_duration_min = 300;
            private const int particle_duration_max = 1000;

            private double? lastShootTime;

            protected int LastShootDirection { get; private set; }

            protected override float ParticleGravity => 800;

            protected virtual double ShootDuration => 800;

            [Resolved]
            private ISkinSource skin { get; set; } = null!;

            public StarFountainSpewer()
                : this(240)
            {
            }

            protected StarFountainSpewer(int perSecond)
                : base(null, perSecond, particle_duration_max)
            {
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                Texture = skin.GetTexture("Menu/fountain-star") ?? textures.Get("Menu/fountain-star");
            }

            protected override FallingParticle CreateParticle()
            {
                return new FallingParticle
                {
                    StartPosition = new Vector2(0, 50),
                    Duration = RNG.NextSingle(particle_duration_min, particle_duration_max),
                    StartAngle = getRandomVariance(4),
                    EndAngle = getRandomVariance(2),
                    EndScale = 2.2f + getRandomVariance(0.4f),
                    Velocity = new Vector2(GetCurrentAngle(), -1400 + getRandomVariance(100)),
                };
            }

            protected virtual float GetCurrentAngle()
            {
                const float x_velocity_random_variance = 60;
                const float x_velocity_from_direction = 500;

                return LastShootDirection * x_velocity_from_direction * (float)(1 - 2 * (Clock.CurrentTime - lastShootTime!.Value) / ShootDuration) + getRandomVariance(x_velocity_random_variance);
            }

            private ScheduledDelegate? deactivateDelegate;

            public void Shoot(int direction)
            {
                Active.Value = true;

                deactivateDelegate?.Cancel();
                deactivateDelegate = Scheduler.AddDelayed(() => Active.Value = false, ShootDuration);

                lastShootTime = Clock.CurrentTime;
                LastShootDirection = direction;
            }

            private static float getRandomVariance(float variance) => RNG.NextSingle(-variance, variance);
        }
    }
}
