// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.Rooms;
using osu.Game.Online.Rooms.RoomStatuses;
using osu.Game.Screens.OnlinePlay.Lounge.Components;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneRoomStatus : OsuTestScene
    {
        public TestSceneRoomStatus()
        {
            Child = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Both,
                Width = 0.5f,
                Children = new Drawable[]
                {
                    new DrawableRoom(new Room
                    {
                        Name = { Value = "Open - ending in 1 day" },
                        Status = { Value = new RoomStatusOpen() },
                        EndDate = { Value = DateTimeOffset.Now.AddDays(1) }
                    }) { MatchingFilter = true },
                    new DrawableRoom(new Room
                    {
                        Name = { Value = "Playing - ending in 1 day" },
                        Status = { Value = new RoomStatusPlaying() },
                        EndDate = { Value = DateTimeOffset.Now.AddDays(1) }
                    }) { MatchingFilter = true },
                    new DrawableRoom(new Room
                    {
                        Name = { Value = "Ended" },
                        Status = { Value = new RoomStatusEnded() },
                        EndDate = { Value = DateTimeOffset.Now }
                    }) { MatchingFilter = true },
                    new DrawableRoom(new Room
                    {
                        Name = { Value = "Open" },
                        Status = { Value = new RoomStatusOpen() },
                        Category = { Value = RoomCategory.Realtime }
                    }) { MatchingFilter = true },
                }
            };
        }
    }
}
