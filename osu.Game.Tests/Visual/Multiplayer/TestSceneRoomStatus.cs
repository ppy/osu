// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.RoomStatuses;
using osu.Game.Screens.Multi.Lounge.Components;

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
                        Name = { Value = "Room 1" },
                        Status = { Value = new RoomStatusOpen() },
                        EndDate = { Value = DateTimeOffset.Now.AddDays(1) }
                    }) { MatchingFilter = true },
                    new DrawableRoom(new Room
                    {
                        Name = { Value = "Room 2" },
                        Status = { Value = new RoomStatusPlaying() },
                        EndDate = { Value = DateTimeOffset.Now.AddDays(1) }
                    }) { MatchingFilter = true },
                    new DrawableRoom(new Room
                    {
                        Name = { Value = "Room 3" },
                        Status = { Value = new RoomStatusEnded() },
                        EndDate = { Value = DateTimeOffset.Now }
                    }) { MatchingFilter = true },
                }
            };
        }
    }
}
