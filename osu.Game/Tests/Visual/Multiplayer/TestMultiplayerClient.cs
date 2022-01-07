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
using osu.Framework.Extensions;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.TeamVersus;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets.Mods;
using APIUser = osu.Game.Online.API.Requests.Responses.APIUser;

namespace osu.Game.Tests.Visual.Multiplayer
{
    /// <summary>
    /// A <see cref="MultiplayerClient"/> for use in multiplayer test scenes. Should generally not be used by itself outside of a <see cref="MultiplayerTestScene"/>.
    /// </summary>
    public class TestMultiplayerClient : MultiplayerClient
    {
        public override IBindable<bool> IsConnected => isConnected;
        private readonly Bindable<bool> isConnected = new Bindable<bool>(true);

        public new Room? APIRoom => base.APIRoom;
        public Action<MultiplayerRoom>? RoomSetupAction;

        public bool RoomJoined { get; private set; }

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private BeatmapManager beatmaps { get; set; } = null!;

        private readonly TestMultiplayerRoomManager roomManager;

        /// <summary>
        /// Guaranteed up-to-date playlist.
        /// </summary>
        private readonly List<MultiplayerPlaylistItem> serverSidePlaylist = new List<MultiplayerPlaylistItem>();

        private MultiplayerPlaylistItem? currentItem => Room?.Playlist[currentIndex];
        private int currentIndex;
        private long lastPlaylistItemId;

        public TestMultiplayerClient(TestMultiplayerRoomManager roomManager)
        {
            this.roomManager = roomManager;
        }

        public void Connect() => isConnected.Value = true;

        public void Disconnect() => isConnected.Value = false;

        public MultiplayerRoomUser AddUser(APIUser user, bool markAsPlaying = false)
        {
            var roomUser = new MultiplayerRoomUser(user.Id) { User = user };

            addUser(roomUser);

            if (markAsPlaying)
                PlayingUserIds.Add(user.Id);

            return roomUser;
        }

        public void TestAddUnresolvedUser() => addUser(new MultiplayerRoomUser(TestUserLookupCache.UNRESOLVED_USER_ID));

        private void addUser(MultiplayerRoomUser user)
        {
            ((IMultiplayerClient)this).UserJoined(user).WaitSafely();

            // We want the user to be immediately available for testing, so force a scheduler update to run the update-bound continuation.
            Scheduler.Update();

            switch (Room?.MatchState)
            {
                case TeamVersusRoomState teamVersus:
                    Debug.Assert(Room != null);

                    // simulate the server's automatic assignment of users to teams on join.
                    // the "best" team is the one with the least users on it.
                    int bestTeam = teamVersus.Teams
                                             .Select(team => (teamID: team.ID, userCount: Room.Users.Count(u => (u.MatchState as TeamVersusUserState)?.TeamID == team.ID)))
                                             .OrderBy(pair => pair.userCount)
                                             .First().teamID;
                    ((IMultiplayerClient)this).MatchUserStateChanged(user.UserID, new TeamVersusUserState { TeamID = bestTeam }).WaitSafely();
                    break;
            }
        }

        public void RemoveUser(APIUser user)
        {
            Debug.Assert(Room != null);
            ((IMultiplayerClient)this).UserLeft(new MultiplayerRoomUser(user.Id));

            Schedule(() =>
            {
                if (Room.Users.Any())
                    TransferHost(Room.Users.First().UserID);
            });
        }

        public void ChangeRoomState(MultiplayerRoomState newState)
        {
            Debug.Assert(Room != null);
            ((IMultiplayerClient)this).RoomStateChanged(newState);
        }

