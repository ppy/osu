// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Screens;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Multiplayer;
using osu.Game.Overlays.SearchableList;
using osu.Game.Screens.Multi.Components;
using osuTK;

namespace osu.Game.Screens.Multi.Screens.Lounge
{
    public class Lounge : MultiplayerScreen
    {
        private readonly Container content;
        private readonly SearchContainer search;
        private readonly RoomSettingsOverlay settings;

        protected readonly FilterControl Filter;
        protected readonly FillFlowContainer<DrawableRoom> RoomsContainer;
        protected readonly RoomInspector Inspector;

        public override string Title => "Lounge";

        protected override Container<Drawable> TransitionContent => content;

        public Lounge()
        {
            Children = new Drawable[]
            {
                Filter = new FilterControl { Depth = -1 },
                content = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new ScrollContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Width = 0.55f,
                            Padding = new MarginPadding
                            {
                                Vertical = 35 - DrawableRoom.SELECTION_BORDER_WIDTH,
                                Right = 20 - DrawableRoom.SELECTION_BORDER_WIDTH
                            },
                            Child = search = new SearchContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Child = RoomsContainer = new RoomsFilterContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Vertical,
                                    Spacing = new Vector2(10 - DrawableRoom.SELECTION_BORDER_WIDTH * 2),
                                },
                            },
                        },
                        Inspector = new RoomInspector
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            RelativeSizeAxes = Axes.Both,
                            Width = 0.45f,
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Child = settings = new RoomSettingsOverlay
                            {
                                RelativeSizeAxes = Axes.Both,
                                Height = 0.9f,
                                Room = new Room()
                            },
                        },
                    },
                }
            };

            Filter.Search.Current.ValueChanged += s => filterRooms();
            Filter.Tabs.Current.ValueChanged += t => filterRooms();
            Filter.Search.Exit += Exit;

            settings.Applied = () =>
            {
                var drawableRoom = addRoom(settings.Room);
                drawableRoom.State = SelectionState.Selected;
            };
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            content.Padding = new MarginPadding
            {
                Top = Filter.DrawHeight,
                Left = SearchableListOverlay.WIDTH_PADDING - DrawableRoom.SELECTION_BORDER_WIDTH,
                Right = SearchableListOverlay.WIDTH_PADDING,
            };
        }

        public IEnumerable<Room> Rooms
        {
            set
            {
                RoomsContainer.ForEach(r => r.Action = null);
                RoomsContainer.Clear();

                foreach (var room in value)
                    addRoom(room);
            }
        }

        private DrawableRoom addRoom(Room room)
        {
            var drawableRoom = new DrawableRoom(room);

            drawableRoom.StateChanged += s =>
            {
                if (s == SelectionState.Selected)
                    didSelect(drawableRoom);
            };

            RoomsContainer.Add(drawableRoom);

            filterRooms();

            return drawableRoom;
        }

        protected override void OnFocus(FocusEvent e)
        {
            GetContainingInputManager().ChangeFocus(Filter.Search);
        }

        protected override void OnEntering(Screen last)
        {
            base.OnEntering(last);
            Filter.Search.HoldFocus = true;
        }

        protected override bool OnExiting(Screen next)
        {
            Filter.Search.HoldFocus = false;
            return base.OnExiting(next);
        }

        protected override void OnResuming(Screen last)
        {
            base.OnResuming(last);

            settings.Room = new Room();

            Filter.Search.HoldFocus = true;
        }

        protected override void OnSuspending(Screen next)
        {
            base.OnSuspending(next);
            Filter.Search.HoldFocus = false;
        }

        private void filterRooms()
        {
            if (Filter.Tabs.Current.Value == LoungeTab.Create)
                settings.Show();
            else
                settings.Hide();

            search.SearchTerm = Filter.Search.Current.Value ?? string.Empty;

            foreach (DrawableRoom r in RoomsContainer.Children)
            {
                r.MatchingFilter = r.MatchingFilter && r.Room.Availability.Value == Filter.Availability;
            }
        }

        private void didSelect(DrawableRoom room)
        {
            RoomsContainer.Children.ForEach(c =>
            {
                if (c != room)
                    c.State = SelectionState.NotSelected;
            });

            Inspector.Room = room.Room;

            // open the room if its selected and is clicked again
            if (room.State == SelectionState.Selected)
                Push(new Match.Match(room.Room));
        }

        private class RoomsFilterContainer : FillFlowContainer<DrawableRoom>, IHasFilterableChildren
        {
            public IEnumerable<string> FilterTerms => new string[] { };
            public IEnumerable<IFilterable> FilterableChildren => Children;

            public bool MatchingFilter
            {
                set
                {
                    if (value)
                        InvalidateLayout();
                }
            }

            public RoomsFilterContainer()
            {
                LayoutDuration = 200;
                LayoutEasing = Easing.OutQuint;
            }
        }
    }
}
