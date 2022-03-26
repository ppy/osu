// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Threading;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.Countdown;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Match
{
    public class MatchStartControl : MultiplayerRoomComposite
    {
        [Resolved]
        private OngoingOperationTracker ongoingOperationTracker { get; set; }

        [CanBeNull]
        private IDisposable clickOperation;

        private Sample sampleReady;
        private Sample sampleReadyAll;
        private Sample sampleUnready;

        private readonly BindableBool enabled = new BindableBool();
        private readonly MultiplayerCountdownButton countdownButton;
        private int countReady;
        private ScheduledDelegate readySampleDelegate;
        private IBindable<bool> operationInProgress;

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
                        new MultiplayerReadyButton
                        {
                            RelativeSizeAxes = Axes.Both,
                            Size = Vector2.One,
                            Action = onReadyClick,
                            Enabled = { BindTarget = enabled },
                        },
                        countdownButton = new MultiplayerCountdownButton
                        {
                            RelativeSizeAxes = Axes.Y,
                            Size = new Vector2(40, 1),
                            Alpha = 0,
                            Action = startCountdown,
                            Enabled = { BindTarget = enabled }
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

            CurrentPlaylistItem.BindValueChanged(_ => updateState());
        }

        protected override void OnRoomUpdated()
        {
            base.OnRoomUpdated();
            updateState();
        }

        protected override void OnRoomLoadRequested()
        {
            base.OnRoomLoadRequested();
            endOperation();
        }

        private void onReadyClick()
        {
            if (Room == null)
                return;

            Debug.Assert(clickOperation == null);
            clickOperation = ongoingOperationTracker.BeginOperation();

            // Ensure the current user becomes ready before being able to do anything else (start match, stop countdown, unready).
            if (!isReady() || !Client.IsHost || Room.Settings.AutoStartEnabled)
            {
                toggleReady();
                return;
            }

            // Local user is the room host and is in a ready state.
            // The only action they can take is to stop a countdown if one's currently running.
            if (Room.Countdown != null)
            {
                stopCountdown();
                return;
            }

            // And if a countdown isn't running, start the match.
            startMatch();

            bool isReady() => Client.LocalUser?.State == MultiplayerUserState.Ready || Client.LocalUser?.State == MultiplayerUserState.Spectating;

            void toggleReady() => Client.ToggleReady().ContinueWith(_ => endOperation());

            void stopCountdown() => Client.SendMatchRequest(new StopCountdownRequest()).ContinueWith(_ => endOperation());

            void startMatch() => Client.StartMatch().ContinueWith(t =>
            {
                // accessing Exception here silences any potential errors from the antecedent task
                if (t.Exception != null)
                {
                    // gameplay was not started due to an exception; unblock button.
                    endOperation();
                }

                // gameplay is starting, the button will be unblocked on load requested.
            });
        }

        private void startCountdown(TimeSpan duration)
        {
            Debug.Assert(clickOperation == null);
            clickOperation = ongoingOperationTracker.BeginOperation();

            Client.SendMatchRequest(new StartMatchCountdownRequest { Duration = duration }).ContinueWith(_ => endOperation());
        }

        private void endOperation()
        {
            clickOperation?.Dispose();
            clickOperation = null;
        }

        private void updateState()
        {
            if (Room == null)
            {
                enabled.Value = false;
                return;
            }

            var localUser = Client.LocalUser;

            int newCountReady = Room.Users.Count(u => u.State == MultiplayerUserState.Ready);
            int newCountTotal = Room.Users.Count(u => u.State != MultiplayerUserState.Spectating);

            if (!Client.IsHost || Room.Countdown != null || Room.Settings.AutoStartEnabled)
                countdownButton.Hide();
            else
            {
                switch (localUser?.State)
                {
                    default:
                        countdownButton.Hide();
                        break;

                    case MultiplayerUserState.Spectating:
                    case MultiplayerUserState.Ready:
                        countdownButton.Show();
                        break;
                }
            }

            enabled.Value =
                Room.State == MultiplayerRoomState.Open
                && CurrentPlaylistItem.Value?.ID == Room.Settings.PlaylistItemId
                && !Room.Playlist.Single(i => i.ID == Room.Settings.PlaylistItemId).Expired
                && !operationInProgress.Value;

            // When the local user is the host and spectating the match, the "start match" state should be enabled if any users are ready.
            if (localUser?.State == MultiplayerUserState.Spectating)
                enabled.Value &= Client.IsHost && newCountReady > 0;

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
    }
}
