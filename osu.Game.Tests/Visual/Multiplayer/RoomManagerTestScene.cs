// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Screens.OnlinePlay;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public abstract class RoomManagerTestScene : RoomTestScene
    {
        [Cached(Type = typeof(IRoomManager))]
        protected TestRoomManager RoomManager { get; } = new TestRoomManager();

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("clear rooms", () => RoomManager.Rooms.Clear());
        }

        protected void AddRooms(int count, RulesetInfo ruleset = null)
        {
            AddStep("add rooms", () =>
            {
                for (int i = 0; i < count; i++)
                {
                    var room = new Room
                    {
                        RoomID = { Value = i },
                        Name = { Value = $"Room {i}" },
                        Host = { Value = new User { Username = "Host" } },
                        EndDate = { Value = DateTimeOffset.Now + TimeSpan.FromSeconds(10) },
                        Category = { Value = i % 2 == 0 ? RoomCategory.Spotlight : RoomCategory.Normal }
                    };

                    if (ruleset != null)
                    {
                        room.Playlist.Add(new PlaylistItem
                        {
                            Ruleset = { Value = ruleset },
                            Beatmap =
                            {
                                Value = new BeatmapInfo
                                {
                                    Metadata = new BeatmapMetadata()
                                }
                            }
                        });
                    }

                    RoomManager.Rooms.Add(room);
                }
            });
        }
    }
}
