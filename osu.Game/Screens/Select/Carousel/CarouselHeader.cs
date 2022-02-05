// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
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
    public class CarouselHeader : Container
    {
        public Container BorderContainer;

        public readonly Bindable<CarouselItemState> State = new Bindable<CarouselItemState>(CarouselItemState.NotSelected);

        private readonly HoverLayer hoverLayer;

        protected override Container<Drawable> Content { get; } = new Container { RelativeSizeAxes = Axes.Both };

        private const float corner_radius = 10;
        private const float border_thickness = 2.5f;

        public CarouselHeader()
        {
            RelativeSizeAxes = Axes.X;
            Height = DrawableCarouselItem.MAX_HEIGHT;

            InternalChild = BorderContainer = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                CornerRadius = corner_radius,
                BorderColour = new Color4(221, 255, 255, 255),
                Children = new Drawable[]
                {
                    Content,
                    hoverLayer = new HoverLayer()
                }
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
                    hoverLayer.InsetForBorder = false;

                    BorderContainer.BorderThickness = 0;
                    BorderContainer.EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Shadow,
                        Offset = new Vector2(1),
                        Radius = 10,
                        Colour = Color4.Black.Opacity(100),
                    };
                    break;

                case CarouselItemState.Selected:
                    hoverLayer.InsetForBorder = true;

                    BorderContainer.BorderThickness = border_thickness;
                    BorderContainer.EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Glow,
                        Colour = new Color4(130, 204, 255, 150),
                        Radius = 20,
                        Roundness = 10,
                    };
                    break;
            }
        }

        public class HoverLayer : HoverSampleDebounceComponent
        {
            private Sample sampleHover;

            private Box box;

            public HoverLayer()
            {
                RelativeSizeAxes = Axes.Both;
            }

            [BackgroundDependencyLoader]
            private void load(AudioManager audio, OsuColour colours)
            {
                InternalChild = box = new Box
                {
                    Colour = colours.Blue.Opacity(0.1f),
                    Alpha = 0,
                    Blending = BlendingParameters.Additive,
                    RelativeSizeAxes = Axes.Both,
                };

                sampleHover = audio.Samples.Get("UI/default-hover");
            }

            public bool InsetForBorder
            {
                set
                {
                    if (value)
                    {
                        // apply same border as above to avoid applying additive overlay to it (and blowing out the colour).
                        Masking = true;
                        CornerRadius = corner_radius;
                        BorderThickness = border_thickness;
                    }
                    else
                    {
                        BorderThickness = 0;
                        CornerRadius = 0;
                        Masking = false;
                    }
                }
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

            public override void PlayHoverSample()
            {
                if (sampleHover == null) return;

                sampleHover.Frequency.Value = 0.99 + RNG.NextDouble(0.02);
                sampleHover.Play();
            }
        }
    }
}
