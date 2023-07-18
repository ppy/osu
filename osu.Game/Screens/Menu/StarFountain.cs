// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Utils;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.Menu
{
    public partial class StarFountain : CompositeDrawable
    {
        private const int stars_per_shoot = 192;

        private DrawablePool<Star> starPool = null!;
        private Container starContainer = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                starPool = new DrawablePool<Star>(stars_per_shoot),
                starContainer = new Container()
            };
        }

        public void Shoot()
        {
            // left centre or right movement.
            int direction = RNG.Next(-1, 2);

            const float x_velocity_from_direction = 0.6f;
            const float x_velocity_random_variance = 0.25f;

            const float y_velocity_base = -2.0f;
            const float y_velocity_random_variance = 0.25f;

            const float x_spawn_position_variance = 10;
            const float y_spawn_position_offset = 50;

            for (int i = 0; i < stars_per_shoot; i++)
            {
                double initialOffset = i * 3;

                starContainer.Add(starPool.Get(s =>
                {
                    s.Velocity = new Vector2(
                        direction * x_velocity_from_direction + getRandomVariance(x_velocity_random_variance),
                        y_velocity_base + getRandomVariance(y_velocity_random_variance));

                    s.Position = new Vector2(getRandomVariance(x_spawn_position_variance), y_spawn_position_offset);

                    s.Hide();

                    using (s.BeginDelayedSequence(initialOffset))
                    {
                        double duration = RNG.Next(300, 1300);

                        s.ScaleTo(1)
                         .ScaleTo(RNG.NextSingle(1, 2.8f), duration, Easing.Out)
                         .FadeOutFromOne(duration, Easing.Out)
                         .Expire();
                    }
                }));
            }
        }

        private partial class Star : PoolableDrawable
        {
            public Vector2 Velocity = Vector2.Zero;

            private float rotation;

            [BackgroundDependencyLoader]
            private void load()
            {
                AutoSizeAxes = Axes.Both;
                Origin = Anchor.Centre;

                InternalChildren = new Drawable[]
                {
                    new SkinnableSprite("Menu/fountain-star")
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Blending = BlendingParameters.Additive,
                    }
                };

                rotation = getRandomVariance(2);
            }

            protected override void Update()
            {
                const float gravity = 0.003f;

                base.Update();

                float elapsed = (float)Time.Elapsed;

                Position += Velocity * elapsed;
                Velocity += new Vector2(0, elapsed * gravity);

                Rotation += rotation * elapsed;
            }
        }

        private static float getRandomVariance(float variance) => RNG.NextSingle(-variance, variance);
    }
}
