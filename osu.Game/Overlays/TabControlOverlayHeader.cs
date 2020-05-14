// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays
{
    /// <summary>
    /// An overlay header which contains a <see cref="OsuTabControl{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of item to be represented by tabs.</typeparam>
    public abstract class TabControlOverlayHeader<T> : OverlayHeader, IHasCurrentValue<T>
    {
        protected OsuTabControl<T> TabControl;

        private readonly Box controlBackground;
        private readonly BindableWithCurrent<T> current = new BindableWithCurrent<T>();

        public Bindable<T> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        protected TabControlOverlayHeader()
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
                    TabControl = CreateTabControl().With(control =>
                    {
                        control.Margin = new MarginPadding { Left = CONTENT_X_MARGIN };
                        control.Current = Current;
                    })
                }
            });
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            controlBackground.Colour = colourProvider.Dark4;
        }

        [NotNull]
        protected virtual OsuTabControl<T> CreateTabControl() => new OverlayHeaderTabControl();

        public class OverlayHeaderTabControl : OverlayTabControl<T>
        {
            private const float bar_height = 1;

            public OverlayHeaderTabControl()
            {
                RelativeSizeAxes = Axes.None;
                AutoSizeAxes = Axes.X;
                Anchor = Anchor.BottomLeft;
                Origin = Anchor.BottomLeft;
                Height = 47;
                BarHeight = bar_height;
            }

            protected override TabItem<T> CreateTabItem(T value) => new OverlayHeaderTabItem(value);

            protected override TabFillFlowContainer CreateTabFlow() => new TabFillFlowContainer
            {
                RelativeSizeAxes = Axes.Y,
                AutoSizeAxes = Axes.X,
                Direction = FillDirection.Horizontal,
            };

            private class OverlayHeaderTabItem : OverlayTabItem
            {
                public OverlayHeaderTabItem(T value)
                    : base(value)
                {
                    Text.Text = value.ToString().ToLower();
                    Text.Font = OsuFont.GetFont(size: 14);
                    Text.Margin = new MarginPadding { Vertical = 16.5f }; // 15px padding + 1.5px line-height difference compensation
                    Bar.Margin = new MarginPadding { Bottom = bar_height };
                }
            }
        }
    }
}
