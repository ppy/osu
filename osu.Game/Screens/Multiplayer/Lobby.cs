// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.Multiplayer;
using osu.Game.Overlays.SearchableList;
using OpenTK;

namespace osu.Game.Screens.Multiplayer
{
    public class Lobby : MultiplayerScreen
    {
        private readonly FillFlowContainer<DrawableRoom> roomsContainer;
        private readonly RoomInspector roomInspector;

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

                roomsContainer.Children = Rooms.Select(r => new DrawableRoom(r)).ToList();
            }
        }

        public Lobby()
        {
            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Horizontal = SearchableListOverlay.WIDTH_PADDING },
                    Children = new Drawable[]
                    {
                        new ScrollContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Width = 0.55f,
                            Padding = new MarginPadding { Vertical = 35, Right = 20 },
                            Child = roomsContainer = new FillFlowContainer<DrawableRoom>
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical,
                                Spacing = new Vector2(10),
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
        }
    }
}
