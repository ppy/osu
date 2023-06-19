// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Development;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using osu.Game.Database;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer.Countdown;
using osu.Game.Online.Rooms;
using osu.Game.Online.Rooms.RoomStatuses;
using osu.Game.Overlays.Notifications;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Utils;

namespace osu.Game.Online.Multiplayer
{
    public abstract partial class MultiplayerClient : Component, IMultiplayerClient, IMultiplayerRoomServer
    {
        public Action<Notification>? PostNotification { protected get; set; }

        /// <summary>
        /// Invoked when any change occurs to the multiplayer room.
        /// </summary>
        public virtual event Action? RoomUpdated;

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
        /// Invoked when the multiplayer server requests loading of play to be aborted.
        /// </summary>
        public event Action? LoadAborted;

        /// <summary>
        /// Invoked when the multiplayer server requests gameplay to be started.
        /// </summary>
        public event Action? GameplayStarted;

        /// <summary>
        /// Invoked when the multiplayer server has finished collating results.
        /// </summary>
        public event Action? ResultsReady;

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

        [BackgroundDependencyLoader]
        private void load()
        {
            IsConnected.BindValueChanged(connected => Scheduler.Add(() =>
            {
                // clean up local room state on server disconnect.
                if (!connected.NewValue && Room != null)
                {
                    Logger.Log("Clearing room due to multiplayer server connection loss.", LoggingTarget.Runtime, LogLevel.Important);
                    LeaveRoom();
                }
            }));
        }

        private readonly TaskChain joinOrLeaveTaskChain = new TaskChain();
        private CancellationTokenSource? joinCancellationSource;

        /// <summary>
        /// Joins the <see cref="MultiplayerRoom"/> for a given API <see cref="Room"/>.
        /// </summary>
        /// <param name="room">The API <see cref="Room"/>.</param>
        /// <param name="password">An optional password to use for the join operation.</param>
        public async Task JoinRoom(Room room, string? password = null)
        {
            if (Room != null)
                throw new InvalidOperationException("Cannot join a multiplayer room while already in one.");

            var cancellationSource = joinCancellationSource = new CancellationTokenSource();

            await joinOrLeaveTaskChain.Add(async () =>
            {
                Debug.Assert(room.RoomID.Value != null);

                // Join the server-side room.
                var joinedRoom = await JoinRoom(room.RoomID.Value.Value, password ?? room.Password.Value).ConfigureAwait(false);
                Debug.Assert(joinedRoom != null);

                // Populate users.
                Debug.Assert(joinedRoom.Users != null);
                await Task.WhenAll(joinedRoom.Users.Select(PopulateUser)).ConfigureAwait(false);

                // Update the stored room (must be done on update thread for thread-safety).
                await runOnUpdateThreadAsync(() =>
                {
                    Debug.Assert(Room == null);

                    Room = joinedRoom;
                    APIRoom = room;

                    Debug.Assert(joinedRoom.Playlist.Count > 0);

                    APIRoom.Playlist.Clear();
                    APIRoom.Playlist.AddRange(joinedRoom.Playlist.Select(createPlaylistItem));
                    APIRoom.CurrentPlaylistItem.Value = APIRoom.Playlist.Single(item => item.ID == joinedRoom.Settings.PlaylistItemId);

                    // The server will null out the end date upon the host joining the room, but the null value is never communicated to the client.
                    APIRoom.EndDate.Value = null;

                    Debug.Assert(LocalUser != null);
                    addUserToAPIRoom(LocalUser);

                    foreach (var user in joinedRoom.Users)
                        updateUserPlayingState(user.UserID, user.State);

                    updateLocalRoomSettings(joinedRoom.Settings);

                    postServerShuttingDownNotification();

                    OnRoomJoined();
                }, cancellationSource.Token).ConfigureAwait(false);
            }, cancellationSource.Token).ConfigureAwait(false);
        }

        /// <summary>
        /// Fired when the room join sequence is complete
        /// </summary>
        protected virtual void OnRoomJoined()
        {
        }

