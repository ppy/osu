// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;

namespace osu.Game.Overlays
{
    public abstract class ControllableOverlayHeader<TModel, T> : OverlayHeader
        where TModel : TabControl<T>
    {
        protected readonly TModel TabControl;

        private readonly Box controlBackground;

        protected ControllableOverlayHeader(OverlayColourScheme colourScheme)
            : base(colourScheme)
        {
            HeaderInfo.Add(new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Depth = -float.MaxValue,
                Children = new Drawable[]
                {
                    controlBackground = new Box
                    {
                        RelativeSizeAxes = Axes.Both
                    },
                    TabControl = CreateControl().With(control => control.Margin = new MarginPadding { Left = UserProfileOverlay.CONTENT_X_MARGIN })
                }
            });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            controlBackground.Colour = colours.ForOverlayElement(ColourScheme, 0.2f, 0.2f);
        }

        protected abstract TModel CreateControl();
    }
}
