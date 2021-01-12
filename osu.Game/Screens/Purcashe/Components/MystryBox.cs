using System;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Purcashe.Components
{
    public class MystryBox : CompositeDrawable
    {
        private readonly Container content;

        private float scale = 1;

        public Action OnFireAction;
        public ColourInfo GlowColor = Color4.Silver;

        private readonly ParallaxContainer hoverContainer;
        private readonly Container bounceContainer;
        private SampleChannel sample1;
        private SampleChannel sample2;
        private SampleChannel sample3;
        private readonly bool immidateFire;
        private bool clicked;

        public MystryBox(bool multipleTime)
        {
            immidateFire = !multipleTime;
            Size = new Vector2(400, 400);
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            InternalChild = hoverContainer = new ParallaxContainer
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                ParallaxAmount = 0,
                Child = bounceContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Child = content = new Container
                    {
                        CornerRadius = 25f,
                        Masking = true,
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,

                        Children = new Drawable[]
                        {
                            new Box
                            {
                                Colour = Color4Extensions.FromHex("#222"),
                                RelativeSizeAxes = Axes.Both,
                            },
                            new OsuSpriteText
                            {
                                Text = "?",
                                Font = OsuFont.Numeric.With(size: 60),
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre
                            },
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            sample1 = audio.Samples.Get(@"Menu/osu-logo-select");
            sample2 = audio.Samples.Get(@"Menu/button-play-select");
            sample3 = audio.Samples.Get(@"Menu/button-solo-select");
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (clicked) return base.OnClick(e);

            this.ScaleTo(scale += 0.1f, 500, Easing.OutElastic);

            if (scale == 1.1f)
            {
                content.TweenEdgeEffectTo(new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Glow,
                    Radius = 10f,
                    Colour = Color4.LightBlue
                });
                sample1?.Play();
            }

            if (scale == 1.2f)
            {
                content.TweenEdgeEffectTo(new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Glow,
                    Radius = 30f,
                    Colour = Color4.LightBlue
                });
                sample2?.Play();
            }

            if (scale > 1.2f || immidateFire)
            {
                clicked = true;

                content.TweenEdgeEffectTo(new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Glow,
                    Radius = 70f,
                    Colour = GlowColor
                });

                sample3?.Play();
                this.ScaleTo(3, 300, Easing.OutQuint).FadeOut(300, Easing.OutQuad);
                OnFireAction?.Invoke();
                this.Delay(300).Then().Schedule(Hide);
            }

            return base.OnClick(e);
        }

        protected override bool OnHover(HoverEvent e)
        {
            hoverContainer.ParallaxAmount = -0.025f;
            hoverContainer.ScaleTo(1.1f, 300, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            hoverContainer.ParallaxAmount = 0;
            hoverContainer.ScaleTo(1f, 300, Easing.OutQuint);
            base.OnHoverLost(e);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            bounceContainer.ScaleTo(0.9f, 1500, Easing.Out);
            return base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            bounceContainer.ScaleTo(1f, 600, Easing.OutElastic);
            base.OnMouseUp(e);
        }
    }
}
