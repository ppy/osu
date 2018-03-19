// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;

namespace osu.Game.Screens.Evast.MusicVisualizers
{
    public class VisualizerBarTestScreen : BeatmapScreen
    {
        public VisualizerBarTestScreen()
        {
            Children = new Drawable[]
            {
                new FallBarVisualizer()
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    BarsAmount = 100,
                    BarWidth = 5,
                    CircleSize = 250,
                    X = -400,
                },
                new CircularVisualizer()
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    BarsAmount = 100,
                    BarWidth = 5,
                    CircleSize = 250,
                },
                new CircularBarVisualizer()
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
                public override void SetValue(float amplitudeValue, float valueMultiplier, int softness, int faloff)
                {
                    var newValue = Width + amplitudeValue * valueMultiplier;

                    if (newValue <= Height)
                        return;

                    this.ResizeHeightTo(newValue)
                            .Then()
                            .ResizeHeightTo(0, softness);
                }

                protected override void LoadComplete()
                {
                    base.LoadComplete();
                    CornerRadius = Width / 2;
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

                public override void SetValue(float amplitudeValue, float valueMultiplier, int softness, int faloff)
                {
                    var newValue = amplitudeValue * valueMultiplier;

                    if (newValue > mainBar.Height)
                    {
                        mainBar.ResizeHeightTo(newValue)
                            .Then()
                            .ResizeHeightTo(0, softness);
                    }

                    if (mainBar.Height > -fallingPiece.Y)
                    {
                        fallingPiece.MoveToY(-newValue)
                            .Then()
                            .MoveToY(0, softness * 7);
                    }
                }
            }
        }
    }
}
