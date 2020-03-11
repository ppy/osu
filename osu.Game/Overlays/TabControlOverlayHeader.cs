// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
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
    /// An overlay header which contains a <see cref="OsuTabControl{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of item to be represented by tabs.</typeparam>
    public abstract class TabControlOverlayHeader<T> : OverlayHeader, IHasCurrentValue<T>
    {
        protected OsuTabControl<T> TabControl;

        private readonly BindableWithCurrent<T> current = new BindableWithCurrent<T>();

        public Bindable<T> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        private readonly Box controlBackground;

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
            TabControl.AccentColour = colourProvider.Highlight1;
            controlBackground.Colour = colourProvider.Dark4;
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
                    Text.Text = formatItemText(value);
                    Text.Font = OsuFont.GetFont(size: 14);
                    Bar.ExpandedSize = 5;
                }

                private string formatItemText(object value)
                {
                    string baseText;

                    switch (value)
                    {
                        case string stringValue:
                            baseText = stringValue;
                            break;

                        case Enum enumValue:
                            baseText = enumValue.GetDescription();
                            break;

                        default:
                            baseText = value.ToString();
                            break;
                    }

                    return baseText.ToLower();
                }
            }
        }
    }
}
