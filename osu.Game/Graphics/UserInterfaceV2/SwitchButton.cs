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
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public partial class SwitchButton : Checkbox
    {
        private const float border_thickness = 4.5f;
        private const float padding = 1.25f;

        private readonly Box fill;
        private readonly Container switchContainer;
        private readonly Drawable switchCircle;
        private readonly CircularBorderContainer circularContainer;

        private Color4 enabledColour;
        private Color4 disabledColour;

        private Sample? sampleChecked;
        private Sample? sampleUnchecked;

        public SwitchButton()
        {
            Size = new Vector2(45, 20);

            InternalChild = circularContainer = new CircularBorderContainer
            {
                RelativeSizeAxes = Axes.Both,
                BorderColour = Color4.White,
                BorderThickness = border_thickness,
                Masking = true,
                Children = new Drawable[]
                {
                    fill = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        AlwaysPresent = true,
                        Alpha = 0
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding(border_thickness + padding),
                        Child = switchContainer = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Child = switchCircle = new CircularContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                FillMode = FillMode.Fit,
                                Masking = true,
                                Child = new Box { RelativeSizeAxes = Axes.Both }
                            }
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader(true)]
        private void load(OverlayColourProvider? colourProvider, OsuColour colours, AudioManager audio)
        {
            enabledColour = colourProvider?.Highlight1 ?? colours.BlueDark;
            disabledColour = colourProvider?.Background3 ?? colours.Gray3;

            switchContainer.Colour = enabledColour;
            fill.Colour = disabledColour;

            sampleChecked = audio.Samples.Get(@"UI/check-on");
            sampleUnchecked = audio.Samples.Get(@"UI/check-off");
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Current.BindValueChanged(updateState, true);
            FinishTransforms(true);
        }

        private void updateState(ValueChangedEvent<bool> state)
        {
            switchCircle.MoveToX(state.NewValue ? switchContainer.DrawWidth - switchCircle.DrawWidth : 0, 200, Easing.OutQuint);
            fill.FadeTo(state.NewValue ? 1 : 0, 250, Easing.OutQuint);

            updateBorder();
        }

        protected override bool OnHover(HoverEvent e)
        {
            updateBorder();
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            updateBorder();
            base.OnHoverLost(e);
        }

        protected override void OnUserChange(bool value)
        {
            base.OnUserChange(value);

            if (value)
                sampleChecked?.Play();
            else
                sampleUnchecked?.Play();
        }

        private void updateBorder()
        {
            circularContainer.TransformBorderTo((Current.Value ? enabledColour : disabledColour).Lighten(IsHovered ? 0.3f : 0));
        }

        private partial class CircularBorderContainer : CircularContainer
        {
            public void TransformBorderTo(ColourInfo colour)
                => this.TransformTo(nameof(BorderColour), colour, 250, Easing.OutQuint);
        }
    }
}
