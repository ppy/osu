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
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Select.Carousel
{
    public class CarouselHeader : Container
    {
        private SampleChannel sampleHover;

        private readonly Box hoverLayer;

        public Container BorderContainer;

        public readonly Bindable<CarouselItemState> State = new Bindable<CarouselItemState>(CarouselItemState.NotSelected);

        protected override Container<Drawable> Content { get; } = new Container { RelativeSizeAxes = Axes.Both };

        public CarouselHeader()
        {
            RelativeSizeAxes = Axes.X;
            Height = DrawableCarouselItem.MAX_HEIGHT;

            InternalChild = BorderContainer = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                CornerRadius = 10,
                BorderColour = new Color4(221, 255, 255, 255),
                Children = new Drawable[]
                {
                    Content,
                    hoverLayer = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0,
                        Blending = BlendingParameters.Additive,
                    },
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, OsuColour colours)
        {
            sampleHover = audio.Samples.Get($@"SongSelect/song-ping-variation-{RNG.Next(1, 5)}");
            hoverLayer.Colour = colours.Blue.Opacity(0.1f);
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
                    BorderContainer.EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Shadow,
                        Offset = new Vector2(1),
                        Radius = 10,
                        Colour = Color4.Black.Opacity(100),
                    };
                    break;

                case CarouselItemState.Selected:
                    BorderContainer.BorderThickness = 2.5f;
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

        protected override bool OnHover(HoverEvent e)
        {
            sampleHover?.Play();

            hoverLayer.FadeIn(100, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            hoverLayer.FadeOut(1000, Easing.OutQuint);
            base.OnHoverLost(e);
        }
    }
}
