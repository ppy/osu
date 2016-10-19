//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Transformations;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Input;
using osu.Framework.MathUtils;

namespace osu.Game.Beatmaps.Objects.Osu.Drawable
{
    public enum HitState
    {
        Disarmed,
        Armed
    }

    public class DrawableCircle : Container, IStateful<HitState>
    {
        private Sprite approachCircle;
        private CirclePart circle;
        private RingPart ring;
        private FlashPart flash;
        private ExplodePart explode;
        private NumberPart number;
        private GlowPart glow;
        private OsuBaseHit h;

        public DrawableCircle(Circle h)
        {
            this.h = h;

            Origin = Anchor.Centre;
            Alpha = 0;
            Position = h.Position;
            Scale = new Vector2(0.4f);

            Children = new Framework.Graphics.Drawable[]
            {
                glow = new GlowPart()
                {
                    Colour = h.Colour,
                },
                circle = new CirclePart()
                {
                    Colour = h.Colour,
                    Hit = delegate { State = HitState.Armed; }
                },
                number = new NumberPart(),
                ring = new RingPart(),
                flash = new FlashPart(),
                explode = new ExplodePart()
                {
                    Colour = h.Colour
                },
                approachCircle = new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Colour = h.Colour
                }
            };

            Size = new Vector2(100);

            State = HitState.Armed;
        }

        public override void Load(BaseGame game)
        {
            base.Load(game);

            approachCircle.Texture = game.Textures.Get(@"Play/osu/approachcircle@2x");

            Transforms.Add(new TransformAlpha(Clock) { StartTime = h.StartTime - 1000, EndTime = h.StartTime - 800, StartValue = 0, EndValue = 1 });

            approachCircle.Transforms.Add(new TransformScale(Clock) { StartTime = h.StartTime - 1000, EndTime = h.StartTime, StartValue = new Vector2(2f), EndValue = new Vector2(0.6f) });
            approachCircle.Transforms.Add(new TransformAlpha(Clock) { StartTime = h.StartTime, EndTime = h.StartTime, StartValue = 1, EndValue = 0 });

            glow.Transforms.Add(new TransformAlpha(Clock) { StartTime = h.StartTime, EndTime = h.StartTime + 400, StartValue = glow.Alpha, EndValue = 0 });

            updateState();

            Transforms.Add(new TransformScale(Clock) { StartTime = h.StartTime + h.Duration, EndTime = h.StartTime + h.Duration + 400, StartValue = Scale, EndValue = Scale * 1.5f, Easing = EasingTypes.OutQuad });
            Expire(true);
        }

        private HitState state;
        public HitState State
        {
            get { return state; }

            set
            {
                state = value;

                updateState();
            }
        }

        private void updateState()
        {
            if (!IsLoaded) return;

            switch (state)
            {
                case HitState.Disarmed:
                    Transforms.Add(new TransformAlpha(Clock) { StartTime = h.StartTime + h.Duration + 200, EndTime = h.StartTime + h.Duration + 400, StartValue = 1, EndValue = 0 });
                    break;
                case HitState.Armed:
                    const float flashIn = 30;
                    const float fadeOut = 800;

                    //Transforms.Add(new TransformScale(Clock) { StartTime = h.StartTime, EndTime = h.StartTime + 400, StartValue = Scale, EndValue = Scale * 1.1f });

                    ring.Transforms.Add(new TransformAlpha(Clock) { StartTime = h.StartTime + flashIn, EndTime = h.StartTime + flashIn, StartValue = 0, EndValue = 0 });
                    circle.Transforms.Add(new TransformAlpha(Clock) { StartTime = h.StartTime + flashIn, EndTime = h.StartTime + flashIn, StartValue = 0, EndValue = 0 });
                    number.Transforms.Add(new TransformAlpha(Clock) { StartTime = h.StartTime + flashIn, EndTime = h.StartTime + flashIn, StartValue = 0, EndValue = 0 });

                    flash.Transforms.Add(new TransformAlpha(Clock) { StartTime = h.StartTime, EndTime = h.StartTime + flashIn, StartValue = 0, EndValue = 0.8f });
                    flash.Transforms.Add(new TransformAlpha(Clock) { StartTime = h.StartTime + flashIn, EndTime = h.StartTime + flashIn + 100, StartValue = 0.8f, EndValue = 0 });

                    explode.Transforms.Add(new TransformAlpha(Clock) { StartTime = h.StartTime, EndTime = h.StartTime + flashIn, StartValue = 0, EndValue = 1 });

                    Transforms.Add(new TransformAlpha(Clock) { StartTime = h.StartTime + flashIn, EndTime = h.StartTime + flashIn + fadeOut, StartValue = 1, EndValue = 0 });

                    break;
            }
        }

