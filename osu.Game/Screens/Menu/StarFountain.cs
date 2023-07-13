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
        private DrawablePool<Star> starPool = null!;
        private Container starContainer = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                starPool = new DrawablePool<Star>(192),
                starContainer = new Container()
            };
        }

        public void Shoot()
        {
            int dir = RNG.Next(-1, 2);

            for (int i = 0; i < 192; i++)
            {
                double offset = i * 3;

                starContainer.Add(starPool.Get(s =>
                {
                    s.Velocity = new Vector2(dir * 0.6f + RNG.NextSingle(-0.25f, 0.25f), -RNG.NextSingle(2.2f, 2.4f));
                    s.Position = new Vector2(RNG.NextSingle(-5, 5), 50);
                    s.Hide();

                    using (s.BeginDelayedSequence(offset))
                    {
                        s.FadeIn();
                        s.ScaleTo(1);

                        double duration = RNG.Next(300, 1300);

                        s.FadeOutFromOne(duration, Easing.Out);
                        s.ScaleTo(RNG.NextSingle(1, 2.8f), duration, Easing.Out);

                        s.Expire();
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
                    new SkinnableSprite("star2")
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Blending = BlendingParameters.Additive,
                    }
                };

                rotation = RNG.NextSingle(-2f, 2f);
            }

            protected override void Update()
            {
                const float gravity = 0.004f;

                base.Update();

                float elapsed = (float)Time.Elapsed;

                Position += Velocity * elapsed;
                Velocity += new Vector2(0, elapsed * gravity);

                Rotation += rotation * elapsed;
            }
        }
    }
}
