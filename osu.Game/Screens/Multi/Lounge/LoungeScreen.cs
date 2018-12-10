// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Configuration;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Screens;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Multiplayer;
using osu.Game.Overlays.SearchableList;
using osu.Game.Screens.Multi.Lounge.Components;
using osu.Game.Screens.Multi.Match;
using osuTK;

namespace osu.Game.Screens.Multi.Lounge
{
    public class LoungeScreen : MultiplayerScreen
    {
        private readonly Container content;
        private readonly SearchContainer search;

        protected readonly FilterControl Filter;
        protected readonly FillFlowContainer<DrawableRoom> RoomsContainer;
        protected readonly RoomInspector Inspector;

        public override string Title => "Lounge";

        private Room roomBeingCreated;
        private readonly IBindable<bool> roomCreated = new Bindable<bool>();

        protected override Drawable TransitionContent => content;

        public LoungeScreen()
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
                    },
                }
            };

            Filter.Search.Current.ValueChanged += s => filterRooms();
            Filter.Tabs.Current.ValueChanged += t => filterRooms();
            Filter.Search.Exit += Exit;

            roomCreated.BindValueChanged(v =>
            {
                if (v)
                    addRoom(roomBeingCreated);
            });
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

            drawableRoom.Action = () => selectionRequested(drawableRoom);

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
            {
                roomBeingCreated = new Room();

                roomCreated.UnbindBindings();
                roomCreated.BindTo(roomBeingCreated.Created);

                Filter.Tabs.Current.Value = LoungeTab.Public;

                Push(new MatchScreen(roomBeingCreated));
            }

            search.SearchTerm = Filter.Search.Current.Value ?? string.Empty;

            foreach (DrawableRoom r in RoomsContainer.Children)
            {
                r.MatchingFilter = r.MatchingFilter && r.Room.Availability.Value == Filter.Availability;
            }
        }

        private void selectionRequested(DrawableRoom room)
        {
            if (room.State == SelectionState.Selected)
                openRoom(room);
            else
            {
                RoomsContainer.ForEach(c => c.State = c == room ? SelectionState.Selected : SelectionState.NotSelected);
                Inspector.Room = room.Room;
            }
        }

        private void openRoom(DrawableRoom room)
        {
            if (!IsCurrentScreen)
                return;

            RoomsContainer.ForEach(c => c.State = c == room ? SelectionState.Selected : SelectionState.NotSelected);
            Inspector.Room = room.Room;

            Push(new MatchScreen(room.Room));
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
