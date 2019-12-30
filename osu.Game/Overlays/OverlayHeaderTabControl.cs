﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.UserInterface;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;
using osu.Game.Graphics;
using System;

namespace osu.Game.Overlays
{
    public class OverlayHeaderTabControl<T> : OverlayTabControl<T>
    {
        public OverlayHeaderTabControl()
        {
            BarHeight = 1;
            RelativeSizeAxes = Axes.None;
            AutoSizeAxes = Axes.X;
            Anchor = Anchor.BottomLeft;
            Origin = Anchor.BottomLeft;
            Height = 35;

            if (typeof(T).IsEnum)
            {
                foreach (var val in (T[])Enum.GetValues(typeof(T)))
                    AddItem(val);
            }
        }

        protected override TabItem<T> CreateTabItem(T value) => new OverlayHeaderTabItem(value)
        {
            AccentColour = AccentColour,
        };

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
