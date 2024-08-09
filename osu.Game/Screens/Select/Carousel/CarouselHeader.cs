// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Screens.Select.Carousel
{
    public partial class CarouselHeader : Container
    {
        public Container AlphaContainer;
        public Container EffectContainer;

        public readonly Bindable<CarouselItemState> State = new Bindable<CarouselItemState>(CarouselItemState.NotSelected);
        public static readonly Vector2 SHEAR = new Vector2(0.15f, 0);

        protected override Container<Drawable> Content { get; } = new Container { RelativeSizeAxes = Axes.Both };

        private const float corner_radius = 10;

        public CarouselHeader()
        {
            RelativeSizeAxes = Axes.X;
            Height = DrawableCarouselItem.MAX_HEIGHT;
            Shear = SHEAR;

            InternalChild = AlphaContainer = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Child = EffectContainer = new Container
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
            };
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
