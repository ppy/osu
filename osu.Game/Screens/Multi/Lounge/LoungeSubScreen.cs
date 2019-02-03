// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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
        public override string Title => "Lounge";

        protected readonly FilterControl Filter;

        private readonly Container content;
        private readonly RoomsContainer rooms;
        private readonly Action<Screen> pushGameplayScreen;
        private readonly ProcessingOverlay processingOverlay;

        public LoungeSubScreen(Action<Screen> pushGameplayScreen)
        {
            this.pushGameplayScreen = pushGameplayScreen;

            RoomInspector inspector;

            InternalChildren = new Drawable[]
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
            Filter.Search.Exit += this.Exit;
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            content.Padding = new MarginPadding
            {
                Top = Filter.DrawHeight,
                Left = SearchableListOverlay.WIDTH_PADDING - DrawableRoom.SELECTION_BORDER_WIDTH + OsuScreen.HORIZONTAL_OVERFLOW_PADDING,
                Right = SearchableListOverlay.WIDTH_PADDING + OsuScreen.HORIZONTAL_OVERFLOW_PADDING,
            };
        }

        protected override void OnFocus(FocusEvent e)
        {
            Filter.Search.TakeFocus();
        }

        public override void OnEntering(IScreen last)
        {
            base.OnEntering(last);
            Filter.Search.HoldFocus = true;
        }

        public override bool OnExiting(IScreen next)
        {
            Filter.Search.HoldFocus = false;
            return base.OnExiting(next);
        }

        public override void OnSuspending(IScreen next)
        {
            base.OnSuspending(next);
            Filter.Search.HoldFocus = false;
        }

        private void filterRooms()
        {
            rooms.Filter(Filter.CreateCriteria());
            Manager?.Filter(Filter.CreateCriteria());
        }

        private void joinRequested(Room room)
        {
            processingOverlay.Show();
            Manager?.JoinRoom(room, r =>
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
            if (!this.IsCurrentScreen())
                return;

            this.Push(new MatchSubScreen(room, s => pushGameplayScreen?.Invoke(s)));
        }
    }
}
