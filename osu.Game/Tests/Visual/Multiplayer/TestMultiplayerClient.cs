// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MessagePack;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Game.Online;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.TeamVersus;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Tests.Visual.Multiplayer
{
    /// <summary>
    /// A <see cref="MultiplayerClient"/> for use in multiplayer test scenes. Should generally not be used by itself outside of a <see cref="MultiplayerTestScene"/>.
    /// </summary>
    public partial class TestMultiplayerClient : MultiplayerClient
    {
        public override IBindable<bool> IsConnected => isConnected;
        private readonly Bindable<bool> isConnected = new Bindable<bool>(true);

        /// <summary>
        /// The local client's <see cref="Room"/>. This is not always equivalent to the server-side room.
        /// </summary>
        public Room? ClientAPIRoom => base.APIRoom;

        /// <summary>
        /// The local client's <see cref="MultiplayerRoom"/>. This is not always equivalent to the server-side room.
        /// </summary>
        public MultiplayerRoom? ClientRoom => base.Room;

        /// <summary>
        /// The server's <see cref="Room"/>. This is always up-to-date.
        /// </summary>
        public Room? ServerAPIRoom { get; private set; }

        /// <summary>
        /// The server's <see cref="MultiplayerRoom"/>. This is always up-to-date.
        /// </summary>
        public MultiplayerRoom? ServerRoom { get; private set; }

        [Obsolete]
        protected new Room APIRoom => throw new InvalidOperationException($"Accessing the client-side API room via {nameof(TestMultiplayerClient)} is unsafe. "
                                                                          + $"Use {nameof(ClientAPIRoom)} if this was intended.");

        [Obsolete]
        public new MultiplayerRoom Room => throw new InvalidOperationException($"Accessing the client-side room via {nameof(TestMultiplayerClient)} is unsafe. "
                                                                               + $"Use {nameof(ClientRoom)} if this was intended.");

        public new MultiplayerRoomUser? LocalUser => ServerRoom?.Users.SingleOrDefault(u => u.User?.Id == API.LocalUser.Value.Id);

        public Action<MultiplayerRoom>? RoomSetupAction;

        public bool RoomJoined { get; private set; }

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        private readonly TestMultiplayerRoomManager roomManager;

        private MultiplayerPlaylistItem? currentItem => ServerRoom?.Playlist[currentIndex];
        private int currentIndex;
        private long lastPlaylistItemId;

        public TestMultiplayerClient(TestMultiplayerRoomManager roomManager)
        {
            this.roomManager = roomManager;
        }

        public void Connect() => isConnected.Value = true;

        public void Disconnect() => isConnected.Value = false;

        public MultiplayerRoomUser AddUser(APIUser user, bool markAsPlaying = false)
            => AddUser(new MultiplayerRoomUser(user.Id) { User = user }, markAsPlaying);

        public MultiplayerRoomUser AddUser(MultiplayerRoomUser roomUser, bool markAsPlaying = false)
        {
            addUser(roomUser);

            if (markAsPlaying)
                PlayingUserIds.Add(roomUser.UserID);

            return roomUser;
        }

        public void TestAddUnresolvedUser() => addUser(new MultiplayerRoomUser(TestUserLookupCache.UNRESOLVED_USER_ID));

        private void addUser(MultiplayerRoomUser user)
        {
            Debug.Assert(ServerRoom != null);

            ServerRoom.Users.Add(user);
            ((IMultiplayerClient)this).UserJoined(clone(user)).WaitSafely();

            switch (ServerRoom?.MatchState)
            {
                case TeamVersusRoomState teamVersus:
                    // simulate the server's automatic assignment of users to teams on join.
                    // the "best" team is the one with the least users on it.
                    int bestTeam = teamVersus.Teams
                                             .Select(team => (teamID: team.ID, userCount: ServerRoom.Users.Count(u => (u.MatchState as TeamVersusUserState)?.TeamID == team.ID)))
                                             .MinBy(pair => pair.userCount).teamID;

                    user.MatchState = new TeamVersusUserState { TeamID = bestTeam };
                    ((IMultiplayerClient)this).MatchUserStateChanged(clone(user.UserID), clone(user.MatchState)).WaitSafely();
                    break;
            }
        }

        public void RemoveUser(APIUser user)
        {
            Debug.Assert(ServerRoom != null);

            ServerRoom.Users.Remove(ServerRoom.Users.Single(u => u.UserID == user.Id));
            ((IMultiplayerClient)this).UserLeft(clone(new MultiplayerRoomUser(user.Id)));

            if (ServerRoom.Users.Any())
                TransferHost(ServerRoom.Users.First().UserID);
        }

        public void ChangeRoomState(MultiplayerRoomState newState)
        {
            Debug.Assert(ServerRoom != null);

            ServerRoom.State = clone(newState);

            ((IMultiplayerClient)this).RoomStateChanged(clone(ServerRoom.State));
        }

        public void ChangeUserState(int userId, MultiplayerUserState newState)
        {
            Debug.Assert(ServerRoom != null);

            var user = ServerRoom.Users.Single(u => u.UserID == userId);
            user.State = clone(newState);

            ((IMultiplayerClient)this).UserStateChanged(clone(userId), clone(user.State));

            updateRoomStateIfRequired();
        }

        private void updateRoomStateIfRequired()
        {
            Debug.Assert(ServerRoom != null);

            switch (ServerRoom.State)
            {
                case MultiplayerRoomState.Open:
                    break;

                case MultiplayerRoomState.WaitingForLoad:
                    if (ServerRoom.Users.All(u => u.State != MultiplayerUserState.WaitingForLoad))
                    {
                        var loadedUsers = ServerRoom.Users.Where(u => u.State == MultiplayerUserState.Loaded).ToArray();

                        if (loadedUsers.Length == 0)
                        {
                            // all users have bailed from the load sequence. cancel the game start.
                            ChangeRoomState(MultiplayerRoomState.Open);
                            return;
                        }

                        foreach (var u in ServerRoom.Users.Where(u => u.State == MultiplayerUserState.Loaded))
                            ChangeUserState(u.UserID, MultiplayerUserState.Playing);

                        ((IMultiplayerClient)this).GameplayStarted();

                        ChangeRoomState(MultiplayerRoomState.Playing);
                    }

                    break;

                case MultiplayerRoomState.Playing:
                    if (ServerRoom.Users.All(u => u.State != MultiplayerUserState.Playing))
                    {
                        foreach (var u in ServerRoom.Users.Where(u => u.State == MultiplayerUserState.FinishedPlay))
                            ChangeUserState(u.UserID, MultiplayerUserState.Results);

                        ChangeRoomState(MultiplayerRoomState.Open);
                        ((IMultiplayerClient)this).ResultsReady();

                        FinishCurrentItem().WaitSafely();
                    }

                    break;
            }
        }

        public void ChangeUserBeatmapAvailability(int userId, BeatmapAvailability newBeatmapAvailability)
        {
            Debug.Assert(ServerRoom != null);

            var user = ServerRoom.Users.Single(u => u.UserID == userId);
            user.BeatmapAvailability = newBeatmapAvailability;

            ((IMultiplayerClient)this).UserBeatmapAvailabilityChanged(clone(userId), clone(user.BeatmapAvailability));
        }

        protected override async Task<MultiplayerRoom> JoinRoom(long roomId, string? password = null)
        {
            roomId = clone(roomId);
            password = clone(password);

            ServerAPIRoom = roomManager.ServerSideRooms.Single(r => r.RoomID.Value == roomId);

            if (password != ServerAPIRoom.Password.Value)
                throw new InvalidOperationException("Invalid password.");

            lastPlaylistItemId = ServerAPIRoom.Playlist.Max(item => item.ID);

            var localUser = new MultiplayerRoomUser(api.LocalUser.Value.Id)
            {
                User = api.LocalUser.Value
            };

            ServerRoom = new MultiplayerRoom(roomId)
            {
                Settings =
                {
                    Name = ServerAPIRoom.Name.Value,
                    MatchType = ServerAPIRoom.Type.Value,
                    Password = password,
                    QueueMode = ServerAPIRoom.QueueMode.Value,
                    AutoStartDuration = ServerAPIRoom.AutoStartDuration.Value
                },
                Playlist = ServerAPIRoom.Playlist.Select(CreateMultiplayerPlaylistItem).ToList(),
                Users = { localUser },
                Host = localUser
            };

            await updatePlaylistOrder(ServerRoom).ConfigureAwait(false);
            await updateCurrentItem(ServerRoom, false).ConfigureAwait(false);

            RoomSetupAction?.Invoke(ServerRoom);
            RoomSetupAction = null;

            return clone(ServerRoom);
        }

        protected override void OnRoomJoined()
        {
            Debug.Assert(ServerRoom != null);

            // emulate the server sending this after the join room. scheduler required to make sure the join room event is fired first (in Join).
            changeMatchType(ServerRoom.Settings.MatchType).WaitSafely();

            RoomJoined = true;
        }

        protected override Task LeaveRoomInternal()
        {
            RoomJoined = false;
            return Task.CompletedTask;
        }

        public override Task InvitePlayer(int userId)
        {
            return Task.CompletedTask;
        }

        public override Task TransferHost(int userId)
        {
            userId = clone(userId);

            Debug.Assert(ServerRoom != null);

            ServerRoom.Host = ServerRoom.Users.Single(u => u.UserID == userId);

            return ((IMultiplayerClient)this).HostChanged(clone(userId));
        }

        public override Task KickUser(int userId)
        {
            userId = clone(userId);

            Debug.Assert(ServerRoom != null);

            var user = ServerRoom.Users.Single(u => u.UserID == userId);
            ServerRoom.Users.Remove(user);

            return ((IMultiplayerClient)this).UserKicked(clone(user));
        }

        public override async Task ChangeSettings(MultiplayerRoomSettings settings)
        {
            settings = clone(settings);

            Debug.Assert(ServerRoom != null);
            Debug.Assert(currentItem != null);

            // Server is authoritative for the time being.
            settings.PlaylistItemId = ServerRoom.Settings.PlaylistItemId;
            ServerRoom.Settings = settings;

            await changeQueueMode(settings.QueueMode).ConfigureAwait(false);

            await ((IMultiplayerClient)this).SettingsChanged(clone(settings)).ConfigureAwait(false);

            foreach (var user in ServerRoom.Users.Where(u => u.State == MultiplayerUserState.Ready))
                ChangeUserState(user.UserID, MultiplayerUserState.Idle);

            await changeMatchType(settings.MatchType).ConfigureAwait(false);
            updateRoomStateIfRequired();
        }

        public override Task ChangeState(MultiplayerUserState newState)
        {
            newState = clone(newState);

            if (newState == MultiplayerUserState.Idle && LocalUser?.State == MultiplayerUserState.WaitingForLoad)
                return Task.CompletedTask;

            ChangeUserState(api.LocalUser.Value.Id, clone(newState));
            return Task.CompletedTask;
        }

        public override Task ChangeBeatmapAvailability(BeatmapAvailability newBeatmapAvailability)
        {
            ChangeUserBeatmapAvailability(api.LocalUser.Value.Id, clone(newBeatmapAvailability));
            return Task.CompletedTask;
        }

        public void ChangeUserMods(int userId, IEnumerable<Mod> newMods)
            => ChangeUserMods(userId, newMods.Select(m => new APIMod(m)));

        public void ChangeUserMods(int userId, IEnumerable<APIMod> newMods)
        {
            Debug.Assert(ServerRoom != null);

            var user = ServerRoom.Users.Single(u => u.UserID == userId);
            user.Mods = newMods.ToArray();

            ((IMultiplayerClient)this).UserModsChanged(clone(userId), clone(user.Mods));
        }

        public override Task ChangeUserMods(IEnumerable<APIMod> newMods)
        {
            ChangeUserMods(api.LocalUser.Value.Id, clone(newMods));
            return Task.CompletedTask;
        }

        public override async Task SendMatchRequest(MatchUserRequest request)
        {
            request = clone(request);

            Debug.Assert(ServerRoom != null);
            Debug.Assert(LocalUser != null);

            switch (request)
            {
                case ChangeTeamRequest changeTeam:

                    TeamVersusRoomState roomState = (TeamVersusRoomState)ServerRoom.MatchState!;
                    TeamVersusUserState userState = (TeamVersusUserState)LocalUser.MatchState!;

                    var targetTeam = roomState.Teams.FirstOrDefault(t => t.ID == changeTeam.TeamID);

                    if (targetTeam != null)
                    {
                        userState.TeamID = targetTeam.ID;

                        await ((IMultiplayerClient)this).MatchUserStateChanged(clone(LocalUser.UserID), clone(userState)).ConfigureAwait(false);
                    }

                    break;
            }
        }

        public override Task StartMatch()
        {
            Debug.Assert(ServerRoom != null);

            ChangeRoomState(MultiplayerRoomState.WaitingForLoad);
            foreach (var user in ServerRoom.Users.Where(u => u.State == MultiplayerUserState.Ready))
                ChangeUserState(user.UserID, MultiplayerUserState.WaitingForLoad);

            return ((IMultiplayerClient)this).LoadRequested();
        }

        public override Task AbortGameplay()
        {
            Debug.Assert(LocalUser != null);

            ChangeUserState(LocalUser.UserID, MultiplayerUserState.Idle);

            return Task.CompletedTask;
        }

        public override Task AbortMatch()
        {
            // Todo:
            return Task.CompletedTask;
        }

        public async Task AddUserPlaylistItem(int userId, MultiplayerPlaylistItem item)
        {
            Debug.Assert(ServerRoom != null);
            Debug.Assert(currentItem != null);

            if (ServerRoom.Settings.QueueMode == QueueMode.HostOnly && ServerRoom.Host?.UserID != LocalUser?.UserID)
                throw new InvalidOperationException("Local user is not the room host.");

            item.OwnerID = userId;

            await addItem(item).ConfigureAwait(false);
            await updateCurrentItem(ServerRoom).ConfigureAwait(false);
            updateRoomStateIfRequired();
        }

        public override Task AddPlaylistItem(MultiplayerPlaylistItem item) => AddUserPlaylistItem(api.LocalUser.Value.OnlineID, clone(item));

        public async Task EditUserPlaylistItem(int userId, MultiplayerPlaylistItem item)
        {
            Debug.Assert(ServerRoom != null);
            Debug.Assert(currentItem != null);
            Debug.Assert(ServerAPIRoom != null);

            item.OwnerID = userId;

            var existingItem = ServerRoom.Playlist.SingleOrDefault(i => i.ID == item.ID);

            if (existingItem == null)
                throw new InvalidOperationException("Attempted to change an item that doesn't exist.");

            if (existingItem.OwnerID != userId && ServerRoom.Host?.UserID != LocalUser?.UserID)
                throw new InvalidOperationException("Attempted to change an item which is not owned by the user.");

            if (existingItem.Expired)
                throw new InvalidOperationException("Attempted to change an item which has already been played.");

            // Ensure the playlist order doesn't change.
            item.PlaylistOrder = existingItem.PlaylistOrder;

            ServerRoom.Playlist[ServerRoom.Playlist.IndexOf(existingItem)] = item;
            ServerAPIRoom.Playlist[ServerAPIRoom.Playlist.IndexOf(ServerAPIRoom.Playlist.Single(i => i.ID == item.ID))] = new PlaylistItem(item);

            await ((IMultiplayerClient)this).PlaylistItemChanged(clone(item)).ConfigureAwait(false);
        }

        public override Task EditPlaylistItem(MultiplayerPlaylistItem item) => EditUserPlaylistItem(api.LocalUser.Value.OnlineID, clone(item));

        public async Task RemoveUserPlaylistItem(int userId, long playlistItemId)
        {
            Debug.Assert(ServerRoom != null);
            Debug.Assert(ServerAPIRoom != null);

            var item = ServerRoom.Playlist.FirstOrDefault(i => i.ID == playlistItemId);

            if (item == null)
                throw new InvalidOperationException("Item does not exist in the room.");

            if (item == currentItem)
                throw new InvalidOperationException("The room's current item cannot be removed.");

            if (item.OwnerID != userId)
                throw new InvalidOperationException("Attempted to remove an item which is not owned by the user.");

            if (item.Expired)
                throw new InvalidOperationException("Attempted to remove an item which has already been played.");

            ServerRoom.Playlist.Remove(item);
            ServerAPIRoom.Playlist.RemoveAll(i => i.ID == item.ID);
            await ((IMultiplayerClient)this).PlaylistItemRemoved(clone(playlistItemId)).ConfigureAwait(false);

            await updateCurrentItem(ServerRoom).ConfigureAwait(false);
            updateRoomStateIfRequired();
        }

        public override Task RemovePlaylistItem(long playlistItemId) => RemoveUserPlaylistItem(api.LocalUser.Value.OnlineID, clone(playlistItemId));

        private async Task changeMatchType(MatchType type)
        {
            Debug.Assert(ServerRoom != null);

            switch (type)
            {
                case MatchType.HeadToHead:
                    ServerRoom.MatchState = null;
                    await ((IMultiplayerClient)this).MatchRoomStateChanged(clone(ServerRoom.MatchState)).ConfigureAwait(false);

                    foreach (var user in ServerRoom.Users)
                    {
                        user.MatchState = null;
                        await ((IMultiplayerClient)this).MatchUserStateChanged(clone(user.UserID), clone(user.MatchState)).ConfigureAwait(false);
                    }

                    break;

                case MatchType.TeamVersus:
                    ServerRoom.MatchState = TeamVersusRoomState.CreateDefault();
                    await ((IMultiplayerClient)this).MatchRoomStateChanged(clone(ServerRoom.MatchState)).ConfigureAwait(false);

                    foreach (var user in ServerRoom.Users)
                    {
                        user.MatchState = new TeamVersusUserState();
                        await ((IMultiplayerClient)this).MatchUserStateChanged(clone(user.UserID), clone(user.MatchState)).ConfigureAwait(false);
                    }

                    break;
            }
        }

        private async Task changeQueueMode(QueueMode newMode)
        {
            Debug.Assert(ServerRoom != null);
            Debug.Assert(currentItem != null);

            // When changing to host-only mode, ensure that at least one non-expired playlist item exists by duplicating the current item.
            if (newMode == QueueMode.HostOnly && ServerRoom.Playlist.All(item => item.Expired))
                await duplicateCurrentItem().ConfigureAwait(false);

            await updatePlaylistOrder(ServerRoom).ConfigureAwait(false);
            await updateCurrentItem(ServerRoom).ConfigureAwait(false);
        }

        public async Task FinishCurrentItem()
        {
            Debug.Assert(ServerRoom != null);
            Debug.Assert(currentItem != null);

            // Expire the current playlist item.
            currentItem.Expired = true;
            currentItem.PlayedAt = DateTimeOffset.Now;

            await ((IMultiplayerClient)this).PlaylistItemChanged(clone(currentItem)).ConfigureAwait(false);
            await updatePlaylistOrder(ServerRoom).ConfigureAwait(false);

            // In host-only mode, a duplicate playlist item will be used for the next round.
            if (ServerRoom.Settings.QueueMode == QueueMode.HostOnly && ServerRoom.Playlist.All(item => item.Expired))
                await duplicateCurrentItem().ConfigureAwait(false);

            await updateCurrentItem(ServerRoom).ConfigureAwait(false);
        }

        private async Task duplicateCurrentItem()
        {
            Debug.Assert(currentItem != null);

            await addItem(new MultiplayerPlaylistItem
            {
                BeatmapID = currentItem.BeatmapID,
                BeatmapChecksum = currentItem.BeatmapChecksum,
                RulesetID = currentItem.RulesetID,
                RequiredMods = currentItem.RequiredMods,
                AllowedMods = currentItem.AllowedMods
            }).ConfigureAwait(false);
        }

        private async Task addItem(MultiplayerPlaylistItem item)
        {
            Debug.Assert(ServerRoom != null);
            Debug.Assert(ServerAPIRoom != null);

            item.ID = ++lastPlaylistItemId;

            ServerRoom.Playlist.Add(item);
            ServerAPIRoom.Playlist.Add(new PlaylistItem(item));
            await ((IMultiplayerClient)this).PlaylistItemAdded(clone(item)).ConfigureAwait(false);

            await updatePlaylistOrder(ServerRoom).ConfigureAwait(false);
        }

        private IEnumerable<MultiplayerPlaylistItem> upcomingItems => ServerRoom?.Playlist.Where(i => !i.Expired).OrderBy(i => i.PlaylistOrder) ?? Enumerable.Empty<MultiplayerPlaylistItem>();

        private async Task updateCurrentItem(MultiplayerRoom room, bool notify = true)
        {
            Debug.Assert(ServerRoom != null);

            // Pick the next non-expired playlist item by playlist order, or default to the most-recently-expired item.
            MultiplayerPlaylistItem nextItem = upcomingItems.FirstOrDefault() ?? ServerRoom.Playlist.OrderByDescending(i => i.PlayedAt).First();

            currentIndex = ServerRoom.Playlist.IndexOf(nextItem);

            long lastItem = room.Settings.PlaylistItemId;
            room.Settings.PlaylistItemId = nextItem.ID;

            if (notify && nextItem.ID != lastItem)
                await ((IMultiplayerClient)this).SettingsChanged(clone(room.Settings)).ConfigureAwait(false);
        }

        private async Task updatePlaylistOrder(MultiplayerRoom room)
        {
            Debug.Assert(ServerRoom != null);
            Debug.Assert(ServerAPIRoom != null);

            List<MultiplayerPlaylistItem> orderedActiveItems;

            switch (room.Settings.QueueMode)
            {
                default:
                    orderedActiveItems = ServerRoom.Playlist.Where(item => !item.Expired).OrderBy(item => item.ID).ToList();
                    break;

                case QueueMode.AllPlayersRoundRobin:
                    var itemsByPriority = new List<(MultiplayerPlaylistItem item, int priority)>();

                    // Assign a priority for items from each user, starting from 0 and increasing in order which the user added the items.
                    foreach (var group in ServerRoom.Playlist.Where(item => !item.Expired).OrderBy(item => item.ID).GroupBy(item => item.OwnerID))
                    {
                        int priority = 0;
                        itemsByPriority.AddRange(group.Select(item => (item, priority++)));
                    }

                    orderedActiveItems = itemsByPriority
                                         // Order by each user's priority.
                                         .OrderBy(i => i.priority)
                                         // Many users will have the same priority of items, so attempt to break the tie by maintaining previous ordering.
                                         // Suppose there are two users: User1 and User2. User1 adds two items, and then User2 adds a third. If the previous order is not maintained,
                                         // then after playing the first item by User1, their second item will become priority=0 and jump to the front of the queue (because it was added first).
                                         .ThenBy(i => i.item.PlaylistOrder)
                                         // If there are still ties (normally shouldn't happen), break ties by making items added earlier go first.
                                         // This could happen if e.g. the item orders get reset.
                                         .ThenBy(i => i.item.ID)
                                         .Select(i => i.item)
                                         .ToList();

                    break;
            }

            for (int i = 0; i < orderedActiveItems.Count; i++)
            {
                var item = orderedActiveItems[i];

                if (item.PlaylistOrder == i)
                    continue;

                item.PlaylistOrder = (ushort)i;

                await ((IMultiplayerClient)this).PlaylistItemChanged(clone(item)).ConfigureAwait(false);
            }

            // Also ensure that the API room's playlist is correct.
            foreach (var item in ServerAPIRoom.Playlist)
                item.PlaylistOrder = ServerRoom.Playlist.Single(i => i.ID == item.ID).PlaylistOrder;
        }

        private T clone<T>(T incoming)
        {
            byte[] serialized = MessagePackSerializer.Serialize(typeof(T), incoming, SignalRUnionWorkaroundResolver.OPTIONS);
            return MessagePackSerializer.Deserialize<T>(serialized, SignalRUnionWorkaroundResolver.OPTIONS);
        }

        public static MultiplayerPlaylistItem CreateMultiplayerPlaylistItem(PlaylistItem item) => new MultiplayerPlaylistItem
        {
            ID = item.ID,
            OwnerID = item.OwnerID,
            BeatmapID = item.Beatmap.OnlineID,
            BeatmapChecksum = item.Beatmap.MD5Hash,
            RulesetID = item.RulesetID,
            RequiredMods = item.RequiredMods.ToArray(),
            AllowedMods = item.AllowedMods.ToArray(),
            Expired = item.Expired,
            PlaylistOrder = item.PlaylistOrder ?? 0,
            PlayedAt = item.PlayedAt,
            StarRating = item.Beatmap.StarRating,
        };

        public override Task DisconnectInternal()
        {
            isConnected.Value = false;
            return Task.CompletedTask;
        }
    }
}
