// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface.Tab;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Select.Filter;
using osu.Game.Screens.Select.Tab;
using Container = osu.Framework.Graphics.Containers.Container;

namespace osu.Game.Screens.Select
{
    public class FilterControl : Container
    {
        public Action FilterChanged;

        public string Search => searchTextBox.Text;
        private SortMode sort = SortMode.Title;
        public SortMode Sort { 
            get { return sort; } 
            set
            {
                if (sort != value)
                {
                    sort = value;
                    FilterChanged?.Invoke();
                }
            } 
        }

        public Action Exit;

        private SearchTextBox searchTextBox;

        public FilterControl(float height)
        {
            Children = new Drawable[]
            {
                new Box
                {
                    Colour = Color4.Black,
                    Alpha = 0.8f,
                    RelativeSizeAxes = Axes.X,
                    Height = height
                },
                new FillFlowContainer
                {
                    Padding = new MarginPadding(20),
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Width = 0.4f, // TODO: InnerWidth property or something
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        searchTextBox = new SearchTextBox
                        {
                            RelativeSizeAxes = Axes.X,
                            OnChange = (sender, newText) =>
                            {
                                if (newText)
                                    FilterChanged?.Invoke();
                            },
                            Exit = () => Exit?.Invoke(),
                        },
                        new GroupSortTabs()
                    }
                }
            };
        }

        public void Deactivate()
        {
            searchTextBox.HoldFocus = false;
            searchTextBox.TriggerFocusLost();
        }
        
        public void Activate()
        {
            searchTextBox.HoldFocus = true;
        }

        private class GroupSortTabs : Container
        {
            private TabControl<GroupMode> groupTabs;
            private TabControl<SortMode> sortTabs;
            private OsuSpriteText spriteText;

            public GroupSortTabs()
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 1,
                        Colour = OsuColour.Gray(80),
                        Origin = Anchor.TopLeft,
                        Anchor = Anchor.TopLeft,
                        Position = new Vector2(0, 23)
                    },
                    groupTabs = new FilterTabControl<GroupMode>(GroupMode.All, GroupMode.RecentlyPlayed)
                    {
                        Width = 230,
                        AutoSort = true
                    },
                    new Container
                    {
                        AutoSizeAxes = Axes.Both,
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        Children = new Drawable[]
                        {
                            spriteText = new OsuSpriteText
                            {
                                Font = @"Exo2.0-Bold",
                                Text = "Sort results by",
                                TextSize = 14,
                                Margin = new MarginPadding
                                {
                                    Top = 5,
                                    Bottom = 5
                                },
                            },
                            sortTabs = new FilterTabControl<SortMode>(87)
                            {
                                Width = 191,
                                AutoSort = true
                            }
                        }
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours) {
                spriteText.Colour = colours.GreenLight;
            }
        }
    }
}