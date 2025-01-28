// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Threading;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.Countdown;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Overlays.Dialog;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Match
{
    public partial class MatchStartControl : CompositeDrawable
    {
        public required Bindable<PlaylistItem?> SelectedItem
        {
            get => selectedItem;
            set => selectedItem.Current = value;
        }

        [Resolved]
        private OngoingOperationTracker ongoingOperationTracker { get; set; } = null!;

        [Resolved(canBeNull: true)]
        private IDialogOverlay? dialogOverlay { get; set; }

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        private readonly BindableWithCurrent<PlaylistItem?> selectedItem = new BindableWithCurrent<PlaylistItem?>();
        private readonly MultiplayerReadyButton readyButton;
        private readonly MultiplayerCountdownButton countdownButton;

        private IBindable<bool> operationInProgress = null!;
        private ScheduledDelegate? readySampleDelegate;
        private IDisposable? clickOperation;
        private Sample? sampleReady;
        private Sample? sampleReadyAll;
        private Sample? sampleUnready;
        private int countReady;

        public MatchStartControl()
        {
            InternalChild = new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                ColumnDimensions = new[]
                {
                    new Dimension(),
                    new Dimension(GridSizeMode.AutoSize)
                },
                Content = new[]
                {
                    new Drawable[]
                    {
                        readyButton = new MultiplayerReadyButton
                        {
                            RelativeSizeAxes = Axes.Both,
                            Size = Vector2.One,
                            Action = onReadyButtonClick,
                        },
                        countdownButton = new MultiplayerCountdownButton
                        {
                            RelativeSizeAxes = Axes.Y,
                            Size = new Vector2(40, 1),
                            Alpha = 0,
                            Action = startCountdown,
                            CancelAction = cancelCountdown
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            operationInProgress = ongoingOperationTracker.InProgress.GetBoundCopy();
            operationInProgress.BindValueChanged(_ => updateState());

            sampleReady = audio.Samples.Get(@"Multiplayer/player-ready");
            sampleReadyAll = audio.Samples.Get(@"Multiplayer/player-ready-all");
            sampleUnready = audio.Samples.Get(@"Multiplayer/player-unready");
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            SelectedItem.BindValueChanged(_ => updateState());
            client.RoomUpdated += onRoomUpdated;
            client.LoadRequested += onLoadRequested;
            updateState();
        }

        private void onRoomUpdated() => Scheduler.AddOnce(updateState);

        private void onLoadRequested() => Scheduler.AddOnce(endOperation);

        private void onReadyButtonClick()
        {
            if (client.Room == null)
                return;

            Debug.Assert(clickOperation == null);
            clickOperation = ongoingOperationTracker.BeginOperation();

            if (client.IsHost)
            {
                if (client.Room.State == MultiplayerRoomState.Open)
                {
                    if (isReady() && !client.Room.ActiveCountdowns.Any(c => c is MatchStartCountdown))
                        startMatch();
                    else
                        toggleReady();
                }
                else
                {
                    if (dialogOverlay == null)
                        abortMatch();
                    else
                        dialogOverlay.Push(new ConfirmAbortDialog(abortMatch, endOperation));
                }
            }
            else if (client.Room.State != MultiplayerRoomState.Closed)
                toggleReady();

            bool isReady() => client.LocalUser?.State == MultiplayerUserState.Ready || client.LocalUser?.State == MultiplayerUserState.Spectating;

            void toggleReady() => client.ToggleReady().FireAndForget(
                onSuccess: endOperation,
                onError: _ => endOperation());

            void startMatch() => client.StartMatch().FireAndForget(onSuccess: () =>
            {
                // gameplay is starting, the button will be unblocked on load requested.
            }, onError: _ =>
            {
                // gameplay was not started due to an exception; unblock button.
                endOperation();
            });

            void abortMatch() => client.AbortMatch().FireAndForget(endOperation, _ => endOperation());
        }

        private void startCountdown(TimeSpan duration)
        {
            Debug.Assert(clickOperation == null);
            clickOperation = ongoingOperationTracker.BeginOperation();

            client.SendMatchRequest(new StartMatchCountdownRequest { Duration = duration }).ContinueWith(_ => endOperation());
        }

        private void cancelCountdown()
        {
            if (client.Room == null)
                return;

            Debug.Assert(clickOperation == null);
            clickOperation = ongoingOperationTracker.BeginOperation();

            MultiplayerCountdown countdown = client.Room.ActiveCountdowns.Single(c => c is MatchStartCountdown);
            client.SendMatchRequest(new StopCountdownRequest(countdown.ID)).ContinueWith(_ => endOperation());
        }

        private void endOperation()
        {
            clickOperation?.Dispose();
            clickOperation = null;
        }

        private void updateState()
        {
            if (client.Room == null)
            {
                readyButton.Enabled.Value = false;
                countdownButton.Enabled.Value = false;
                return;
            }

            var localUser = client.LocalUser;

            int newCountReady = client.Room.Users.Count(u => u.State == MultiplayerUserState.Ready);
            int newCountTotal = client.Room.Users.Count(u => u.State != MultiplayerUserState.Spectating);

            if (!client.IsHost || client.Room.Settings.AutoStartEnabled)
                countdownButton.Hide();
            else
            {
                switch (localUser?.State)
                {
                    default:
                        countdownButton.Hide();
                        break;

                    case MultiplayerUserState.Idle:
                    case MultiplayerUserState.Spectating:
                    case MultiplayerUserState.Ready:
                        countdownButton.Show();
                        break;
                }
            }

            readyButton.Enabled.Value = countdownButton.Enabled.Value =
                client.Room.State != MultiplayerRoomState.Closed
                && SelectedItem.Value?.ID == client.Room.Settings.PlaylistItemId
                && !client.Room.Playlist.Single(i => i.ID == client.Room.Settings.PlaylistItemId).Expired
                && !operationInProgress.Value;

            // When the local user is the host and spectating the match, the ready button should be enabled only if any users are ready.
            if (localUser?.State == MultiplayerUserState.Spectating)
                readyButton.Enabled.Value &= client.IsHost && newCountReady > 0 && !client.Room.ActiveCountdowns.Any(c => c is MatchStartCountdown);

            // When the local user is not the host, the button should only be enabled when no match is in progress.
            if (!client.IsHost)
                readyButton.Enabled.Value &= client.Room.State == MultiplayerRoomState.Open;

            // At all times, the countdown button should only be enabled when no match is in progress.
            countdownButton.Enabled.Value &= client.Room.State == MultiplayerRoomState.Open;

            if (newCountReady == countReady)
                return;

            readySampleDelegate?.Cancel();
            readySampleDelegate = Schedule(() =>
            {
                if (newCountReady > countReady)
                {
                    if (newCountReady == newCountTotal)
                        sampleReadyAll?.Play();
                    else
                        sampleReady?.Play();
                }
                else if (newCountReady < countReady)
                {
                    sampleUnready?.Play();
                }

                countReady = newCountReady;
            });
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (client.IsNotNull())
            {
                client.RoomUpdated -= onRoomUpdated;
                client.LoadRequested -= onLoadRequested;
            }
        }

        public partial class ConfirmAbortDialog : DangerousActionDialog
        {
            public ConfirmAbortDialog(Action abortMatch, Action cancel)
            {
                HeaderText = "Are you sure you want to abort the match?";

                DangerousAction = abortMatch;
                CancelAction = cancel;
            }
        }
    }
}
