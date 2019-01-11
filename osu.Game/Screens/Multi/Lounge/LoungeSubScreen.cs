﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Screens;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Multiplayer;
using osu.Game.Overlays.SearchableList;
using osu.Game.Screens.Multi.Lounge.Components;
using osu.Game.Screens.Multi.Match;

namespace osu.Game.Screens.Multi.Lounge
{
    public class LoungeSubScreen : MultiplayerSubScreen
    {
        protected readonly FilterControl Filter;

        private readonly Container content;
        private readonly RoomsContainer rooms;
        private readonly Action<Screen> pushGameplayScreen;
        private readonly ProcessingOverlay processingOverlay;

        [Resolved(CanBeNull = true)]
        private IRoomManager roomManager { get; set; }

        public override string Title => "Lounge";

        protected override Drawable TransitionContent => content;

        public LoungeSubScreen(Action<Screen> pushGameplayScreen)
        {
            this.pushGameplayScreen = pushGameplayScreen;

            RoomInspector inspector;

            Children = new Drawable[]
            {
                Filter = new FilterControl { Depth = -1 },
                content = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Width = 0.55f,
                            Children = new Drawable[]
                            {
                                new ScrollContainer
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    ScrollbarOverlapsContent = false,
                                    Padding = new MarginPadding(10),
                                    Child = new SearchContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Child = rooms = new RoomsContainer { JoinRequested = joinRequested }
                                    },
                                },
                                processingOverlay = new ProcessingOverlay { Alpha = 0 }
                            }
                        },
                        inspector = new RoomInspector
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            RelativeSizeAxes = Axes.Both,
                            Width = 0.45f,
                        },
                    },
                },
            };

            inspector.Room.BindTo(rooms.SelectedRoom);

            Filter.Search.Current.ValueChanged += s => filterRooms();
            Filter.Tabs.Current.ValueChanged += t => filterRooms();
            Filter.Search.Exit += Exit;
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
            // no base call; don't animate
            return false;
        }

        protected override void OnSuspending(Screen next)
        {
            base.OnSuspending(next);
            Filter.Search.HoldFocus = false;
        }

        private void filterRooms()
        {
            rooms.Filter(Filter.CreateCriteria());
            roomManager?.Filter(Filter.CreateCriteria());
        }

        private void joinRequested(Room room)
        {
            processingOverlay.Show();
            roomManager?.JoinRoom(room, r =>
            {
                Push(room);
                processingOverlay.Hide();
            }, _ => processingOverlay.Hide());
        }

        /// <summary>
        /// Push a room as a new subscreen.
        /// </summary>
        public void Push(Room room)
        {
            // Handles the case where a room is clicked 3 times in quick succession
            if (!IsCurrentScreen)
                return;

            Push(new MatchSubScreen(room, s => pushGameplayScreen?.Invoke(s)));
        }
    }
}