        public void ChangeUserState(int userId, MultiplayerUserState newState)
        {
            Debug.Assert(Room != null);
            ((IMultiplayerClient)this).UserStateChanged(userId, newState);

            Schedule(() =>
            {
                switch (Room.State)
                {
                    case MultiplayerRoomState.WaitingForLoad:
                        if (Room.Users.All(u => u.State != MultiplayerUserState.WaitingForLoad))
                        {
                            var loadedUsers = Room.Users.Where(u => u.State == MultiplayerUserState.Loaded).ToArray();

                            if (loadedUsers.Length == 0)
                            {
                                // all users have bailed from the load sequence. cancel the game start.
                                ChangeRoomState(MultiplayerRoomState.Open);
                                return;
                            }

                            foreach (var u in Room.Users.Where(u => u.State == MultiplayerUserState.Loaded))
                                ChangeUserState(u.UserID, MultiplayerUserState.Playing);

                            ((IMultiplayerClient)this).MatchStarted();

                            ChangeRoomState(MultiplayerRoomState.Playing);
                        }

                        break;

                    case MultiplayerRoomState.Playing:
                        if (Room.Users.All(u => u.State != MultiplayerUserState.Playing))
                        {
                            foreach (var u in Room.Users.Where(u => u.State == MultiplayerUserState.FinishedPlay))
                                ChangeUserState(u.UserID, MultiplayerUserState.Results);

                            ChangeRoomState(MultiplayerRoomState.Open);
                            ((IMultiplayerClient)this).ResultsReady();

                            FinishCurrentItem().WaitSafely();
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

        protected override async Task<MultiplayerRoom> JoinRoom(long roomId, string? password = null)
        {
            var apiRoom = roomManager.ServerSideRooms.Single(r => r.RoomID.Value == roomId);

            if (password != apiRoom.Password.Value)
                throw new InvalidOperationException("Invalid password.");

            serverSidePlaylist.Clear();
            serverSidePlaylist.AddRange(apiRoom.Playlist.Select(item => new MultiplayerPlaylistItem(item)));
            lastPlaylistItemId = serverSidePlaylist.Max(item => item.ID);

            var localUser = new MultiplayerRoomUser(api.LocalUser.Value.Id)
            {
                User = api.LocalUser.Value
            };

            var room = new MultiplayerRoom(roomId)
            {
                Settings =
                {
                    Name = apiRoom.Name.Value,
                    MatchType = apiRoom.Type.Value,
                    Password = password,
                    QueueMode = apiRoom.QueueMode.Value
                },
                Playlist = serverSidePlaylist.ToList(),
                Users = { localUser },
                Host = localUser
            };

            await updatePlaylistOrder(room).ConfigureAwait(false);
            await updateCurrentItem(room, false).ConfigureAwait(false);

            RoomSetupAction?.Invoke(room);
            RoomSetupAction = null;

            return room;
        }

        protected override void OnRoomJoined()
        {
            Debug.Assert(APIRoom != null);
            Debug.Assert(Room != null);

            // emulate the server sending this after the join room. scheduler required to make sure the join room event is fired first (in Join).
            changeMatchType(Room.Settings.MatchType).WaitSafely();

            RoomJoined = true;
        }

        protected override Task LeaveRoomInternal()
        {
            RoomJoined = false;
            return Task.CompletedTask;
        }

        public override Task TransferHost(int userId) => ((IMultiplayerClient)this).HostChanged(userId);

        public override Task KickUser(int userId)
        {
            Debug.Assert(Room != null);

            return ((IMultiplayerClient)this).UserKicked(Room.Users.Single(u => u.UserID == userId));
        }

        public override async Task ChangeSettings(MultiplayerRoomSettings settings)
        {
            Debug.Assert(Room != null);
            Debug.Assert(APIRoom != null);
            Debug.Assert(currentItem != null);

            // Server is authoritative for the time being.
            settings.PlaylistItemId = Room.Settings.PlaylistItemId;

            await changeQueueMode(settings.QueueMode).ConfigureAwait(false);

            await ((IMultiplayerClient)this).SettingsChanged(settings).ConfigureAwait(false);

            foreach (var user in Room.Users.Where(u => u.State == MultiplayerUserState.Ready))
                ChangeUserState(user.UserID, MultiplayerUserState.Idle);

            await changeMatchType(settings.MatchType).ConfigureAwait(false);
        }

        public override Task ChangeState(MultiplayerUserState newState)
        {
            Debug.Assert(Room != null);

            if (newState == MultiplayerUserState.Idle && LocalUser?.State == MultiplayerUserState.WaitingForLoad)
                return Task.CompletedTask;

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

        public override async Task SendMatchRequest(MatchUserRequest request)
        {
            Debug.Assert(Room != null);
            Debug.Assert(LocalUser != null);

            switch (request)
            {
                case ChangeTeamRequest changeTeam:

                    TeamVersusRoomState roomState = (TeamVersusRoomState)Room.MatchState!;
                    TeamVersusUserState userState = (TeamVersusUserState)LocalUser.MatchState!;

                    var targetTeam = roomState.Teams.FirstOrDefault(t => t.ID == changeTeam.TeamID);

                    if (targetTeam != null)
                    {
                        userState.TeamID = targetTeam.ID;

                        await ((IMultiplayerClient)this).MatchUserStateChanged(LocalUser.UserID, userState).ConfigureAwait(false);
                    }

                    break;
            }
        }

        public override Task StartMatch()
        {
            Debug.Assert(Room != null);

            ChangeRoomState(MultiplayerRoomState.WaitingForLoad);
            foreach (var user in Room.Users.Where(u => u.State == MultiplayerUserState.Ready))
                ChangeUserState(user.UserID, MultiplayerUserState.WaitingForLoad);

            return ((IMultiplayerClient)this).LoadRequested();
        }

        public override Task AbortGameplay()
        {
            Debug.Assert(Room != null);
            Debug.Assert(LocalUser != null);

            ChangeUserState(LocalUser.UserID, MultiplayerUserState.Idle);

            return Task.CompletedTask;
        }

        public async Task AddUserPlaylistItem(int userId, MultiplayerPlaylistItem item)
        {
            Debug.Assert(Room != null);
            Debug.Assert(APIRoom != null);
            Debug.Assert(currentItem != null);

            if (Room.Settings.QueueMode == QueueMode.HostOnly && Room.Host?.UserID != LocalUser?.UserID)
                throw new InvalidOperationException("Local user is not the room host.");

            item.OwnerID = userId;

            await addItem(item).ConfigureAwait(false);
            await updateCurrentItem(Room).ConfigureAwait(false);
        }

        public override Task AddPlaylistItem(MultiplayerPlaylistItem item) => AddUserPlaylistItem(api.LocalUser.Value.OnlineID, item);

        public async Task EditUserPlaylistItem(int userId, MultiplayerPlaylistItem item)
        {
            Debug.Assert(Room != null);
            Debug.Assert(APIRoom != null);
            Debug.Assert(currentItem != null);

            item.OwnerID = userId;

            var existingItem = serverSidePlaylist.SingleOrDefault(i => i.ID == item.ID);

            if (existingItem == null)
                throw new InvalidOperationException("Attempted to change an item that doesn't exist.");

            if (existingItem.OwnerID != userId && Room.Host?.UserID != LocalUser?.UserID)
                throw new InvalidOperationException("Attempted to change an item which is not owned by the user.");

            if (existingItem.Expired)
                throw new InvalidOperationException("Attempted to change an item which has already been played.");

            // Ensure the playlist order doesn't change.
            item.PlaylistOrder = existingItem.PlaylistOrder;

            serverSidePlaylist[serverSidePlaylist.IndexOf(existingItem)] = item;

            await ((IMultiplayerClient)this).PlaylistItemChanged(item).ConfigureAwait(false);
        }

        public override Task EditPlaylistItem(MultiplayerPlaylistItem item) => EditUserPlaylistItem(api.LocalUser.Value.OnlineID, item);

        public async Task RemoveUserPlaylistItem(int userId, long playlistItemId)
        {
            Debug.Assert(Room != null);
            Debug.Assert(APIRoom != null);

            var item = serverSidePlaylist.Find(i => i.ID == playlistItemId);

            if (item == null)
                throw new InvalidOperationException("Item does not exist in the room.");

            if (item == currentItem)
                throw new InvalidOperationException("The room's current item cannot be removed.");

            if (item.OwnerID != userId)
                throw new InvalidOperationException("Attempted to remove an item which is not owned by the user.");

            if (item.Expired)
                throw new InvalidOperationException("Attempted to remove an item which has already been played.");

            serverSidePlaylist.Remove(item);
            await ((IMultiplayerClient)this).PlaylistItemRemoved(playlistItemId).ConfigureAwait(false);

            await updateCurrentItem(Room).ConfigureAwait(false);
        }

        public override Task RemovePlaylistItem(long playlistItemId) => RemoveUserPlaylistItem(api.LocalUser.Value.OnlineID, playlistItemId);

        public override Task<APIBeatmap> GetAPIBeatmap(int beatmapId, CancellationToken cancellationToken = default)
        {
            IBeatmapInfo? beatmap = roomManager.ServerSideRooms.SelectMany(r => r.Playlist)
                                               .FirstOrDefault(p => p.BeatmapID == beatmapId)?.Beatmap.Value
                                    ?? beatmaps.QueryBeatmap(b => b.OnlineID == beatmapId);

            if (beatmap == null)
                throw new InvalidOperationException("Beatmap not found.");

            return Task.FromResult(new APIBeatmap
            {
                BeatmapSet = new APIBeatmapSet { OnlineID = beatmap.BeatmapSet?.OnlineID ?? -1 },
                OnlineID = beatmapId,
                Checksum = beatmap.MD5Hash
            });
        }

        private async Task changeMatchType(MatchType type)
        {
            Debug.Assert(Room != null);

            switch (type)
            {
                case MatchType.HeadToHead:
                    await ((IMultiplayerClient)this).MatchRoomStateChanged(null).ConfigureAwait(false);

                    foreach (var user in Room.Users)
                        await ((IMultiplayerClient)this).MatchUserStateChanged(user.UserID, null).ConfigureAwait(false);
                    break;

                case MatchType.TeamVersus:
                    await ((IMultiplayerClient)this).MatchRoomStateChanged(TeamVersusRoomState.CreateDefault()).ConfigureAwait(false);

                    foreach (var user in Room.Users)
                        await ((IMultiplayerClient)this).MatchUserStateChanged(user.UserID, new TeamVersusUserState()).ConfigureAwait(false);
                    break;
            }
        }

        private async Task changeQueueMode(QueueMode newMode)
        {
            Debug.Assert(Room != null);
            Debug.Assert(APIRoom != null);
            Debug.Assert(currentItem != null);

            // When changing to host-only mode, ensure that at least one non-expired playlist item exists by duplicating the current item.
            if (newMode == QueueMode.HostOnly && serverSidePlaylist.All(item => item.Expired))
                await duplicateCurrentItem().ConfigureAwait(false);

            await updatePlaylistOrder(Room).ConfigureAwait(false);
            await updateCurrentItem(Room).ConfigureAwait(false);
        }

        public async Task FinishCurrentItem()
        {
            Debug.Assert(Room != null);
            Debug.Assert(APIRoom != null);
            Debug.Assert(currentItem != null);

            // Expire the current playlist item.
            currentItem.Expired = true;
            currentItem.PlayedAt = DateTimeOffset.Now;

            await ((IMultiplayerClient)this).PlaylistItemChanged(currentItem).ConfigureAwait(false);
            await updatePlaylistOrder(Room).ConfigureAwait(false);

            // In host-only mode, a duplicate playlist item will be used for the next round.
            if (Room.Settings.QueueMode == QueueMode.HostOnly && serverSidePlaylist.All(item => item.Expired))
                await duplicateCurrentItem().ConfigureAwait(false);

            await updateCurrentItem(Room).ConfigureAwait(false);
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
            Debug.Assert(Room != null);

            item.ID = ++lastPlaylistItemId;

            serverSidePlaylist.Add(item);
            await ((IMultiplayerClient)this).PlaylistItemAdded(item).ConfigureAwait(false);

            await updatePlaylistOrder(Room).ConfigureAwait(false);
        }

        private IEnumerable<MultiplayerPlaylistItem> upcomingItems => serverSidePlaylist.Where(i => !i.Expired).OrderBy(i => i.PlaylistOrder);

        private async Task updateCurrentItem(MultiplayerRoom room, bool notify = true)
        {
            // Pick the next non-expired playlist item by playlist order, or default to the most-recently-expired item.
            MultiplayerPlaylistItem nextItem = upcomingItems.FirstOrDefault() ?? serverSidePlaylist.OrderByDescending(i => i.PlayedAt).First();

            currentIndex = serverSidePlaylist.IndexOf(nextItem);

            long lastItem = room.Settings.PlaylistItemId;
            room.Settings.PlaylistItemId = nextItem.ID;

            if (notify && nextItem.ID != lastItem)
                await ((IMultiplayerClient)this).SettingsChanged(room.Settings).ConfigureAwait(false);
        }

        private async Task updatePlaylistOrder(MultiplayerRoom room)
        {
            List<MultiplayerPlaylistItem> orderedActiveItems;

            switch (room.Settings.QueueMode)
            {
                default:
                    orderedActiveItems = serverSidePlaylist.Where(item => !item.Expired).OrderBy(item => item.ID).ToList();
                    break;

                case QueueMode.AllPlayersRoundRobin:
                    var itemsByPriority = new List<(MultiplayerPlaylistItem item, int priority)>();

                    // Assign a priority for items from each user, starting from 0 and increasing in order which the user added the items.
                    foreach (var group in serverSidePlaylist.Where(item => !item.Expired).OrderBy(item => item.ID).GroupBy(item => item.OwnerID))
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

                await ((IMultiplayerClient)this).PlaylistItemChanged(item).ConfigureAwait(false);
            }
        }
    }
}
