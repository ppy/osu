// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Overlays;
using osu.Game.Screens.Edit.Components;
using osu.Game.Screens.Edit.Components.Timelines.Summary;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Edit
{
    internal partial class BottomBar : CompositeDrawable
    {
        public TestGameplayButton TestGameplayButton { get; private set; }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider, Editor editor)
        {
            Anchor = Anchor.BottomLeft;
            Origin = Anchor.BottomLeft;

            RelativeSizeAxes = Axes.X;

            Height = 60;

            Masking = true;
            EdgeEffect = new EdgeEffectParameters
            {
                Colour = Color4.Black.Opacity(0.2f),
                Type = EdgeEffectType.Shadow,
                Radius = 10f,
            };

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background4,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = new GridContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        ColumnDimensions = new[]
                        {
                            new Dimension(GridSizeMode.Absolute, 170),
                            new Dimension(),
                            new Dimension(GridSizeMode.Absolute, 220),
                            new Dimension(GridSizeMode.Absolute, 120),
                        },
                        Content = new[]
                        {
                            new Drawable[]
                            {
                                new TimeInfoContainer { RelativeSizeAxes = Axes.Both },
                                new SummaryTimeline { RelativeSizeAxes = Axes.Both },
                                new PlaybackControl { RelativeSizeAxes = Axes.Both },
                                TestGameplayButton = new TestGameplayButton
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Padding = new MarginPadding { Left = 10 },
                                    Size = new Vector2(1),
                                    Action = editor.TestGameplay,
                                }
                            },
                        }
                    },
                }
            };
        }
    }
}
