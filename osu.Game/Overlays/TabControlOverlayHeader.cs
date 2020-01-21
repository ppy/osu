// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Overlays
{
    /// <summary>
    /// <see cref="OverlayHeader"/> which contains <see cref="OsuTabControl{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of item to be represented by tabs in <see cref="OsuTabControl{T}"/>.</typeparam>
    public abstract class TabControlOverlayHeader<T> : OverlayHeader
    {
        protected OsuTabControl<T> TabControl;

        private readonly Box controlBackground;

        protected TabControlOverlayHeader(OverlayColourScheme colourScheme)
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
                    },
                    TabControl = CreateTabControl().With(control => control.Margin = new MarginPadding { Left = UserProfileOverlay.CONTENT_X_MARGIN })
                }
            });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            TabControl.AccentColour = colours.ForOverlayElement(ColourScheme, 1, 0.75f);
            controlBackground.Colour = colours.ForOverlayElement(ColourScheme, 0.2f, 0.2f);
        }

        [NotNull]
        protected virtual OsuTabControl<T> CreateTabControl() => new OverlayHeaderTabControl();

        public class OverlayHeaderTabControl : OverlayTabControl<T>
        {
            public OverlayHeaderTabControl()
            {
                BarHeight = 1;
                RelativeSizeAxes = Axes.None;
                AutoSizeAxes = Axes.X;
                Anchor = Anchor.BottomLeft;
                Origin = Anchor.BottomLeft;
                Height = 35;
            }

            protected override TabItem<T> CreateTabItem(T value) => new OverlayHeaderTabItem(value);

            protected override TabFillFlowContainer CreateTabFlow() => new TabFillFlowContainer
            {
                RelativeSizeAxes = Axes.Y,
                AutoSizeAxes = Axes.X,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(5, 0),
            };

            private class OverlayHeaderTabItem : OverlayTabItem
            {
                public OverlayHeaderTabItem(T value)
                    : base(value)
                {
                    Text.Text = value.ToString().ToLowerInvariant();
                    Text.Font = OsuFont.GetFont(size: 14);
                    Bar.ExpandedSize = 5;
                }
            }
        }
    }
}
