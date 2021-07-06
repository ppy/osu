// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Rooms;
using osu.Game.Online.Rooms.RoomStatuses;
using osu.Game.Screens.OnlinePlay.Lounge.Components;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneDrawableRoom : OsuTestScene
    {
        public TestSceneDrawableRoom()
        {
            Child = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Both,
                Width = 0.5f,
                Children = new Drawable[]
                {
                    createDrawableRoom(new Room
                    {
                        Name = { Value = "Open - ending in 1 day" },
                        Status = { Value = new RoomStatusOpen() },
                        EndDate = { Value = DateTimeOffset.Now.AddDays(1) }
                    }),
                    createDrawableRoom(new Room
                    {
                        Name = { Value = "Playing - ending in 1 day" },
                        Status = { Value = new RoomStatusPlaying() },
                        EndDate = { Value = DateTimeOffset.Now.AddDays(1) }
                    }),
                    createDrawableRoom(new Room
                    {
                        Name = { Value = "Ended" },
                        Status = { Value = new RoomStatusEnded() },
                        EndDate = { Value = DateTimeOffset.Now }
                    }),
                    createDrawableRoom(new Room
                    {
                        Name = { Value = "Open" },
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
