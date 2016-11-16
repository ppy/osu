//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.ComponentModel;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Input;
using osu.Framework.IO.Stores;
using osu.Framework.MathUtils;
using osu.Game.Modes.Objects.Drawables;
using OpenTK;
using Container = osu.Framework.Graphics.Containers.Container;

namespace osu.Game.Modes.Osu.Objects.Drawables
{
    public class DrawableHitCircle : DrawableHitObject
    {
        private Sprite approachCircle;
        private CircleLayer circle;
        private RingLayer ring;
        private FlashLayer flash;
        private ExplodeLayer explode;
        private NumberLayer number;
        private GlowLayer glow;
        private OsuBaseHit h;
        private HitExplosion explosion;

        public DrawableHitCircle(HitCircle h) : base(h)
        {
            this.h = h;

            Origin = Anchor.Centre;
            RelativePositionAxes = Axes.Both;
            Position = new Vector2(h.Position.X / 512, h.Position.Y / 384);

            Children = new Drawable[]
            {
                glow = new GlowLayer
                {
                    Colour = h.Colour
                },
                circle = new CircleLayer
                {
                    Colour = h.Colour,
                    Hit = Hit,
                },
                number = new NumberLayer(),
                ring = new RingLayer(),
                flash = new FlashLayer(),
                explode = new ExplodeLayer
                {
                    Colour = h.Colour,
                },
                approachCircle = new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Colour = h.Colour
                }
            };

            //may not be so correct
            Size = circle.DrawSize;
        }

        [BackgroundDependencyLoader]
        private void load(BaseGame game)
        {
            approachCircle.Texture = game.Textures.Get(@"Play/osu/approachcircle@2x");
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            //force application of the state that was set before we loaded.
            UpdateState(State);
        }

        protected override void UpdateState(ArmedState state)
        {
            if (!IsLoaded) return;

            Flush(true); //move to DrawableHitObject

            double t = HitTime ?? h.StartTime;

            //sane defaults
            ring.Alpha = circle.Alpha = number.Alpha = approachCircle.Alpha = glow.Alpha = 1;
            explode.Alpha = 0;
            Scale = Vector2.One;

            //always-present transforms
            Transforms.Add(new TransformAlpha { StartTime = t - 1000, EndTime = t - 800, StartValue = 0, EndValue = 1 });
            approachCircle.Transforms.Add(new TransformScale { StartTime = t - 1000, EndTime = t, StartValue = new Vector2(2f), EndValue = new Vector2(0.6f) });

            //set transform delay to t==hitTime
            Delay(t - Time.Current, true);

            approachCircle.FadeOut();
            glow.FadeOut(400);

            switch (state)
            {
                case ArmedState.Disarmed:
                    Delay(h.Duration + 200);
                    FadeOut(200);

                    explosion?.Expire();
                    explosion = null;
                    break;
                case ArmedState.Armed:
                    const double flash_in = 30;

                    flash.FadeTo(0.8f, flash_in);
                    flash.Delay(flash_in);
                    flash.FadeOut(100);

                    explode.FadeIn(flash_in);

                    Delay(flash_in, true);

                    //after the flash, we can hide some elements that were behind it
                    ring.FadeOut();
                    circle.FadeOut();
                    number.FadeOut();

                    FadeOut(800);
                    ScaleTo(Scale * 1.5f, 400, EasingTypes.OutQuad);

                    Schedule(() => Add(explosion = new HitExplosion(Judgement.Hit300)));
                    break;
            }
        }

        private class NumberLayer : Container
        {
            private Sprite number;

            public NumberLayer()
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                Children = new[]
                {
                    number = new Sprite
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Alpha = 1
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                number.Texture = textures.Get(@"Play/osu/number@2x");
            }
        }

        private class GlowLayer : Container
        {
            private Sprite layer;