        class NumberPart : Container
        {
            private Sprite number;

            public NumberPart()
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                Children = new[]
                {
                        number = new Sprite
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Alpha = 1,
                        }
                };
            }

            public override void Load(BaseGame game)
            {
                base.Load(game);
                number.Texture = game.Textures.Get(@"Play/osu/number@2x");
            }
        }

        class GlowPart : Container
        {
            private Sprite layer3;

            public GlowPart()
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                Children = new[]
                {
                        layer3 = new Sprite
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Additive = true,
                            Alpha = 0.5f,
                        }
                };
            }

            public override void Load(BaseGame game)
            {
                base.Load(game);
                layer3.Texture = game.Textures.Get(@"Play/osu/ring-glow@2x");
            }
        }

        class RingPart : Container
        {
            private Sprite ring;

            public RingPart()
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                Children = new Framework.Graphics.Drawable[]
                {
                    ring = new Sprite
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    },
                };
            }

            public override void Load(BaseGame game)
            {
                base.Load(game);
                ring.Texture = game.Textures.Get(@"Play/osu/ring@2x");
            }
        }

        class FlashPart : Container
        {
            public FlashPart()
            {
                Size = new Vector2(144);
                Masking = true;
                CornerRadius = Size.X / 2;

                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                Additive = true;
                Alpha = 0;

                Children = new Framework.Graphics.Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                    }
                };
            }
        }

        class ExplodePart : Container
        {
            public ExplodePart()
            {
                Size = new Vector2(144);

                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                Additive = true;
                Alpha = 0;

                Children = new Framework.Graphics.Drawable[]
                {
                    new Triangles
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                };
            }

            class Triangles : Container
            {
                private Texture tex;

                public override void Load(BaseGame game)
                {
                    base.Load(game);

                    tex = game.Textures.Get(@"Play/osu/triangle@2x");

                    for (int i = 0; i < 10; i++)
                    {
                        Add(new Sprite
                        {
                            Texture = tex,
                            Origin = Anchor.Centre,
                            Position = new Vector2(RNG.NextSingle() * DrawSize.X, RNG.NextSingle() * DrawSize.Y),
                            Scale = new Vector2(RNG.NextSingle() * 0.4f + 0.2f),
                            Alpha = RNG.NextSingle() * 0.3f,
                        });
                    }
                }

                protected override void Update()
                {
                    base.Update();

                    foreach (Framework.Graphics.Drawable d in Children)
                        d.Position -= new Vector2(0, (float)(d.Scale.X * (Clock.ElapsedFrameTime / 20)));
                }
            }
        }

        class CirclePart : Container
        {

            private Sprite disc;
            private Triangles triangles;

            public Action Hit;

            public CirclePart()
            {
                Size = new Vector2(144);
                Masking = true;
                CornerRadius = DrawSize.X / 2;

                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                Children = new Framework.Graphics.Drawable[]
                {
                    disc = new Sprite
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    },
                    triangles = new Triangles
                    {
                        Additive = true,
                        RelativeSizeAxes = Axes.Both,
                    },
                };
            }

            public override void Load(BaseGame game)
            {
                base.Load(game);
                disc.Texture = game.Textures.Get(@"Play/osu/disc@2x");
            }

            protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
            {
                Hit?.Invoke();
                return true;
            }

            class Triangles : Container
            {
                private Texture tex;

                public override void Load(BaseGame game)
                {
                    base.Load(game);

                    tex = game.Textures.Get(@"Play/osu/triangle@2x");

                    for (int i = 0; i < 10; i++)
                    {
                        Add(new Sprite
                        {
                            Texture = tex,
                            Origin = Anchor.Centre,
                            Position = new Vector2(RNG.NextSingle() * DrawSize.X, RNG.NextSingle() * DrawSize.Y),
                            Scale = new Vector2(RNG.NextSingle() * 0.4f + 0.2f),
                            Alpha = RNG.NextSingle() * 0.3f,
                        });
                    }
                }

                protected override void Update()
                {
                    base.Update();

                    foreach (Framework.Graphics.Drawable d in Children)
                        d.Position -= new Vector2(0, (float)(d.Scale.X * (Clock.ElapsedFrameTime / 20)));
                }
            }
        }
    }
}
