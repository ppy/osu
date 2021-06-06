// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Allocation;
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
        protected override HueSelector CreateHueSelector() => new OsuHueSelector();
        protected override SaturationValueSelector CreateSaturationValueSelector() => new OsuSaturationValueSelector();

        [BackgroundDependencyLoader(true)]
        private void load([CanBeNull] OverlayColourProvider colourProvider, OsuColour osuColour)
        {
            Background.Colour = colourProvider?.Dark5 ?? osuColour.GreySeafoamDark;

            Content.Padding = new MarginPadding(10);
            Content.Spacing = new Vector2(0, 10);
        }

        private class OsuHueSelector : HueSelector
        {
            public OsuHueSelector()
            {
                Margin = new MarginPadding
                {
                    Bottom = 15
                };

                SliderBar.CornerRadius = SliderBar.Height / 2;
                SliderBar.Masking = true;
            }

            protected override Drawable CreateSliderNub() => new SliderNub();

            private class SliderNub : CompositeDrawable
            {
                public SliderNub()
                {
                    InternalChild = new Triangle
                    {
                        Width = 20,
                        Height = 15,
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.TopCentre
                    };
                }
            }
        }

        private class OsuSaturationValueSelector : SaturationValueSelector
        {
            public OsuSaturationValueSelector()
            {
                SelectionArea.CornerRadius = 10;
                SelectionArea.Masking = true;
                // purposefully use hard non-AA'd masking to avoid edge artifacts.
                SelectionArea.MaskingSmoothness = 0;
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
                        BorderThickness = 3,
                        EdgeEffect = new EdgeEffectParameters
                        {
                            Type = EdgeEffectType.Shadow,
                            Offset = new Vector2(0, 1),
                            Radius = 3,
                            Colour = Colour4.Black.Opacity(0.3f)
                        },
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
