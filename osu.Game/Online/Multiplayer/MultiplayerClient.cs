// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Development;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Database;
using osu.Game.Localisation;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer.Countdown;
using osu.Game.Online.Rooms;
using osu.Game.Overlays.Notifications;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Utils;

namespace osu.Game.Online.Multiplayer
{
    public abstract partial class MultiplayerClient : Component, IMultiplayerClient, IMultiplayerRoomServer
    {
        public Action<Notification>? PostNotification { protected get; set; }

        public Action<Room, string>? PresentMatch { protected get; set; }

        /// <summary>
        /// Invoked when any change occurs to the multiplayer room.
        /// </summary>
        public virtual event Action? RoomUpdated;

        /// <summary>
        /// Invoked when a user's local style is changed.
        /// </summary>
        public event Action<MultiplayerRoomUser>? UserStyleChanged;

        /// <summary>
        /// Invoked when a user's local mods are changed.
        /// </summary>
        public event Action<MultiplayerRoomUser>? UserModsChanged;

        /// <summary>
        /// Invoked when the room's settings are changed.
        /// </summary>
        public event Action<MultiplayerRoomSettings>? SettingsChanged;

        /// <summary>
        /// Invoked when a new user joins the room.
        /// </summary>
        public event Action<MultiplayerRoomUser>? UserJoined;

        /// <summary>
        /// Invoked when a user leaves the room of their own accord.
        /// </summary>
        public virtual event Action<MultiplayerRoomUser>? UserLeft;

        /// <summary>
        /// Invoked when a user was kicked from the room forcefully.
        /// </summary>
        public event Action<MultiplayerRoomUser>? UserKicked;

        /// <summary>
        /// Invoked when the room's host is changed.
        /// </summary>
        public event Action<MultiplayerRoomUser?>? HostChanged;

        /// <summary>
        /// Invoked when a new item is added to the playlist.
        /// </summary>
        public event Action<MultiplayerPlaylistItem>? ItemAdded;

        /// <summary>
        /// Invoked when a playlist item is removed from the playlist. The provided <c>long</c> is the playlist's item ID.
        /// </summary>
        public event Action<long>? ItemRemoved;

        /// <summary>
        /// Invoked when a playlist item's details change.
        /// </summary>
        public event Action<MultiplayerPlaylistItem>? ItemChanged;

        /// <summary>
        /// Invoked when the multiplayer server requests the current beatmap to be loaded into play.
        /// </summary>
        public virtual event Action? LoadRequested;

        /// <summary>
        /// Invoked when the multiplayer server requests gameplay to be aborted.
        /// </summary>
        public event Action<GameplayAbortReason>? GameplayAborted;

        /// <summary>
        /// Invoked when the multiplayer server requests gameplay to be started.
        /// </summary>
        public event Action? GameplayStarted;

        /// <summary>
        /// Invoked when the multiplayer server has finished collating results.
        /// </summary>
        public event Action? ResultsReady;

        /// <summary>
        /// Invoked just prior to disconnection requested by the server via <see cref="IStatefulUserHubClient.DisconnectRequested"/>.
        /// </summary>
        public event Action? Disconnecting;

        /// <summary>
        /// Whether the <see cref="MultiplayerClient"/> is currently connected.
        /// This is NOT thread safe and usage should be scheduled.
        /// </summary>
        public abstract IBindable<bool> IsConnected { get; }

        /// <summary>
        /// The joined <see cref="MultiplayerRoom"/>.
        /// </summary>
        public virtual MultiplayerRoom? Room // virtual for moq
        {
            get
            {
                Debug.Assert(ThreadSafety.IsUpdateThread);
                return room;
            }
            private set
            {
                Debug.Assert(ThreadSafety.IsUpdateThread);
                room = value;
            }
        }

        private MultiplayerRoom? room;

        /// <summary>
        /// The users in the joined <see cref="Room"/> which are participating in the current gameplay loop.
        /// </summary>
        public virtual IBindableList<int> CurrentMatchPlayingUserIds => PlayingUserIds;

