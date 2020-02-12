// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Screens;
using osu.Game.Graphics.Containers;
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
        private readonly ProcessingOverlay processingOverlay;

        [Resolved]
        private Bindable<Room> currentRoom { get; set; }

        public LoungeSubScreen()
        {
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
                                new OsuScrollContainer
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    ScrollbarOverlapsContent = false,
                                    Padding = new MarginPadding(10),
                                    Child = new SearchContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Child = new RoomsContainer { JoinRequested = joinRequested }
                                    },
                                },
                                processingOverlay = new ProcessingOverlay { Alpha = 0 }
                            }
                        },
                        new RoomInspector
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            RelativeSizeAxes = Axes.Both,
                            Width = 0.45f,
                        },
                    },
                },
            };
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            content.Padding = new MarginPadding
            {
                Top = Filter.DrawHeight,
                Left = SearchableListOverlay.WIDTH_PADDING - DrawableRoom.SELECTION_BORDER_WIDTH + HORIZONTAL_OVERFLOW_PADDING,
                Right = SearchableListOverlay.WIDTH_PADDING + HORIZONTAL_OVERFLOW_PADDING,
            };
        }

        protected override void OnFocus(FocusEvent e)
        {
            Filter.Search.TakeFocus();
        }

        public override void OnEntering(IScreen last)
        {
            base.OnEntering(last);

            onReturning();
        }

        public override void OnResuming(IScreen last)
        {
            base.OnResuming(last);

            if (currentRoom.Value?.RoomID.Value == null)
                currentRoom.Value = new Room();

            onReturning();
        }

        private void onReturning()
        {
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

        private void joinRequested(Room room)
        {
            processingOverlay.Show();
            RoomManager?.JoinRoom(room, r =>
            {
                Open(room);
                processingOverlay.Hide();
            }, _ => processingOverlay.Hide());
        }

        /// <summary>
        /// Push a room as a new subscreen.
        /// </summary>
        public void Open(Room room)
        {
            // Handles the case where a room is clicked 3 times in quick succession
            if (!this.IsCurrentScreen())
                return;

            currentRoom.Value = room;

            this.Push(new MatchSubScreen(room));
        }
    }
}
