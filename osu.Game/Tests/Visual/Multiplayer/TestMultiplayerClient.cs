// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets.Mods;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestMultiplayerClient : StatefulMultiplayerClient
    {
        public override IBindable<bool> IsConnected => isConnected;
        private readonly Bindable<bool> isConnected = new Bindable<bool>(true);

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private BeatmapManager beatmaps { get; set; } = null!;

        private readonly TestMultiplayerRoomManager roomManager;

        public TestMultiplayerClient(TestMultiplayerRoomManager roomManager)
        {
            this.roomManager = roomManager;
        }

        public void Connect() => isConnected.Value = true;

        public void Disconnect() => isConnected.Value = false;

        public void AddUser(User user) => ((IMultiplayerClient)this).UserJoined(new MultiplayerRoomUser(user.Id) { User = user });

        public void AddNullUser(int userId) => ((IMultiplayerClient)this).UserJoined(new MultiplayerRoomUser(userId));

        public void RemoveUser(User user)
        {
            Debug.Assert(Room != null);
            ((IMultiplayerClient)this).UserLeft(new MultiplayerRoomUser(user.Id));

            Schedule(() =>
            {
                if (Room.Users.Any())
                    TransferHost(Room.Users.First().UserID);
            });
        }

        public void ChangeUserState(int userId, MultiplayerUserState newState)
        {
            Debug.Assert(Room != null);

            ((IMultiplayerClient)this).UserStateChanged(userId, newState);

            Schedule(() =>
            {
                switch (newState)
                {
                    case MultiplayerUserState.Loaded:
                        if (Room.Users.All(u => u.State != MultiplayerUserState.WaitingForLoad))
                        {
                            foreach (var u in Room.Users.Where(u => u.State == MultiplayerUserState.Loaded))
                                ChangeUserState(u.UserID, MultiplayerUserState.Playing);

                            ((IMultiplayerClient)this).MatchStarted();
                        }

                        break;

                    case MultiplayerUserState.FinishedPlay:
                        if (Room.Users.All(u => u.State != MultiplayerUserState.Playing))
                        {
                            foreach (var u in Room.Users.Where(u => u.State == MultiplayerUserState.FinishedPlay))
                                ChangeUserState(u.UserID, MultiplayerUserState.Results);

                            ((IMultiplayerClient)this).ResultsReady();
                        }

                        break;
                }
            });
        }

        public void ChangeUserBeatmapAvailability(int userId, BeatmapAvailability newBeatmapAvailability)
        {
            Debug.Assert(Room != null);

            ((IMultiplayerClient)this).UserBeatmapAvailabilityChanged(userId, newBeatmapAvailability);
        }

        protected override Task<MultiplayerRoom> JoinRoom(long roomId)
        {
            var apiRoom = roomManager.Rooms.Single(r => r.RoomID.Value == roomId);

            var user = new MultiplayerRoomUser(api.LocalUser.Value.Id)
            {
                User = api.LocalUser.Value
            };

            var room = new MultiplayerRoom(roomId)
            {
                Settings =
                {
                    Name = apiRoom.Name.Value,
                    BeatmapID = apiRoom.Playlist.Last().BeatmapID,
                    RulesetID = apiRoom.Playlist.Last().RulesetID,
                    BeatmapChecksum = apiRoom.Playlist.Last().Beatmap.Value.MD5Hash,
                    RequiredMods = apiRoom.Playlist.Last().RequiredMods.Select(m => new APIMod(m)).ToArray(),
                    AllowedMods = apiRoom.Playlist.Last().AllowedMods.Select(m => new APIMod(m)).ToArray(),
                    PlaylistItemId = apiRoom.Playlist.Last().ID
                },
                Users = { user },
                Host = user
            };

            return Task.FromResult(room);
        }

        protected override Task LeaveRoomInternal() => Task.CompletedTask;

        public override Task TransferHost(int userId) => ((IMultiplayerClient)this).HostChanged(userId);

        public override async Task ChangeSettings(MultiplayerRoomSettings settings)
        {
            Debug.Assert(Room != null);

            await ((IMultiplayerClient)this).SettingsChanged(settings).ConfigureAwait(false);

            foreach (var user in Room.Users.Where(u => u.State == MultiplayerUserState.Ready))
                ChangeUserState(user.UserID, MultiplayerUserState.Idle);
        }

        public override Task ChangeState(MultiplayerUserState newState)
        {
            ChangeUserState(api.LocalUser.Value.Id, newState);
            return Task.CompletedTask;
        }

        public override Task ChangeBeatmapAvailability(BeatmapAvailability newBeatmapAvailability)
        {
            ChangeUserBeatmapAvailability(api.LocalUser.Value.Id, newBeatmapAvailability);
            return Task.CompletedTask;
        }

        public void ChangeUserMods(int userId, IEnumerable<Mod> newMods)
            => ChangeUserMods(userId, newMods.Select(m => new APIMod(m)).ToList());

        public void ChangeUserMods(int userId, IEnumerable<APIMod> newMods)
        {
            Debug.Assert(Room != null);
            ((IMultiplayerClient)this).UserModsChanged(userId, newMods.ToList());
        }

        public override Task ChangeUserMods(IEnumerable<APIMod> newMods)
        {
            ChangeUserMods(api.LocalUser.Value.Id, newMods);
            return Task.CompletedTask;
        }

        public override Task StartMatch()
        {
            Debug.Assert(Room != null);

            foreach (var user in Room.Users.Where(u => u.State == MultiplayerUserState.Ready))
                ChangeUserState(user.UserID, MultiplayerUserState.WaitingForLoad);

            return ((IMultiplayerClient)this).LoadRequested();
        }

        protected override Task<BeatmapSetInfo> GetOnlineBeatmapSet(int beatmapId, CancellationToken cancellationToken = default)
        {
            Debug.Assert(Room != null);

            var apiRoom = roomManager.Rooms.Single(r => r.RoomID.Value == Room.RoomID);
            var set = apiRoom.Playlist.FirstOrDefault(p => p.BeatmapID == beatmapId)?.Beatmap.Value.BeatmapSet
                      ?? beatmaps.QueryBeatmap(b => b.OnlineBeatmapID == beatmapId)?.BeatmapSet;

            if (set == null)
                throw new InvalidOperationException("Beatmap not found.");

            return Task.FromResult(set);
        }
    }
}
