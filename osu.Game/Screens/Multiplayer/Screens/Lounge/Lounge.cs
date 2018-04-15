// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Screens;
using osu.Game.Online.Multiplayer;
using osu.Game.Overlays.SearchableList;
using osu.Game.Screens.Multiplayer.Components;
using OpenTK;

namespace osu.Game.Screens.Multiplayer.Screens.Lounge
{
    public class Lounge : MultiplayerScreen
    {
        private readonly FilterControl filter;
        private readonly Container content;
        private readonly SearchContainer search;
        private readonly RoomsFilterContainer roomsContainer;
        private readonly RoomInspector roomInspector;

        protected override Container<Drawable> TransitionContent => content;

        public override string Title => "lounge";
        public override string Name => "Lounge";

        private IEnumerable<Room> rooms;
        public IEnumerable<Room> Rooms
        {
            get { return rooms; }
            set
            {
                if (Equals(value, rooms)) return;
                rooms = value;

                roomsContainer.Children = Rooms.Select(r => new DrawableRoom(r)
                {
                    Action = select,
                }).ToList();

                if (!Rooms.Contains(roomInspector.Room))
                {
                    roomInspector.Room = null;
                }

                filterRooms();
            }
        }

        public Lounge()
        {
            Children = new Drawable[]
            {
                filter = new FilterControl(),
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
                                Child = roomsContainer = new RoomsFilterContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Vertical,
                                    Spacing = new Vector2(10 - DrawableRoom.SELECTION_BORDER_WIDTH * 2),
                                },
                            },
                        },
                        roomInspector = new RoomInspector
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            RelativeSizeAxes = Axes.Both,
                            Width = 0.45f,
                        },
                    },
                },
            };

            filter.Search.Current.ValueChanged += s => filterRooms();
            filter.Tabs.Current.ValueChanged += t => filterRooms();
            filter.Search.Exit += Exit;
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            content.Padding = new MarginPadding
            {
                Top = filter.DrawHeight,
                Left = SearchableListOverlay.WIDTH_PADDING - DrawableRoom.SELECTION_BORDER_WIDTH,
                Right = SearchableListOverlay.WIDTH_PADDING,
            };
        }

        protected override void OnFocus(InputState state)
        {
            GetContainingInputManager().ChangeFocus(filter.Search);
        }

        protected override void OnEntering(Screen last)
        {
            base.OnEntering(last);
            filter.Search.HoldFocus = true;
        }

        protected override bool OnExiting(Screen next)
        {
            filter.Search.HoldFocus = false;
            return base.OnExiting(next);
        }

        protected override void OnResuming(Screen last)
        {
            base.OnResuming(last);
            filter.Search.HoldFocus = true;
        }

        protected override void OnSuspending(Screen next)
        {
            base.OnSuspending(next);
            filter.Search.HoldFocus = false;
        }

        private void filterRooms()
        {
            search.SearchTerm = filter.Search.Current.Value ?? string.Empty;

            foreach (DrawableRoom room in roomsContainer.Children)
            {
                room.MatchingFilter = room.MatchingFilter && room.Room.Availability.Value == (RoomAvailability)filter.Tabs.Current.Value;
            }
        }

        private void select(DrawableRoom room)
        {
            var lastState = room.State;
            roomsContainer.Children.ForEach(c => c.State = Visibility.Hidden);
            room.State = Visibility.Visible;
            roomInspector.Room = room.Room;

            // open the room if its selected and is clicked again
            if (lastState == Visibility.Visible)
            {
                Push(new Match.Match(room.Room));
            }
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