            public GlowLayer()
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                Children = new[]
                {
                    layer = new Sprite
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        BlendingMode = BlendingMode.Additive,
                        Alpha = 0.5f
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                layer.Texture = textures.Get(@"Play/osu/ring-glow@2x");
            }
        }

        private class RingLayer : Container
        {
            private Sprite ring;

            public RingLayer()
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                Children = new Drawable[]
                {
                    ring = new Sprite
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                ring.Texture = textures.Get(@"Play/osu/ring@2x");
            }
        }

        private class FlashLayer : Container
        {
            public FlashLayer()
            {
                Size = new Vector2(144);

                Masking = true;
                CornerRadius = Size.X / 2;

                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                BlendingMode = BlendingMode.Additive;
                Alpha = 0;

                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both
                    }
                };
            }
        }

        private class ExplodeLayer : Container
        {
            public ExplodeLayer()
            {
                Size = new Vector2(144);

                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                BlendingMode = BlendingMode.Additive;
                Alpha = 0;

                Children = new Drawable[]
                {
                    new Triangles
                    {
                        BlendingMode = BlendingMode.Additive,
                        RelativeSizeAxes = Axes.Both
                    }
                };
            }
        }

        private class CircleLayer : Container
        {

            private Sprite disc;
            private Triangles triangles;

            public Func<bool> Hit;

            public CircleLayer()
            {
                Size = new Vector2(144);
                Masking = true;
                CornerRadius = DrawSize.X / 2;

                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                Children = new Drawable[]
                {
                    disc = new Sprite
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre
                    },
                    triangles = new Triangles
                    {
                        BlendingMode = BlendingMode.Additive,
                        RelativeSizeAxes = Axes.Both
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                disc.Texture = textures.Get(@"Play/osu/disc@2x");
            }

            protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
            {
                Hit?.Invoke();
                return true;
            }
        }

        private class Triangles : Container
        {
            private Texture triangle;

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                triangle = textures.Get(@"Play/osu/triangle@2x");
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                for (int i = 0; i < 10; i++)
                {
                    Add(new Sprite
                    {
                        Texture = triangle,
                        Origin = Anchor.Centre,
                        RelativePositionAxes = Axes.Both,
                        Position = new Vector2(RNG.NextSingle(), RNG.NextSingle()),
                        Scale = new Vector2(RNG.NextSingle() * 0.4f + 0.2f),
                        Alpha = RNG.NextSingle() * 0.3f
                    });
                }
            }

            protected override void Update()
            {
                base.Update();

                foreach (Drawable d in Children)
                    d.Position -= new Vector2(0, (float)(d.Scale.X * (Time.Elapsed / 2880)));
            }
        }

        public enum Judgement
        {
            [Description(@"Miss")]
            Miss,
            [Description(@"50")]
            Hit50,
            [Description(@"100")]
            Hit100,
            [Description(@"300")]
            Hit300,
            [Description(@"500")]
            Hit500
        }

        public enum ComboJudgement
        {
            [Description(@"")]
            None,
            [Description(@"Good")]
            Good,
            [Description(@"Amazing")]
            Perfect
        }

        class HitExplosion : FlowContainer
        {
            private SpriteText line1;
            private SpriteText line2;

            public HitExplosion(Judgement judgement, ComboJudgement comboJudgement = ComboJudgement.None)
            {
                AutoSizeAxes = Axes.Both;
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                Direction = FlowDirection.VerticalOnly;
                Spacing = new Vector2(0, 2);

                Children = new Drawable[]
                {
                    line1 = new SpriteText
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Text = judgement.GetDescription(),
                        Font = @"Venera",
                        TextSize = 20,
                    },
                    line2 = new SpriteText
                    {
                        Text = comboJudgement.GetDescription(),
                        Font = @"Venera",
                        TextSize = 14,
                    }
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                line1.TransformSpacingTo(14, 1800, EasingTypes.OutQuint);
                line2.TransformSpacingTo(14, 1800, EasingTypes.OutQuint);
            }
        }
    }
}
