// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Rooms;
using osu.Game.Online.Rooms.RoomStatuses;
using osu.Game.Screens.OnlinePlay.Lounge.Components;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneDrawableRoom : OsuTestScene
    {
        public TestSceneDrawableRoom()
        {
            Child = new FillFlowContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Size = new Vector2(0.9f),
                Spacing = new Vector2(10),
                Children = new Drawable[]
                {
                    createDrawableRoom(new Room
                    {
                        Name = { Value = "Room 1" },
                        Status = { Value = new RoomStatusOpen() },
                        EndDate = { Value = DateTimeOffset.Now.AddDays(1) },
                    }),
                    createDrawableRoom(new Room
                    {
                        Name = { Value = "Room 2" },
                        Status = { Value = new RoomStatusPlaying() },
                        EndDate = { Value = DateTimeOffset.Now.AddDays(1) },
                    }),
                    createDrawableRoom(new Room
                    {
                        Name = { Value = "Room 3" },
                        Status = { Value = new RoomStatusEnded() },
                        EndDate = { Value = DateTimeOffset.Now },
                    }),
                    createDrawableRoom(new Room
                    {
                        Name = { Value = "Room 4 (realtime)" },
                        Status = { Value = new RoomStatusOpen() },
                        Category = { Value = RoomCategory.Realtime },
                    }),
                    createDrawableRoom(new Room
                    {
                        Name = { Value = "Room 4 (spotlight)" },
                        Status = { Value = new RoomStatusOpen() },
                        Category = { Value = RoomCategory.Spotlight },
                    }),
                }
            };
        }

        private DrawableRoom createDrawableRoom(Room room)
        {
            room.Host.Value ??= new User { Username = "peppy", Id = 2 };

            if (room.RecentParticipants.Count == 0)
            {
                room.RecentParticipants.AddRange(Enumerable.Range(0, 20).Select(i => new User
                {
                    Id = i,
                    Username = $"User {i}"
                }));
            }

            var drawableRoom = new DrawableRoom(room) { MatchingFilter = true };
            drawableRoom.Action = () => drawableRoom.State = drawableRoom.State == SelectionState.Selected ? SelectionState.NotSelected : SelectionState.Selected;

            return drawableRoom;
        }
    }
}
