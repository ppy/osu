// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public class OsuHSVColourPicker : HSVColourPicker
    {
        private const float spacing = 10;
        private const float corner_radius = 10;
        private const float control_border_thickness = 3;

        protected override HueSelector CreateHueSelector() => new OsuHueSelector();
        protected override SaturationValueSelector CreateSaturationValueSelector() => new OsuSaturationValueSelector();

        [BackgroundDependencyLoader(true)]
        private void load([CanBeNull] OverlayColourProvider colourProvider, OsuColour osuColour)
        {
            Background.Colour = colourProvider?.Dark5 ?? osuColour.GreySeaFoamDark;

            Content.Padding = new MarginPadding(spacing);
            Content.Spacing = new Vector2(0, spacing);
        }

        private static EdgeEffectParameters createShadowParameters() => new EdgeEffectParameters
        {
            Type = EdgeEffectType.Shadow,
            Offset = new Vector2(0, 1),
            Radius = 3,
            Colour = Colour4.Black.Opacity(0.3f)
        };

        private class OsuHueSelector : HueSelector
        {
            public OsuHueSelector()
            {
                SliderBar.CornerRadius = corner_radius;
                SliderBar.Masking = true;
            }

            protected override Drawable CreateSliderNub() => new SliderNub(this);

            private class SliderNub : CompositeDrawable
            {
                private readonly Bindable<float> hue;
                private readonly Box fill;

                public SliderNub(OsuHueSelector osuHueSelector)
                {
                    hue = osuHueSelector.Hue.GetBoundCopy();

                    InternalChild = new CircularContainer
                    {
                        Height = 35,
                        Width = 10,
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,
                        Masking = true,
                        BorderColour = Colour4.White,
                        BorderThickness = control_border_thickness,
                        EdgeEffect = createShadowParameters(),
                        Child = fill = new Box
                        {
                            RelativeSizeAxes = Axes.Both
                        }
                    };
                }

                protected override void LoadComplete()
                {
                    hue.BindValueChanged(h => fill.Colour = Colour4.FromHSV(h.NewValue, 1, 1), true);
                }
            }
        }

        private class OsuSaturationValueSelector : SaturationValueSelector
        {
            public OsuSaturationValueSelector()
            {
                SelectionArea.CornerRadius = corner_radius;
                SelectionArea.Masking = true;
            }

            protected override Marker CreateMarker() => new OsuMarker();

            private class OsuMarker : Marker
            {
                private readonly Box previewBox;

                public OsuMarker()
                {
                    AutoSizeAxes = Axes.Both;

                    InternalChild = new CircularContainer
                    {
                        Size = new Vector2(20),
                        Masking = true,
                        BorderColour = Colour4.White,
                        BorderThickness = control_border_thickness,
                        EdgeEffect = createShadowParameters(),
                        Child = previewBox = new Box
                        {
                            RelativeSizeAxes = Axes.Both
                        }
                    };
                }

                protected override void LoadComplete()
                {
                    base.LoadComplete();

                    Current.BindValueChanged(colour => previewBox.Colour = colour.NewValue, true);
                }
            }
        }
    }
}