        protected readonly BindableList<int> PlayingUserIds = new BindableList<int>();

        /// <summary>
        /// The <see cref="MultiplayerRoomUser"/> corresponding to the local player, if available.
        /// </summary>
        public virtual MultiplayerRoomUser? LocalUser => Room?.Users.SingleOrDefault(u => u.User?.Id == API.LocalUser.Value.Id);

        /// <summary>
        /// Whether the <see cref="LocalUser"/> is the host in <see cref="Room"/>.
        /// </summary>
        public virtual bool IsHost
        {
            get
            {
                var localUser = LocalUser;
                return localUser != null && Room?.Host != null && localUser.Equals(Room.Host);
            }
        }

        [Resolved]
        protected IAPIProvider API { get; private set; } = null!;

        [Resolved]
        protected IRulesetStore Rulesets { get; private set; } = null!;

        [Resolved]
        private UserLookupCache userLookupCache { get; set; } = null!;

        protected Room? APIRoom { get; private set; }

        private readonly Queue<Action> pendingRequests = new Queue<Action>();

        [BackgroundDependencyLoader]
        private void load()
        {
            IsConnected.BindValueChanged(connected => Scheduler.Add(() =>
            {
                // clean up local room state on server disconnect.
                if (!connected.NewValue && Room != null)
                    LeaveRoom();
            }));
        }

        private readonly TaskChain joinOrLeaveTaskChain = new TaskChain();
        private CancellationTokenSource? joinCancellationSource;

        /// <summary>
        /// Creates and joins a <see cref="MultiplayerRoom"/> described by an API <see cref="Room"/>.
        /// </summary>
        /// <param name="room">The API <see cref="Room"/> describing the room to create.</param>
        /// <exception cref="InvalidOperationException">If the current user is already in another room.</exception>
        public async Task CreateRoom(Room room)
        {
            if (Room != null)
                throw new InvalidOperationException("Cannot create a multiplayer room while already in one.");

            var cancellationSource = joinCancellationSource = new CancellationTokenSource();

            await joinOrLeaveTaskChain.Add(async () =>
            {
                await runOnUpdateThreadAsync(() => pendingRequests.Clear(), cancellationSource.Token).ConfigureAwait(false);
                var multiplayerRoom = await CreateRoomInternal(new MultiplayerRoom(room)).ConfigureAwait(false);
                await setupJoinedRoom(room, multiplayerRoom, cancellationSource.Token).ConfigureAwait(false);
            }, cancellationSource.Token).ConfigureAwait(false);
        }

        /// <summary>
        /// Joins the <see cref="MultiplayerRoom"/> for a given API <see cref="Room"/>.
        /// </summary>
        /// <param name="room">The API <see cref="Room"/>.</param>
        /// <param name="password">An optional password to use for the join operation.</param>
        /// <exception cref="InvalidOperationException">If the current user is already in another room, or <paramref name="room"/> does not represent an active room.</exception>
        public async Task JoinRoom(Room room, string? password = null)
        {
            if (Room != null)
                throw new InvalidOperationException("Cannot join a multiplayer room while already in one.");

            if (room.RoomID == null)
                throw new InvalidOperationException("Cannot join an inactive room.");

            var cancellationSource = joinCancellationSource = new CancellationTokenSource();

            await joinOrLeaveTaskChain.Add(async () =>
            {
                await runOnUpdateThreadAsync(() => pendingRequests.Clear(), cancellationSource.Token).ConfigureAwait(false);
                var multiplayerRoom = await JoinRoomInternal(room.RoomID.Value, password ?? room.Password).ConfigureAwait(false);
                await setupJoinedRoom(room, multiplayerRoom, cancellationSource.Token).ConfigureAwait(false);
            }, cancellationSource.Token).ConfigureAwait(false);
        }

