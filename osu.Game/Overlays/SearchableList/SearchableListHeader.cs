﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Framework.Graphics.Shapes;

namespace osu.Game.Overlays.SearchableList
{
    public abstract class SearchableListHeader<T> : Container
    {
        private readonly Box tabStrip;

        public readonly HeaderTabControl<T> Tabs;

        protected abstract Color4 BackgroundColour { get; }
        protected abstract float TabStripWidth { get; } //can be removed once (if?) TabControl support auto sizing
        protected abstract T DefaultTab { get; }
        protected abstract Drawable CreateHeaderText();
        protected abstract FontAwesome Icon { get; }

        protected SearchableListHeader()
        {
            if (!typeof(T).IsEnum)
                throw new InvalidOperationException("BrowseHeader only supports enums as the generic type argument");

            RelativeSizeAxes = Axes.X;
            Height = 90;

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = BackgroundColour,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Left = SearchableListOverlay.WIDTH_PADDING, Right = SearchableListOverlay.WIDTH_PADDING },
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.BottomLeft,
                            Position = new Vector2(-35f, 5f),
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(10f, 0f),
                            Children = new[]
                            {
                                new SpriteIcon
                                {
                                    Size = new Vector2(25),
                                    Icon = Icon,
                                },
                                CreateHeaderText(),
                            },
                        },
                        tabStrip = new Box
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Width = TabStripWidth,
                            Height = 1,
                        },
                        Tabs = new HeaderTabControl<T>
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            RelativeSizeAxes = Axes.X,
                        },
                    },
                },
            };

            Tabs.Current.Value = DefaultTab;
            Tabs.Current.TriggerChange();
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            tabStrip.Colour = colours.Green;
        }
    }
}
