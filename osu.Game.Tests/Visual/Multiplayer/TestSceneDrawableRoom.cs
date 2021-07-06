// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Rooms;
using osu.Game.Online.Rooms.RoomStatuses;
using osu.Game.Screens.OnlinePlay.Lounge.Components;
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
                        Name = { Value = "Room name: Open - ending in 1 day" },
                        Status = { Value = new RoomStatusOpen() },
                        EndDate = { Value = DateTimeOffset.Now.AddDays(1) }
                    }),
                    createDrawableRoom(new Room
                    {
                        Name = { Value = "Room name: Playing - ending in 1 day" },
                        Status = { Value = new RoomStatusPlaying() },
                        EndDate = { Value = DateTimeOffset.Now.AddDays(1) }
                    }),
                    createDrawableRoom(new Room
                    {
                        Name = { Value = "Room name: Ended" },
                        Status = { Value = new RoomStatusEnded() },
                        EndDate = { Value = DateTimeOffset.Now }
                    }),
                    createDrawableRoom(new Room
                    {
                        Name = { Value = "Room name: Open" },
                        Status = { Value = new RoomStatusOpen() },
                        Category = { Value = RoomCategory.Realtime }
                    }),
                }
            };
        }

        private DrawableRoom createDrawableRoom(Room room)
        {
            var drawableRoom = new DrawableRoom(room)
            {
                MatchingFilter = true
            };

            drawableRoom.Action = () => drawableRoom.State = drawableRoom.State == SelectionState.Selected ? SelectionState.NotSelected : SelectionState.Selected;

            return drawableRoom;
        }
    }
}