        /// <summary>
        /// Performs post-join setup of a <see cref="MultiplayerRoom"/>.
        /// </summary>
        /// <param name="apiRoom">The incoming API <see cref="Room"/> that was requested to be joined.</param>
        /// <param name="joinedRoom">The resuling <see cref="MultiplayerRoom"/> that was joined.</param>
        /// <param name="cancellationToken">A token to cancel the process.</param>
        private async Task setupJoinedRoom(Room apiRoom, MultiplayerRoom joinedRoom, CancellationToken cancellationToken)
        {
            // Populate users.
            await PopulateUsers(joinedRoom.Users).ConfigureAwait(false);
            if (joinedRoom.Host != null)
                await PopulateUsers([joinedRoom.Host]).ConfigureAwait(false);

            // Update the stored room (must be done on update thread for thread-safety).
            await runOnUpdateThreadAsync(() =>
            {
                Debug.Assert(Room == null);
                Debug.Assert(APIRoom == null);

                Room = joinedRoom;
                APIRoom = apiRoom;

                APIRoom.RoomID = joinedRoom.RoomID;
                APIRoom.ChannelId = joinedRoom.ChannelID;
                APIRoom.Host = joinedRoom.Host?.User;
                APIRoom.Playlist = joinedRoom.Playlist.Select(item => new PlaylistItem(item)).ToArray();
                APIRoom.CurrentPlaylistItem = APIRoom.Playlist.Single(item => item.ID == joinedRoom.Settings.PlaylistItemId);
                // The server will null out the end date upon the host joining the room, but the null value is never communicated to the client.
                APIRoom.EndDate = null;

                Debug.Assert(LocalUser != null);
                addUserToAPIRoom(LocalUser);

                foreach (var user in joinedRoom.Users)
                    updateUserPlayingState(user.UserID, user.State);

                updateLocalRoomSettings(joinedRoom.Settings);

                while (pendingRequests.TryDequeue(out Action? action))
                    action();

                postServerShuttingDownNotification();

                OnRoomJoined();
            }, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Fired when the room join sequence is complete
        /// </summary>
        protected virtual void OnRoomJoined()
        {
        }

        public Task LeaveRoom()
        {
            if (Room == null)
                return Task.CompletedTask;

            // The join may have not completed yet, so certain tasks that either update the room or reference the room should be cancelled.
            // This includes the setting of Room itself along with the initial update of the room settings on join.
            joinCancellationSource?.Cancel();

            // Leaving rooms is expected to occur instantaneously whilst the operation is finalised in the background.
            // However a few members need to be reset immediately to prevent other components from entering invalid states whilst the operation hasn't yet completed.
            // For example, if a room was left and the user immediately pressed the "create room" button, then the user could be taken into the lobby if the value of Room is not reset in time.
            var scheduledReset = runOnUpdateThreadAsync(() =>
            {
                APIRoom = null;
                Room = null;
                PlayingUserIds.Clear();

                RoomUpdated?.Invoke();
            });

            return Task.Run(async () =>
            {
                try
                {
                    await joinOrLeaveTaskChain.Add(async () =>
                    {
                        await scheduledReset.ConfigureAwait(false);
                        await LeaveRoomInternal().ConfigureAwait(false);
                    }).ConfigureAwait(false);
                }
                finally
                {
                    await runOnUpdateThreadAsync(() =>
                    {
                        pendingRequests.Clear();
                    }).ConfigureAwait(false);
                }
            });
        }

        /// <summary>
        /// Creates the <see cref="MultiplayerRoom"/> with the given settings.
        /// </summary>
        /// <param name="room">The room.</param>
        /// <returns>The joined <see cref="MultiplayerRoom"/></returns>
        protected abstract Task<MultiplayerRoom> CreateRoomInternal(MultiplayerRoom room);

        /// <summary>
        /// Joins the <see cref="MultiplayerRoom"/> with a given ID.
        /// </summary>
        /// <param name="roomId">The room ID.</param>
        /// <param name="password">An optional password to use when joining the room.</param>
        /// <returns>The joined <see cref="MultiplayerRoom"/>.</returns>
        protected abstract Task<MultiplayerRoom> JoinRoomInternal(long roomId, string? password = null);

        /// <summary>
        /// Leaves the currently-joined <see cref="MultiplayerRoom"/>.
        /// </summary>
        protected abstract Task LeaveRoomInternal();

        public abstract Task InvitePlayer(int userId);

        /// <summary>
        /// Change the current <see cref="MultiplayerRoom"/> settings.
        /// </summary>
        /// <remarks>
        /// A room must be joined for this to have any effect.
        /// </remarks>
        /// <param name="name">The new room name, if any.</param>
        /// <param name="password">The new password, if any.</param>
        /// <param name="matchType">The type of the match, if any.</param>
        /// <param name="queueMode">The new queue mode, if any.</param>
        /// <param name="autoStartDuration">The new auto-start countdown duration, if any.</param>
        /// <param name="autoSkip">The new auto-skip setting.</param>
        public Task ChangeSettings(Optional<string> name = default, Optional<string> password = default, Optional<MatchType> matchType = default, Optional<QueueMode> queueMode = default,
                                   Optional<TimeSpan> autoStartDuration = default, Optional<bool> autoSkip = default)
        {
            if (Room == null)
                throw new InvalidOperationException("Must be joined to a match to change settings.");

            return ChangeSettings(new MultiplayerRoomSettings
            {
                Name = name.GetOr(Room.Settings.Name),
                Password = password.GetOr(Room.Settings.Password),
                MatchType = matchType.GetOr(Room.Settings.MatchType),
                QueueMode = queueMode.GetOr(Room.Settings.QueueMode),
                AutoStartDuration = autoStartDuration.GetOr(Room.Settings.AutoStartDuration),
                AutoSkip = autoSkip.GetOr(Room.Settings.AutoSkip)
            });
        }

        /// <summary>
        /// Toggles the <see cref="LocalUser"/>'s ready state.
        /// </summary>
        /// <exception cref="InvalidOperationException">If a toggle of ready state is not valid at this time.</exception>
        public async Task ToggleReady()
        {
            var localUser = LocalUser;

            if (localUser == null)
                return;

            switch (localUser.State)
            {
                case MultiplayerUserState.Idle:
                    await ChangeState(MultiplayerUserState.Ready).ConfigureAwait(false);
                    return;

                case MultiplayerUserState.Ready:
                    await ChangeState(MultiplayerUserState.Idle).ConfigureAwait(false);
                    return;

                default:
                    throw new InvalidOperationException($"Cannot toggle ready when in {localUser.State}");
            }
        }

        /// <summary>
        /// Toggles the <see cref="LocalUser"/>'s spectating state.
        /// </summary>
        /// <exception cref="InvalidOperationException">If a toggle of the spectating state is not valid at this time.</exception>
        public async Task ToggleSpectate()
        {
            var localUser = LocalUser;

            if (localUser == null)
                return;

            switch (localUser.State)
            {
                case MultiplayerUserState.Idle:
                case MultiplayerUserState.Ready:
                    await ChangeState(MultiplayerUserState.Spectating).ConfigureAwait(false);
                    return;

                case MultiplayerUserState.Spectating:
                    await ChangeState(MultiplayerUserState.Idle).ConfigureAwait(false);
                    return;

                default:
                    throw new InvalidOperationException($"Cannot toggle spectate when in {localUser.State}");
            }
        }

        public abstract Task TransferHost(int userId);

        public abstract Task KickUser(int userId);

        public abstract Task ChangeSettings(MultiplayerRoomSettings settings);

        public abstract Task ChangeState(MultiplayerUserState newState);

        public abstract Task ChangeBeatmapAvailability(BeatmapAvailability newBeatmapAvailability);

        public abstract Task DisconnectInternal();

        public abstract Task ChangeUserStyle(int? beatmapId, int? rulesetId);

        /// <summary>
        /// Change the local user's mods in the currently joined room.
        /// </summary>
        /// <param name="newMods">The proposed new mods, excluding any required by the room itself.</param>
        public Task ChangeUserMods(IEnumerable<Mod> newMods) => ChangeUserMods(newMods.Select(m => new APIMod(m)).ToList());

        public abstract Task ChangeUserMods(IEnumerable<APIMod> newMods);

        public abstract Task SendMatchRequest(MatchUserRequest request);

        public abstract Task StartMatch();

        public abstract Task AbortGameplay();

        public abstract Task AbortMatch();

        public abstract Task AddPlaylistItem(MultiplayerPlaylistItem item);

        public abstract Task EditPlaylistItem(MultiplayerPlaylistItem item);

        public abstract Task RemovePlaylistItem(long playlistItemId);

        Task IMultiplayerClient.RoomStateChanged(MultiplayerRoomState state)
        {
            handleRoomRequest(() =>
            {
                Debug.Assert(Room != null);
                Debug.Assert(APIRoom != null);

                Room.State = state;

                switch (state)
                {
                    case MultiplayerRoomState.Open:
                        APIRoom.Status = RoomStatus.Idle;
                        break;

                    case MultiplayerRoomState.WaitingForLoad:
                    case MultiplayerRoomState.Playing:
                        APIRoom.Status = RoomStatus.Playing;
                        break;

                    case MultiplayerRoomState.Closed:
                        APIRoom.EndDate = DateTimeOffset.Now;
                        APIRoom.Status = RoomStatus.Idle;
                        break;
                }

                RoomUpdated?.Invoke();
            });

            return Task.CompletedTask;
        }

        async Task IMultiplayerClient.UserJoined(MultiplayerRoomUser user)
        {
            await PopulateUsers([user]).ConfigureAwait(false);

            handleRoomRequest(() =>
            {
                Debug.Assert(Room != null);

                // for sanity, ensure that there can be no duplicate users in the room user list.
                if (Room.Users.Any(existing => existing.UserID == user.UserID))
                    return;

                Room.Users.Add(user);

                addUserToAPIRoom(user);

                UserJoined?.Invoke(user);
                RoomUpdated?.Invoke();
            });
        }

        Task IMultiplayerClient.UserLeft(MultiplayerRoomUser user)
        {
            handleRoomRequest(() => handleUserLeft(user, UserLeft));
            return Task.CompletedTask;
        }

        Task IMultiplayerClient.UserKicked(MultiplayerRoomUser user)
        {
            handleRoomRequest(() =>
            {
                if (LocalUser == null)
                    return;

                if (user.Equals(LocalUser))
                    LeaveRoom();

                handleUserLeft(user, UserKicked);
            });

            return Task.CompletedTask;
        }

        private void handleUserLeft(MultiplayerRoomUser user, Action<MultiplayerRoomUser>? callback)
        {
            Debug.Assert(ThreadSafety.IsUpdateThread);
            Debug.Assert(Room != null);

            Room.Users.Remove(user);
            PlayingUserIds.Remove(user.UserID);

            Debug.Assert(APIRoom != null);
            APIRoom.RecentParticipants = APIRoom.RecentParticipants.Where(u => u.Id != user.UserID).ToArray();
            APIRoom.ParticipantCount--;

            callback?.Invoke(user);
            RoomUpdated?.Invoke();
        }

        async Task IMultiplayerClient.Invited(int invitedBy, long roomID, string password)
        {
            APIUser? apiUser = await userLookupCache.GetUserAsync(invitedBy).ConfigureAwait(false);
            Room? apiRoom = await getRoomAsync(roomID).ConfigureAwait(false);

            if (apiUser == null || apiRoom == null) return;

            PostNotification?.Invoke(new MultiplayerInvitationNotification(apiUser, apiRoom)
            {
                Activated = () =>
                {
                    PresentMatch?.Invoke(apiRoom, password);
                    return true;
                }
            });

            Task<Room?> getRoomAsync(long id)
            {
                TaskCompletionSource<Room?> taskCompletionSource = new TaskCompletionSource<Room?>();

                var request = new GetRoomRequest(id);
                request.Success += room => taskCompletionSource.TrySetResult(room);
                request.Failure += _ => taskCompletionSource.TrySetResult(null);

                API.Queue(request);

                return taskCompletionSource.Task;
            }
        }

        private void addUserToAPIRoom(MultiplayerRoomUser user)
        {
            Debug.Assert(APIRoom != null);

            APIRoom.RecentParticipants = APIRoom.RecentParticipants.Append(user.User ?? new APIUser
            {
                Id = user.UserID,
                Username = "[Unresolved]"
            }).ToArray();
            APIRoom.ParticipantCount++;
        }

        Task IMultiplayerClient.HostChanged(int userId)
        {
            handleRoomRequest(() =>
            {
                Debug.Assert(Room != null);
                Debug.Assert(APIRoom != null);

                var user = Room.Users.FirstOrDefault(u => u.UserID == userId);

                Room.Host = user;
                APIRoom.Host = user?.User;

                HostChanged?.Invoke(user);
                RoomUpdated?.Invoke();
            });

            return Task.CompletedTask;
        }

        Task IMultiplayerClient.SettingsChanged(MultiplayerRoomSettings newSettings)
        {
            handleRoomRequest(() => updateLocalRoomSettings(newSettings));
            return Task.CompletedTask;
        }

        Task IMultiplayerClient.UserStateChanged(int userId, MultiplayerUserState state)
        {
            handleRoomRequest(() =>
            {
                Debug.Assert(Room != null);

                var user = Room.Users.SingleOrDefault(u => u.UserID == userId);

                // TODO: user should NEVER be null here, see https://github.com/ppy/osu/issues/17713.
                if (user == null)
                    return;

                user.State = state;
                updateUserPlayingState(userId, state);

                RoomUpdated?.Invoke();
            });

            return Task.CompletedTask;
        }

        Task IMultiplayerClient.MatchUserStateChanged(int userId, MatchUserState state)
        {
            handleRoomRequest(() =>
            {
                Debug.Assert(Room != null);

                var user = Room.Users.SingleOrDefault(u => u.UserID == userId);

                // TODO: user should NEVER be null here, see https://github.com/ppy/osu/issues/17713.
                if (user == null)
                    return;

                user.MatchState = state;
                RoomUpdated?.Invoke();
            });

            return Task.CompletedTask;
        }

        Task IMultiplayerClient.MatchRoomStateChanged(MatchRoomState state)
        {
            handleRoomRequest(() =>
            {
                Debug.Assert(Room != null);

                Room.MatchState = state;
                RoomUpdated?.Invoke();
            });

            return Task.CompletedTask;
        }

        public Task MatchEvent(MatchServerEvent e)
        {
            handleRoomRequest(() =>
            {
                Debug.Assert(Room != null);

                switch (e)
                {
                    case CountdownStartedEvent countdownStartedEvent:
                        Room.ActiveCountdowns.Add(countdownStartedEvent.Countdown);

                        switch (countdownStartedEvent.Countdown)
                        {
                            case ServerShuttingDownCountdown:
                                postServerShuttingDownNotification();
                                break;
                        }

                        break;

                    case CountdownStoppedEvent countdownStoppedEvent:
                        MultiplayerCountdown? countdown = Room.ActiveCountdowns.FirstOrDefault(countdown => countdown.ID == countdownStoppedEvent.ID);
                        if (countdown != null)
                            Room.ActiveCountdowns.Remove(countdown);
                        break;
                }

                RoomUpdated?.Invoke();
            });

            return Task.CompletedTask;
        }

        private void postServerShuttingDownNotification()
        {
            ServerShuttingDownCountdown? countdown = room?.ActiveCountdowns.OfType<ServerShuttingDownCountdown>().FirstOrDefault();

            if (countdown == null)
                return;

            PostNotification?.Invoke(new ServerShutdownNotification(countdown.TimeRemaining));
        }

        Task IMultiplayerClient.UserBeatmapAvailabilityChanged(int userId, BeatmapAvailability beatmapAvailability)
        {
            handleRoomRequest(() =>
            {
                Debug.Assert(Room != null);

                var user = Room.Users.SingleOrDefault(u => u.UserID == userId);

                // errors here are not critical - beatmap availability state is mostly for display.
                if (user == null)
                    return;

                user.BeatmapAvailability = beatmapAvailability;

                RoomUpdated?.Invoke();
            });

            return Task.CompletedTask;
        }

        Task IMultiplayerClient.UserStyleChanged(int userId, int? beatmapId, int? rulesetId)
        {
            handleRoomRequest(() =>
            {
                Debug.Assert(Room != null);

                var user = Room.Users.SingleOrDefault(u => u.UserID == userId);

                // errors here are not critical - user style is mostly for display.
                if (user == null)
                    return;

                user.BeatmapId = beatmapId;
                user.RulesetId = rulesetId;

                UserStyleChanged?.Invoke(user);
                RoomUpdated?.Invoke();
            });

            return Task.CompletedTask;
        }

        Task IMultiplayerClient.UserModsChanged(int userId, IEnumerable<APIMod> mods)
        {
            handleRoomRequest(() =>
            {
                Debug.Assert(Room != null);

                var user = Room.Users.SingleOrDefault(u => u.UserID == userId);

                // errors here are not critical - user mods are mostly for display.
                if (user == null)
                    return;

                user.Mods = mods;

                UserModsChanged?.Invoke(user);
                RoomUpdated?.Invoke();
            });

            return Task.CompletedTask;
        }

        Task IMultiplayerClient.LoadRequested()
        {
            handleRoomRequest(() =>
            {
                Debug.Assert(Room != null);
                LoadRequested?.Invoke();
            });

            return Task.CompletedTask;
        }

        Task IMultiplayerClient.GameplayAborted(GameplayAbortReason reason)
        {
            handleRoomRequest(() =>
            {
                Debug.Assert(Room != null);
                GameplayAborted?.Invoke(reason);
            });

            return Task.CompletedTask;
        }

        Task IMultiplayerClient.GameplayStarted()
        {
            handleRoomRequest(() =>
            {
                Debug.Assert(Room != null);
                GameplayStarted?.Invoke();
            });

            return Task.CompletedTask;
        }

        Task IMultiplayerClient.ResultsReady()
        {
            handleRoomRequest(() =>
            {
                Debug.Assert(Room != null);
                ResultsReady?.Invoke();
            });

            return Task.CompletedTask;
        }

        public Task PlaylistItemAdded(MultiplayerPlaylistItem item)
        {
            handleRoomRequest(() =>
            {
                Debug.Assert(Room != null);
                Debug.Assert(APIRoom != null);

                Room.Playlist.Add(item);
                APIRoom.Playlist = APIRoom.Playlist.Append(new PlaylistItem(item)).ToArray();

                ItemAdded?.Invoke(item);
                RoomUpdated?.Invoke();
            });

            return Task.CompletedTask;
        }

        public Task PlaylistItemRemoved(long playlistItemId)
        {
            handleRoomRequest(() =>
            {
                Debug.Assert(Room != null);
                Debug.Assert(APIRoom != null);

                Room.Playlist.Remove(Room.Playlist.Single(existing => existing.ID == playlistItemId));
                APIRoom.Playlist = APIRoom.Playlist.Where(i => i.ID != playlistItemId).ToArray();

                Debug.Assert(Room.Playlist.Count > 0);

                ItemRemoved?.Invoke(playlistItemId);
                RoomUpdated?.Invoke();
            });

            return Task.CompletedTask;
        }

        public Task PlaylistItemChanged(MultiplayerPlaylistItem item)
        {
            handleRoomRequest(() =>
            {
                Debug.Assert(Room != null);
                Debug.Assert(APIRoom != null);

                Room.Playlist[Room.Playlist.IndexOf(Room.Playlist.Single(existing => existing.ID == item.ID))] = item;
                APIRoom.Playlist = APIRoom.Playlist.Select((pi, i) => pi.ID == item.ID ? new PlaylistItem(item) : APIRoom.Playlist[i]).ToArray();

                ItemChanged?.Invoke(item);
                RoomUpdated?.Invoke();
            });

            return Task.CompletedTask;
        }

        /// <summary>
        /// Populates the <see cref="APIUser"/> for a given collection of <see cref="MultiplayerRoomUser"/>s.
        /// </summary>
        /// <param name="multiplayerUsers">The <see cref="MultiplayerRoomUser"/>s to populate.</param>
        protected async Task PopulateUsers(IEnumerable<MultiplayerRoomUser> multiplayerUsers)
        {
            foreach (int[] userChunk in multiplayerUsers.Select(u => u.UserID).Distinct().Chunk(GetUsersRequest.MAX_IDS_PER_REQUEST))
            {
                var request = new GetUsersRequest(userChunk);

                await API.PerformAsync(request).ConfigureAwait(false);

                if (request.Response == null)
                    return;

                Dictionary<int, APIUser> users = request.Response.Users.ToDictionary(user => user.Id);

                foreach (var multiplayerUser in multiplayerUsers)
                {
                    if (users.TryGetValue(multiplayerUser.UserID, out var user))
                        multiplayerUser.User = user;
                }
            }
        }

        /// <summary>
        /// Updates the local room settings with the given <see cref="MultiplayerRoomSettings"/>.
        /// </summary>
        /// <remarks>
        /// This updates both the joined <see cref="MultiplayerRoom"/> and the respective API <see cref="Room"/>.
        /// </remarks>
        /// <param name="settings">The new <see cref="MultiplayerRoomSettings"/> to update from.</param>
        private void updateLocalRoomSettings(MultiplayerRoomSettings settings)
        {
            Debug.Assert(Room != null);
            Debug.Assert(APIRoom != null);

            // Update a few properties of the room instantaneously.
            Room.Settings = settings;
            APIRoom.Name = Room.Settings.Name;
            APIRoom.Password = Room.Settings.Password;
            APIRoom.Type = Room.Settings.MatchType;
            APIRoom.QueueMode = Room.Settings.QueueMode;
            APIRoom.AutoStartDuration = Room.Settings.AutoStartDuration;
            APIRoom.CurrentPlaylistItem = APIRoom.Playlist.Single(item => item.ID == settings.PlaylistItemId);
            APIRoom.AutoSkip = Room.Settings.AutoSkip;

            SettingsChanged?.Invoke(settings);
            RoomUpdated?.Invoke();
        }

        /// <summary>
        /// For the provided user ID, update whether the user is included in <see cref="CurrentMatchPlayingUserIds"/>.
        /// </summary>
        /// <param name="userId">The user's ID.</param>
        /// <param name="state">The new state of the user.</param>
        private void updateUserPlayingState(int userId, MultiplayerUserState state)
        {
            bool wasPlaying = PlayingUserIds.Contains(userId);
            bool isPlaying = state >= MultiplayerUserState.WaitingForLoad && state <= MultiplayerUserState.FinishedPlay;

            if (isPlaying == wasPlaying)
                return;

            if (isPlaying)
                PlayingUserIds.Add(userId);
            else
                PlayingUserIds.Remove(userId);
        }

        private Task runOnUpdateThreadAsync(Action action, CancellationToken cancellationToken = default)
        {
            var tcs = new TaskCompletionSource<bool>();

            Scheduler.Add(() =>
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    tcs.SetCanceled(cancellationToken);
                    return;
                }

                try
                {
                    action();
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            return tcs.Task;
        }

        private void handleRoomRequest(Action request)
        {
            Scheduler.Add(() =>
            {
                if (Room == null)
                {
                    pendingRequests.Enqueue(request);
                    return;
                }

                request();
            });
        }

        Task IStatefulUserHubClient.DisconnectRequested()
        {
            Schedule(() =>
            {
                Disconnecting?.Invoke();
                DisconnectInternal();
            });
            return Task.CompletedTask;
        }

        private partial class MultiplayerInvitationNotification : UserAvatarNotification
        {
            protected override IconUsage CloseButtonIcon => FontAwesome.Solid.Times;

            public MultiplayerInvitationNotification(APIUser user, Room room)
                : base(user, NotificationsStrings.InvitedYouToTheMultiplayer(user.Username, room.Name))
            {
            }
        }
    }
}
