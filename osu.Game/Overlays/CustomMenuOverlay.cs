// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays.CustomMenu;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays
{
    public class CustomMenuOverlay : FullscreenOverlay
    {

        public CustomMenuOverlay()
            : base(OverlayColourScheme.Orange)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = ColourProvider.Background6
                },
                new BasicScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    ScrollbarVisible = false,
                    Child = new ReverseChildIDFillFlowContainer<Drawable>
                    {
                        AutoSizeAxes = Axes.Y,
                        RelativeSizeAxes = Axes.X,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(0, 10),
                        Children = new Drawable[]
                        {
                            new FillFlowContainer
                            {
                                AutoSizeAxes = Axes.Y,
                                RelativeSizeAxes = Axes.X,
                                Direction = FillDirection.Vertical,
                                Masking = true,
                                EdgeEffect = new EdgeEffectParameters
                                {
                                    Colour = Color4.Black.Opacity(0.25f),
                                    Type = EdgeEffectType.Shadow,
                                    Radius = 3,
                                    Offset = new Vector2(0f, 1f),
                                },
                                Children = new Drawable[]
                                {
                                    new CustomMenuHeader(),
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}