        /// <summary>
        /// Joins the <see cref="MultiplayerRoom"/> with a given ID.
        /// </summary>
        /// <param name="roomId">The room ID.</param>
        /// <param name="password">An optional password to use when joining the room.</param>
        /// <returns>The joined <see cref="MultiplayerRoom"/>.</returns>
        protected abstract Task<MultiplayerRoom> JoinRoom(long roomId, string? password = null);

        public Task LeaveRoom()
        {
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

            return joinOrLeaveTaskChain.Add(async () =>
            {
                await scheduledReset.ConfigureAwait(false);
                await LeaveRoomInternal().ConfigureAwait(false);
            });
        }

        protected abstract Task LeaveRoomInternal();

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

        /// <summary>
        /// Change the local user's mods in the currently joined room.
        /// </summary>
        /// <param name="newMods">The proposed new mods, excluding any required by the room itself.</param>
        public Task ChangeUserMods(IEnumerable<Mod> newMods) => ChangeUserMods(newMods.Select(m => new APIMod(m)).ToList());

        public abstract Task ChangeUserMods(IEnumerable<APIMod> newMods);

        public abstract Task SendMatchRequest(MatchUserRequest request);

        public abstract Task StartMatch();

        public abstract Task AbortGameplay();

        public abstract Task AddPlaylistItem(MultiplayerPlaylistItem item);

        public abstract Task EditPlaylistItem(MultiplayerPlaylistItem item);

        public abstract Task RemovePlaylistItem(long playlistItemId);

        Task IMultiplayerClient.RoomStateChanged(MultiplayerRoomState state)
        {
            Scheduler.Add(() =>
            {
                if (Room == null)
                    return;

                Debug.Assert(APIRoom != null);

                Room.State = state;

                switch (state)
                {
                    case MultiplayerRoomState.Open:
                        APIRoom.Status.Value = new RoomStatusOpen();
                        break;

                    case MultiplayerRoomState.Playing:
                        APIRoom.Status.Value = new RoomStatusPlaying();
                        break;

                    case MultiplayerRoomState.Closed:
                        APIRoom.Status.Value = new RoomStatusEnded();
                        break;
                }

                RoomUpdated?.Invoke();
            }, false);

            return Task.CompletedTask;
        }

        async Task IMultiplayerClient.UserJoined(MultiplayerRoomUser user)
        {
            await PopulateUser(user).ConfigureAwait(false);

            Scheduler.Add(() =>
            {
                if (Room == null)
                    return;

                // for sanity, ensure that there can be no duplicate users in the room user list.
                if (Room.Users.Any(existing => existing.UserID == user.UserID))
                    return;

                Room.Users.Add(user);

                addUserToAPIRoom(user);

                UserJoined?.Invoke(user);
                RoomUpdated?.Invoke();
            }, false);
        }

        Task IMultiplayerClient.UserLeft(MultiplayerRoomUser user) =>
            handleUserLeft(user, UserLeft);

        Task IMultiplayerClient.UserKicked(MultiplayerRoomUser user)
        {
            if (LocalUser == null)
                return Task.CompletedTask;

            if (user.Equals(LocalUser))
                LeaveRoom();

            return handleUserLeft(user, UserKicked);
        }

        private void addUserToAPIRoom(MultiplayerRoomUser user)
        {
            Debug.Assert(APIRoom != null);

            APIRoom.RecentParticipants.Add(user.User ?? new APIUser
            {
                Id = user.UserID,
                Username = "[Unresolved]"
            });
            APIRoom.ParticipantCount.Value++;
        }

        private Task handleUserLeft(MultiplayerRoomUser user, Action<MultiplayerRoomUser>? callback)
        {
            Scheduler.Add(() =>
            {
                if (Room == null)
                    return;

                Room.Users.Remove(user);
                PlayingUserIds.Remove(user.UserID);

                Debug.Assert(APIRoom != null);
                APIRoom.RecentParticipants.RemoveAll(u => u.Id == user.UserID);
                APIRoom.ParticipantCount.Value--;

                callback?.Invoke(user);
                RoomUpdated?.Invoke();
            }, false);

            return Task.CompletedTask;
        }

