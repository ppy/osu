// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osuTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Framework.Graphics.Shapes;

namespace osu.Game.Overlays.SearchableList
{
    public abstract class SearchableListFilterControl<TTab, TCategory> : Container
        where TTab : struct, Enum
        where TCategory : struct, Enum
    {
        private const float padding = 10;

        private readonly Drawable filterContainer;
        private readonly Drawable rightFilterContainer;
        private readonly Box tabStrip;

        public readonly SearchTextBox Search;
        public readonly PageTabControl<TTab> Tabs;
        public readonly SlimEnumDropdown<TCategory> Dropdown;
        public readonly DisplayStyleControl DisplayStyleControl;

        protected abstract Color4 BackgroundColour { get; }
        protected abstract TTab DefaultTab { get; }
        protected abstract TCategory DefaultCategory { get; }
        protected virtual Drawable CreateSupplementaryControls() => null;

        /// <summary>
        /// The amount of padding added to content (does not affect background or tab control strip).
        /// </summary>
        protected virtual float ContentHorizontalPadding => SearchableListOverlay.WIDTH_PADDING;

        protected SearchableListFilterControl()
        {
            RelativeSizeAxes = Axes.X;

            var controls = CreateSupplementaryControls();
            Container controlsContainer;
            Children = new[]
            {
                filterContainer = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = BackgroundColour,
                            Alpha = 0.9f,
                        },
                        tabStrip = new Box
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            RelativeSizeAxes = Axes.X,
                            Height = 1,
                        },
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Padding = new MarginPadding
                            {
                                Top = padding,
                                Horizontal = ContentHorizontalPadding
                            },
                            Children = new Drawable[]
                            {
                                Search = new FilterSearchTextBox
                                {
                                    RelativeSizeAxes = Axes.X,
                                },
                                controlsContainer = new Container
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Margin = new MarginPadding { Top = controls != null ? padding : 0 },
                                },
                                new Container
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Padding = new MarginPadding { Right = 225 },
                                    Child = Tabs = new PageTabControl<TTab>
                                    {
                                        RelativeSizeAxes = Axes.X,
                                    },
                                },
                                new Box // keep the tab strip part of autosize, but don't put it in the flow container
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Height = 1,
                                    Colour = Color4.White.Opacity(0),
                                },
                            },
                        },
                    },
                },
                rightFilterContainer = new FillFlowContainer
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    AutoSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        Dropdown = new SlimEnumDropdown<TCategory>
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            RelativeSizeAxes = Axes.None,
                            Width = 160f,
                        },
                        DisplayStyleControl = new DisplayStyleControl
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                        },
                    }
                }
            };

            if (controls != null) controlsContainer.Children = new[] { controls };

            Tabs.Current.Value = DefaultTab;
            Tabs.Current.TriggerChange();

            Dropdown.Current.Value = DefaultCategory;
            Dropdown.Current.TriggerChange();
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            tabStrip.Colour = colours.Yellow;
        }

        protected override void Update()
        {
            base.Update();

            Height = filterContainer.Height;
            rightFilterContainer.Margin = new MarginPadding { Top = filterContainer.Height - 30, Right = ContentHorizontalPadding };
        }

        private class FilterSearchTextBox : SearchTextBox
        {
            [BackgroundDependencyLoader]
            private void load()
            {
                BackgroundUnfocused = OsuColour.Gray(0.06f);
                BackgroundFocused = OsuColour.Gray(0.12f);
            }
        }
    }
}
