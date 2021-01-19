using System;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Audio;
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

        public Action OnFireAction;
        public ColourInfo GlowColor = Color4.Silver;

        private readonly ParallaxContainer hoverContainer;
        private readonly Container bounceContainer;
        private SampleChannel sample1;
        private SampleChannel sample2;
        private SampleChannel sample3;
        private readonly int rollTimes;

        [CanBeNull]
        private readonly DrawableSample targetSample;

        private int clickTime;
        private bool clicked;

        public MystryBox(int rollTimes, DrawableSample targetSample)
        {
            this.rollTimes = rollTimes;
            this.targetSample = targetSample;

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
                            }
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

            clickTime++;
            this.ScaleTo(1 + (clickTime * 0.1f), 500, Easing.OutElastic);

            if (clickTime == 3 || clickTime >= rollTimes)
            {
                clicked = true;

                content.TweenEdgeEffectTo(new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Glow,
                    Radius = 70f,
                    Colour = GlowColor
                });

                sample3?.Play();
                targetSample?.VolumeTo(1, 200, Easing.Out);
                this.ScaleTo(3, 300, Easing.OutQuint).FadeOut(300, Easing.OutQuad);
                OnFireAction?.Invoke();
                this.Delay(300).Then().Schedule(Hide);
            }
            else
            {
                content.TweenEdgeEffectTo(new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Glow,
                    Radius = 15 * clickTime,
                    Colour = GlowColor
                });

                targetSample?.VolumeTo(0.2f + clickTime * 0.2f, 200, Easing.Out);

                if (clickTime == 1)
                    sample1?.Play();

                if (clickTime == 2)
                    sample2?.Play();
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
