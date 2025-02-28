// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Screens.OnlinePlay.Components;

namespace osu.Game.Tests.Visual.OnlinePlay
{
    /// <summary>
    /// A very simple <see cref="RoomManager"/> for use in online play test scenes.
    /// </summary>
    public partial class TestRoomManager : RoomManager
    {
        private int currentRoomId;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        public void AddRooms(int count, RulesetInfo? ruleset = null, bool withPassword = false, bool withSpotlightRooms = false)
        {
            // Can't reference Osu ruleset project here.
            ruleset ??= rulesets.GetRuleset(0)!;

            for (int i = 0; i < count; i++)
            {
                AddRoom(new Room
                {
                    Name = $@"Room {currentRoomId}",
                    Host = new APIUser { Username = @"Host" },
                    Duration = TimeSpan.FromSeconds(10),
                    Category = withSpotlightRooms && i % 2 == 0 ? RoomCategory.Spotlight : RoomCategory.Normal,
                    Password = withPassword ? @"password" : null,
                    PlaylistItemStats = new Room.RoomPlaylistItemStats { RulesetIDs = [ruleset.OnlineID] },
                    Playlist = [new PlaylistItem(new BeatmapInfo { Metadata = new BeatmapMetadata() }) { RulesetID = ruleset.OnlineID }]
                });
            }
        }

        public void AddRoom(Room room)
        {
            room.RoomID = -currentRoomId;

            var req = new CreateRoomRequest(room);
            req.Success += AddOrUpdateRoom;
            api.Queue(req);

            currentRoomId++;
        }
    }
}
