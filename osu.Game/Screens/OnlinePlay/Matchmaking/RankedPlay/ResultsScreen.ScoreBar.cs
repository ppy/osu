// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay
{
    public partial class ResultsScreen
    {
        public partial class ScoreBar(RankedPlayColourScheme colours) : CompositeDrawable
        {
            [BackgroundDependencyLoader]
            private void load()
            {
                Masking = true;
                CornerRadius = 4;
                CornerExponent = 4;

                InternalChildren =
                [
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Height = 1 / 3f,
                        Colour = ColourInfo.GradientVertical(colours.Primary, colours.PrimaryDarker)
                    },
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        RelativePositionAxes = Axes.Y,
                        Height = 2f / 3f,
                        Y = 1f / 3f,
                        Colour = ColourInfo.GradientVertical(colours.PrimaryDarker, colours.PrimaryDarkest)
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding(3),
                        Child = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Masking = true,
                            CornerRadius = 3,
                            Colour = ColourInfo.GradientHorizontal(Colour4.White, Colour4.White.Opacity(0)),
                            Blending = BlendingParameters.Additive,
                            Child = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Alpha = 0,
                                AlwaysPresent = true,
                            }
                        }
                    },
                ];
            }
        }
    }
}
