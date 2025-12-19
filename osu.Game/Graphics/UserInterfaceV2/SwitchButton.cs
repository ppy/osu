// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
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
        private readonly CircularContainer circularContainer;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        private Sample? sampleChecked;
        private Sample? sampleUnchecked;

        public SwitchButton()
        {
            Size = new Vector2(45, 20);

            InternalChild = circularContainer = new CircularContainer
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
        private void load(AudioManager audio)
        {
            sampleChecked = audio.Samples.Get(@"UI/check-on");
            sampleUnchecked = audio.Samples.Get(@"UI/check-off");
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Current.BindDisabledChanged(_ => updateColours());
            Current.BindValueChanged(_ => updateState(), true);

            FinishTransforms(true);
        }

        private void updateState()
        {
            switchCircle.MoveToX(Current.Value ? switchContainer.DrawWidth - switchCircle.DrawWidth : 0, 200, Easing.OutQuint);
            fill.FadeTo(Current.Value ? 1 : 0, 250, Easing.OutQuint);

            updateColours();
        }

        protected override bool OnHover(HoverEvent e)
        {
            updateColours();
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            updateColours();
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

        private void updateColours()
        {
            ColourInfo targetSwitchColour;
            ColourInfo targetBorderColour;

            if (Current.Disabled)
            {
                if (Current.Value)
                    targetBorderColour = colourProvider.Dark1.Opacity(0.5f);
                else
                    targetBorderColour = colourProvider.Background2.Opacity(0.5f);

                targetSwitchColour = colourProvider.Dark1.Opacity(0.5f);
                fill.Colour = colourProvider.Background5;
            }
            else
            {
                if (Current.Value)
                    targetBorderColour = IsHovered ? colourProvider.Highlight1.Lighten(0.3f) : colourProvider.Highlight1;
                else
                    targetBorderColour = IsHovered ? colourProvider.Background1 : colourProvider.Background2;

                targetSwitchColour = colourProvider.Highlight1;
                fill.Colour = colourProvider.Background4;
            }

            switchContainer.FadeColour(targetSwitchColour, 250, Easing.OutQuint);
            circularContainer.TransformTo(nameof(BorderColour), targetBorderColour, 250, Easing.OutQuint);
        }
    }
}
