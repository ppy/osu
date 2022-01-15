// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osuTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Framework.Graphics.Shapes;

namespace osu.Game.Screens.Select
{
    public class BeatmapDetailAreaTabControl : Container
    {
        public const float HEIGHT = 24;

        public Bindable<BeatmapDetailAreaTabItem> Current
        {
            get => tabs.Current;
            set => tabs.Current = value;
        }

        public Bindable<bool> CurrentModsFilter
        {
            get => modsCheckbox.Current;
            set => modsCheckbox.Current = value;
        }

        public Action<BeatmapDetailAreaTabItem, bool> OnFilter; // passed the selected tab and if mods is checked

        public IReadOnlyList<BeatmapDetailAreaTabItem> TabItems
        {
            get => tabs.Items;
            set => tabs.Items = value;
        }

        private readonly OsuTabControlCheckbox modsCheckbox;
        private readonly OsuTabControl<BeatmapDetailAreaTabItem> tabs;
        private readonly Container tabsContainer;

        public BeatmapDetailAreaTabControl()
        {
            Height = HEIGHT;

            Children = new Drawable[]
            {
                new Box
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.X,
                    Height = 1,
                    Colour = Color4.White.Opacity(0.2f),
                },
                tabsContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = tabs = new OsuTabControl<BeatmapDetailAreaTabItem>
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        RelativeSizeAxes = Axes.Both,
                        IsSwitchable = true,
                    },
                },
                modsCheckbox = new OsuTabControlCheckbox
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    Text = @"Selected Mods",
                    Alpha = 0,
                },
            };

            tabs.Current.ValueChanged += _ => invokeOnFilter();
            modsCheckbox.Current.ValueChanged += _ => invokeOnFilter();
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colour)
        {
            modsCheckbox.AccentColour = tabs.AccentColour = colour.YellowLight;
        }

        private void invokeOnFilter()
        {
            OnFilter?.Invoke(tabs.Current.Value, modsCheckbox.Current.Value);

            if (tabs.Current.Value.FilterableByMods)
            {
                modsCheckbox.FadeTo(1, 200, Easing.OutQuint);
                tabsContainer.Padding = new MarginPadding { Right = 100 };
            }
            else
            {
                modsCheckbox.FadeTo(0, 200, Easing.OutQuint);
                tabsContainer.Padding = new MarginPadding();
            }
        }
    }
}
