// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using Humanizer;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using osu.Framework.Threading;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.Countdown;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Match
{
    public class MultiplayerReadyButton : MultiplayerRoomComposite
    {
        [Resolved]
        private OngoingOperationTracker ongoingOperationTracker { get; set; }

        [CanBeNull]
        private IDisposable clickOperation;

        private Sample sampleReady;
        private Sample sampleReadyAll;
        private Sample sampleUnready;

        private readonly BindableBool enabled = new BindableBool();
        private readonly CountdownButton countdownButton;
        private int countReady;
        private ScheduledDelegate readySampleDelegate;
        private IBindable<bool> operationInProgress;

        public MultiplayerReadyButton()
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
                        new ReadyButton
                        {
                            RelativeSizeAxes = Axes.Both,
                            Size = Vector2.One,
                            Action = onReadyClick,
                            Enabled = { BindTarget = enabled },
                        },
                        countdownButton = new CountdownButton
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
            if (!isReady() || !Client.IsHost)
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

            Client.SendMatchRequest(new StartMatchCountdownRequest { Delay = duration }).ContinueWith(_ => endOperation());
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

            if (Room.Countdown != null)
                countdownButton.Alpha = 0;
            else
            {
                switch (localUser?.State)
                {
                    default:
                        countdownButton.Alpha = 0;
                        break;

                    case MultiplayerUserState.Spectating:
                    case MultiplayerUserState.Ready:
                        countdownButton.Alpha = Room.Host?.Equals(localUser) == true ? 1 : 0;
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
                enabled.Value &= Room.Host?.Equals(localUser) == true && newCountReady > 0;

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

        public class ReadyButton : Components.ReadyButton
        {
            public new Triangles Triangles => base.Triangles;

            [Resolved]
            private MultiplayerClient multiplayerClient { get; set; }

            [Resolved]
            private OsuColour colours { get; set; }

            [CanBeNull]
            private MultiplayerRoom room => multiplayerClient.Room;

            protected override void LoadComplete()
            {
                base.LoadComplete();

                multiplayerClient.RoomUpdated += () => Scheduler.AddOnce(onRoomUpdated);
                onRoomUpdated();
            }

            protected override void Update()
            {
                base.Update();

                if (room?.Countdown != null)
                {
                    // Update the countdown timer.
                    onRoomUpdated();
                }
            }

            private void onRoomUpdated()
            {
                updateButtonText();
                updateButtonColour();
            }

            private void updateButtonText()
            {
                if (room == null)
                {
                    Text = "Ready";
                    return;
                }

                var localUser = multiplayerClient.LocalUser;

                int countReady = room.Users.Count(u => u.State == MultiplayerUserState.Ready);
                int countTotal = room.Users.Count(u => u.State != MultiplayerUserState.Spectating);
                string countText = $"({countReady} / {countTotal} ready)";

                if (room.Countdown != null)
                {
                    string countdownText = $"Starting in {room.Countdown.EndTime - DateTimeOffset.Now:mm\\:ss}";

                    switch (localUser?.State)
                    {
                        default:
                            Text = $"Ready ({countdownText.ToLowerInvariant()})";
                            break;

                        case MultiplayerUserState.Spectating:
                        case MultiplayerUserState.Ready:
                            Text = $"{countdownText} {countText}";
                            break;
                    }
                }
                else
                {
                    switch (localUser?.State)
                    {
                        default:
                            Text = "Ready";
                            break;

                        case MultiplayerUserState.Spectating:
                        case MultiplayerUserState.Ready:
                            Text = room.Host?.Equals(localUser) == true
                                ? $"Start match {countText}"
                                : $"Waiting for host... {countText}";

                            break;
                    }
                }
            }

            private void updateButtonColour()
            {
                if (room == null)
                {
                    setGreen();
                    return;
                }

                var localUser = multiplayerClient.LocalUser;

                switch (localUser?.State)
                {
                    default:
                        setGreen();
                        break;

                    case MultiplayerUserState.Spectating:
                    case MultiplayerUserState.Ready:
                        if (room?.Host?.Equals(localUser) == true && room.Countdown == null)
                            setGreen();
                        else
                            setYellow();

                        break;
                }

                void setYellow()
                {
                    BackgroundColour = colours.YellowDark;
                    Triangles.ColourDark = colours.YellowDark;
                    Triangles.ColourLight = colours.Yellow;
                }

                void setGreen()
                {
                    BackgroundColour = colours.Green;
                    Triangles.ColourDark = colours.Green;
                    Triangles.ColourLight = colours.GreenLight;
                }
            }

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);

                if (multiplayerClient != null)
                    multiplayerClient.RoomUpdated -= onRoomUpdated;
            }

            public override LocalisableString TooltipText
            {
                get
                {
                    if (room?.Countdown != null && multiplayerClient.IsHost && multiplayerClient.LocalUser?.State == MultiplayerUserState.Ready)
                        return "Cancel countdown";

                    return base.TooltipText;
                }
            }
        }

        public class CountdownButton : IconButton, IHasPopover
        {
            private static readonly TimeSpan[] available_delays =
            {
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(30),
                TimeSpan.FromMinutes(1),
                TimeSpan.FromMinutes(2)
            };

            public new Action<TimeSpan> Action;

            private readonly Drawable background;

            public CountdownButton()
            {
                Icon = FontAwesome.Solid.CaretDown;
                IconScale = new Vector2(0.6f);

                Add(background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = float.MaxValue
                });

                base.Action = this.ShowPopover;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                background.Colour = colours.Green;
            }

            public Popover GetPopover()
            {
                var flow = new FillFlowContainer
                {
                    Width = 200,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(2),
                };

                foreach (var duration in available_delays)
                {
                    flow.Add(new PopoverButton
                    {
                        RelativeSizeAxes = Axes.X,
                        Text = $"Start match in {duration.Humanize()}",
                        BackgroundColour = background.Colour,
                        Action = () =>
                        {
                            Action(duration);
                            this.HidePopover();
                        }
                    });
                }

                return new OsuPopover { Child = flow };
            }

            public class PopoverButton : OsuButton
            {
            }
        }
    }
}
