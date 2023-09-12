// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Select.Carousel
{
    public partial class CarouselHeader : Container
    {
        public Container AlphaContainer;
        public Container EffectContainer;
        public Container BorderContainer;

        public readonly Bindable<CarouselItemState> State = new Bindable<CarouselItemState>(CarouselItemState.NotSelected);
        public static readonly Vector2 SHEAR = new Vector2(0.15f, 0);

        protected override Container<Drawable> Content { get; } = new Container { RelativeSizeAxes = Axes.Both };

        public bool HasCustomBorder;

        private const float corner_radius = 10;
        private const float border_thickness = 2;

        public CarouselHeader()
        {
            RelativeSizeAxes = Axes.X;
            Height = DrawableCarouselItem.MAX_HEIGHT;
            Shear = SHEAR;

            InternalChild = AlphaContainer = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    EffectContainer = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        CornerRadius = corner_radius,
                        Children = new Drawable[]
                        {
                            Content,
                            new HoverLayer(),
                            new HeaderSounds(),
                        }
                    },
                    BorderContainer = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        CornerRadius = corner_radius,
                        BorderColour = ColourInfo.GradientHorizontal(Colour4.White, Colour4.White.Opacity(0)),
                        Child = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Colour4.Transparent,
                        }
                    },
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            State.BindValueChanged(updateState, true);
        }

        private void updateState(ValueChangedEvent<CarouselItemState> state)
        {
            switch (state.NewValue)
            {
                case CarouselItemState.Collapsed:
                case CarouselItemState.NotSelected:
                    BorderContainer.BorderThickness = 0;
                    EffectContainer.EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Shadow,
                        Offset = new Vector2(1),
                        Radius = 10,
                        Colour = Color4.Black.Opacity(100),
                    };

                    BorderContainer.EdgeEffect = new EdgeEffectParameters();
                    break;

                case CarouselItemState.Selected:
                    if (HasCustomBorder) return;

                    BorderContainer.BorderThickness = border_thickness;
                    EffectContainer.EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Shadow,
                        Colour = Color4Extensions.FromHex(@"4EBFFF").Opacity(0.5f),
                        Radius = 50,
                    };

                    BorderContainer.EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Shadow,
                        Colour = Color4Extensions.FromHex(@"4EBFFF").Opacity(0.5f),
                        Radius = 15,
                        Hollow = true,
                    };
                    break;
            }
        }

        public partial class HoverLayer : CompositeDrawable
        {
            private Box box = null!;

            public HoverLayer()
            {
                RelativeSizeAxes = Axes.Both;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                InternalChild = box = new Box
                {
                    Colour = colours.Blue.Opacity(0.1f),
                    Alpha = 0,
                    Blending = BlendingParameters.Additive,
                    RelativeSizeAxes = Axes.Both,
                };
            }

            protected override bool OnHover(HoverEvent e)
            {
                box.FadeIn(100, Easing.OutQuint);
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                box.FadeOut(1000, Easing.OutQuint);
                base.OnHoverLost(e);
            }
        }

        private partial class HeaderSounds : HoverSampleDebounceComponent
        {
            private Sample? sampleHover;

            [BackgroundDependencyLoader]
            private void load(AudioManager audio)
            {
                sampleHover = audio.Samples.Get("UI/default-hover");
            }

            public override void PlayHoverSample()
            {
                if (sampleHover == null) return;

                sampleHover.Frequency.Value = 0.99 + RNG.NextDouble(0.02);
                sampleHover.Play();
            }
        }
    }
}