        Task IMultiplayerClient.HostChanged(int userId)
        {
            Scheduler.Add(() =>
            {
                if (Room == null)
                    return;

                Debug.Assert(APIRoom != null);

                var user = Room.Users.FirstOrDefault(u => u.UserID == userId);

                Room.Host = user;
                APIRoom.Host.Value = user?.User;

                RoomUpdated?.Invoke();
            }, false);

            return Task.CompletedTask;
        }

        Task IMultiplayerClient.SettingsChanged(MultiplayerRoomSettings newSettings)
        {
            Scheduler.Add(() => updateLocalRoomSettings(newSettings));
            return Task.CompletedTask;
        }

        Task IMultiplayerClient.UserStateChanged(int userId, MultiplayerUserState state)
        {
            Scheduler.Add(() =>
            {
                var user = Room?.Users.SingleOrDefault(u => u.UserID == userId);

                // TODO: user should NEVER be null here, see https://github.com/ppy/osu/issues/17713.
                if (user == null)
                    return;

                user.State = state;
                updateUserPlayingState(userId, state);

                RoomUpdated?.Invoke();
            }, false);

            return Task.CompletedTask;
        }

        Task IMultiplayerClient.MatchUserStateChanged(int userId, MatchUserState state)
        {
            Scheduler.Add(() =>
            {
                var user = Room?.Users.SingleOrDefault(u => u.UserID == userId);

                // TODO: user should NEVER be null here, see https://github.com/ppy/osu/issues/17713.
                if (user == null)
                    return;

                user.MatchState = state;
                RoomUpdated?.Invoke();
            }, false);

            return Task.CompletedTask;
        }

        Task IMultiplayerClient.MatchRoomStateChanged(MatchRoomState state)
        {
            Scheduler.Add(() =>
            {
                if (Room == null)
                    return;

                Room.MatchState = state;
                RoomUpdated?.Invoke();
            }, false);

            return Task.CompletedTask;
        }

        public Task MatchEvent(MatchServerEvent e)
        {
            Scheduler.Add(() =>
            {
                if (Room == null)
                    return;

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
            }, false);

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
            Scheduler.Add(() =>
            {
                var user = Room?.Users.SingleOrDefault(u => u.UserID == userId);

                // errors here are not critical - beatmap availability state is mostly for display.
                if (user == null)
                    return;

                user.BeatmapAvailability = beatmapAvailability;

                RoomUpdated?.Invoke();
            }, false);

            return Task.CompletedTask;
        }

        public Task UserModsChanged(int userId, IEnumerable<APIMod> mods)
        {
            Scheduler.Add(() =>
            {
                var user = Room?.Users.SingleOrDefault(u => u.UserID == userId);

                // errors here are not critical - user mods are mostly for display.
                if (user == null)
                    return;

                user.Mods = mods;

                RoomUpdated?.Invoke();
            }, false);

            return Task.CompletedTask;
        }

        Task IMultiplayerClient.LoadRequested()
        {
            Scheduler.Add(() =>
            {
                if (Room == null)
                    return;

                LoadRequested?.Invoke();
            }, false);

            return Task.CompletedTask;
        }

        Task IMultiplayerClient.LoadAborted()
        {
            Scheduler.Add(() =>
            {
                if (Room == null)
                    return;

                LoadAborted?.Invoke();
            }, false);

            return Task.CompletedTask;
        }

        Task IMultiplayerClient.GameplayStarted()
        {
            Scheduler.Add(() =>
            {
                if (Room == null)
                    return;

                GameplayStarted?.Invoke();
            }, false);

            return Task.CompletedTask;
        }

        Task IMultiplayerClient.ResultsReady()
        {
            Scheduler.Add(() =>
            {
                if (Room == null)
                    return;

                ResultsReady?.Invoke();
            }, false);

            return Task.CompletedTask;
        }

        public Task PlaylistItemAdded(MultiplayerPlaylistItem item)
        {
            Scheduler.Add(() =>
            {
                if (Room == null)
                    return;

                Debug.Assert(APIRoom != null);

                Room.Playlist.Add(item);
                APIRoom.Playlist.Add(createPlaylistItem(item));

                ItemAdded?.Invoke(item);
                RoomUpdated?.Invoke();
            });

            return Task.CompletedTask;
        }

