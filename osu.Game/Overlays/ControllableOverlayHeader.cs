// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osuTK.Graphics;

namespace osu.Game.Overlays
{
    /// <summary>
    /// <see cref="OverlayHeader"/> which contains <see cref="TabControl{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of item to be represented by tabs in <see cref="TabControl{T}"/>.</typeparam>
    public abstract class ControllableOverlayHeader<T> : OverlayHeader
    {
        private readonly Box controlBackground;

        protected ControllableOverlayHeader(OverlayColourScheme colourScheme)
            : base(colourScheme)
        {
            HeaderInfo.Add(new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Children = new Drawable[]
                {
                    controlBackground = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Gray,
                    },
                    CreateTabControl().With(control => control.Margin = new MarginPadding { Left = UserProfileOverlay.CONTENT_X_MARGIN })
                }
            });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            controlBackground.Colour = colours.ForOverlayElement(ColourScheme, 0.2f, 0.2f);
        }

        protected abstract TabControl<T> CreateTabControl();
    }
}
