﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
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
        private readonly Container tabControlContainer;
        private readonly BindableWithCurrent<T> current = new BindableWithCurrent<T>();

        public Bindable<T> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        protected new float ContentSidePadding
        {
            get => base.ContentSidePadding;
            set
            {
                base.ContentSidePadding = value;
                tabControlContainer.Padding = new MarginPadding { Horizontal = value };
            }
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
                    tabControlContainer = new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Padding = new MarginPadding { Horizontal = ContentSidePadding },
                        Child = TabControl = CreateTabControl().With(control =>
                        {
                            control.Current = Current;
                        })
                    }
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
                    if (!(Value is Enum enumValue))
                        Text.Text = Value.ToString().ToLower();
                    else
                    {
                        var localisableDescription = enumValue.GetLocalisableDescription();
                        var nonLocalisableDescription = enumValue.GetDescription();

                        // If localisable == non-localisable, then we must have a basic string, so .ToLower() is used.
                        Text.Text = localisableDescription.Equals(nonLocalisableDescription)
                            ? nonLocalisableDescription.ToLower()
                            : localisableDescription;
                    }

                    Text.Font = OsuFont.GetFont(size: 14);
                    Text.Margin = new MarginPadding { Vertical = 16.5f }; // 15px padding + 1.5px line-height difference compensation
                    Bar.Margin = new MarginPadding { Bottom = bar_height };
                }
            }
        }
    }
}