        public Task PlaylistItemRemoved(long playlistItemId)
        {
            Scheduler.Add(() =>
            {
                if (Room == null)
                    return;

                Debug.Assert(APIRoom != null);

                Room.Playlist.Remove(Room.Playlist.Single(existing => existing.ID == playlistItemId));
                APIRoom.Playlist.RemoveAll(existing => existing.ID == playlistItemId);

                Debug.Assert(Room.Playlist.Count > 0);

                ItemRemoved?.Invoke(playlistItemId);
                RoomUpdated?.Invoke();
            });

            return Task.CompletedTask;
        }

        public Task PlaylistItemChanged(MultiplayerPlaylistItem item)
        {
            Scheduler.Add(() =>
            {
                if (Room == null)
                    return;

                try
                {
                    Debug.Assert(APIRoom != null);

                    Room.Playlist[Room.Playlist.IndexOf(Room.Playlist.Single(existing => existing.ID == item.ID))] = item;

                    int existingIndex = APIRoom.Playlist.IndexOf(APIRoom.Playlist.Single(existing => existing.ID == item.ID));
                    APIRoom.Playlist.RemoveAt(existingIndex);
                    APIRoom.Playlist.Insert(existingIndex, createPlaylistItem(item));
                }
                catch (Exception ex)
                {
                    throw new AggregateException($"Item: {JsonConvert.SerializeObject(createPlaylistItem(item))}\n\nRoom:{JsonConvert.SerializeObject(APIRoom)}", ex);
                }

                ItemChanged?.Invoke(item);
                RoomUpdated?.Invoke();
            });

            return Task.CompletedTask;
        }

        /// <summary>
        /// Populates the <see cref="APIUser"/> for a given <see cref="MultiplayerRoomUser"/>.
        /// </summary>
        /// <param name="multiplayerUser">The <see cref="MultiplayerRoomUser"/> to populate.</param>
        protected async Task PopulateUser(MultiplayerRoomUser multiplayerUser) => multiplayerUser.User ??= await userLookupCache.GetUserAsync(multiplayerUser.UserID).ConfigureAwait(false);

        /// <summary>
        /// Updates the local room settings with the given <see cref="MultiplayerRoomSettings"/>.
        /// </summary>
        /// <remarks>
        /// This updates both the joined <see cref="MultiplayerRoom"/> and the respective API <see cref="Room"/>.
        /// </remarks>
        /// <param name="settings">The new <see cref="MultiplayerRoomSettings"/> to update from.</param>
        private void updateLocalRoomSettings(MultiplayerRoomSettings settings)
        {
            if (Room == null)
                return;

            Debug.Assert(APIRoom != null);

            // Update a few properties of the room instantaneously.
            Room.Settings = settings;
            APIRoom.Name.Value = Room.Settings.Name;
            APIRoom.Password.Value = Room.Settings.Password;
            APIRoom.Type.Value = Room.Settings.MatchType;
            APIRoom.QueueMode.Value = Room.Settings.QueueMode;
            APIRoom.AutoStartDuration.Value = Room.Settings.AutoStartDuration;
            APIRoom.CurrentPlaylistItem.Value = APIRoom.Playlist.Single(item => item.ID == settings.PlaylistItemId);
            APIRoom.AutoSkip.Value = Room.Settings.AutoSkip;

            RoomUpdated?.Invoke();
        }

        private PlaylistItem createPlaylistItem(MultiplayerPlaylistItem item) => new PlaylistItem(new APIBeatmap { OnlineID = item.BeatmapID, StarRating = item.StarRating })
        {
            ID = item.ID,
            OwnerID = item.OwnerID,
            RulesetID = item.RulesetID,
            Expired = item.Expired,
            PlaylistOrder = item.PlaylistOrder,
            PlayedAt = item.PlayedAt,
            RequiredMods = item.RequiredMods.ToArray(),
            AllowedMods = item.AllowedMods.ToArray()
        };

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
    }
}
