// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.RoomStatuses;
using osu.Game.Screens.Multi.Lounge.Components;

namespace osu.Game.Tests.Visual
{
    public class TestCaseRoomStatus : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(RoomStatusEnded),
            typeof(RoomStatusOpen),
            typeof(RoomStatusPlaying)
        };

        public TestCaseRoomStatus()
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
                        Status = { Value = new RoomStatusOpen() }
                    }),
                    new DrawableRoom(new Room
                    {
                        Name = { Value = "Room 2" },
                        Status = { Value = new RoomStatusPlaying() }
                    }),
                    new DrawableRoom(new Room
                    {
                        Name = { Value = "Room 3" },
                        Status = { Value = new RoomStatusEnded() }
                    }),
                }
            };
        }
    }
}
