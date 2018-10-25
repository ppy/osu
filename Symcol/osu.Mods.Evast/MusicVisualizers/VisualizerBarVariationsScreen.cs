// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Core.Screens.Evast;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Mods.Evast.MusicVisualizers
{
    public class VisualizerBarVariationsScreen : BeatmapScreen
    {
        public VisualizerBarVariationsScreen()
        {
            Children = new Drawable[]
            {
                new FallBarVisualizer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    BarsAmount = 100,
                    BarWidth = 5,
                    CircleSize = 250,
                    X = -400,
                },
                new SplittedBarVisualizer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    BarsAmount = 100,
                    BarWidth = 5,
                    CircleSize = 250,
                },
                new CircularBarVisualizer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    BarsAmount = 100,
                    BarWidth = 5,
                    CircleSize = 250,
                    X = 400,
                }
            };
        }

        private class CircularBarVisualizer : CircularVisualizer
        {
            protected override VisualizerBar CreateNewBar() => new CircularBar() { Masking = true };

            private class CircularBar : DefaultBar
            {
                public override void SetValue(float amplitudeValue, float valueMultiplier, int smoothness, int faloff)
                {
                    var newValue = Width + amplitudeValue * valueMultiplier;

                    if (newValue <= Height)
                        return;

                    this.ResizeHeightTo(newValue)
                        .Then()
                        .ResizeHeightTo(0, smoothness);
                }

                protected override void LoadComplete()
                {
                    base.LoadComplete();
                    CornerRadius = Width / 2;
                }
            }
        }

        private class SplittedBarVisualizer : CircularVisualizer
        {
            protected override VisualizerBar CreateNewBar() => new SplittedBar();

            private class SplittedBar : VisualizerBar
            {
                private const int spacing = 2;
                private const int piece_height = 2;

                private readonly Container mainBar;
                private readonly Container fakeBar;

                private int previousValue = -1;

                public SplittedBar()
                {
                    AutoSizeAxes = Axes.Y;
                    Children = new Drawable[]
                    {
                        mainBar = new Container
                        {
                            Origin = Anchor.BottomCentre,
                            AutoSizeAxes = Axes.Y,
                            RelativeSizeAxes = Axes.X
                        },
                        fakeBar = new Container { Origin = Anchor.BottomCentre }
                    };
                }

                public override void SetValue(float amplitudeValue, float valueMultiplier, int smoothness, int faloff)
                {
                    var newValue = amplitudeValue * valueMultiplier;

                    if (newValue <= fakeBar.Height)
                        return;

                    fakeBar.ResizeHeightTo(newValue)
                        .Then()
                        .ResizeHeightTo(0, smoothness);
                }

                protected override void Update()
                {
                    base.Update();

                    var currentValue = (int)(fakeBar.Height / (piece_height + spacing));
                    if (previousValue == currentValue)
                        return;

                    previousValue = currentValue;

                    if (mainBar.Children.Count > 0)
                        mainBar.Clear(true);

                    for (int i = 0; i < currentValue + 1; i++)
                        mainBar.Add(new Container
                        {
                            Origin = Anchor.BottomCentre,
                            RelativeSizeAxes = Axes.X,
                            Height = piece_height,
                            Y = i * (piece_height + spacing),
                            Child = new Box
                            {
                                EdgeSmoothness = Vector2.One,
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4.White,
                            }
                        });
                }
            }
        }

        private class FallBarVisualizer : CircularVisualizer
        {
            protected override VisualizerBar CreateNewBar() => new FallBar();

            private class FallBar : VisualizerBar
            {
                private readonly Container mainBar;
                private readonly Container fallingPiece;

                public FallBar()
                {
                    AutoSizeAxes = Axes.Y;
                    Children = new Drawable[]
                    {
                        mainBar = new Container
                        {
                            Origin = Anchor.BottomCentre,
                            RelativeSizeAxes = Axes.X,
                            Child = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4.White,
                                EdgeSmoothness = Vector2.One,
                            }
                        },
                        fallingPiece = new Container
                        {
                            Origin = Anchor.BottomCentre,
                            RelativeSizeAxes = Axes.X,
                            Height = 2,
                            Child = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4.White,
                                EdgeSmoothness = Vector2.One,
                            }
                        }
                    };
                }

                public override void SetValue(float amplitudeValue, float valueMultiplier, int smoothness, int faloff)
                {
                    var newValue = amplitudeValue * valueMultiplier;

                    if (newValue > mainBar.Height)
                    {
                        mainBar.ResizeHeightTo(newValue)
                            .Then()
                            .ResizeHeightTo(0, smoothness);
                    }

                    if(mainBar.Height > -fallingPiece.Y)
                    {
                        fallingPiece.MoveToY(-newValue)
                            .Then()
                            .MoveToY(0, smoothness * 6);
                    }
                }
            }
        }
    }
}
