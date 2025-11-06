// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Graphics.Cursor;
using osu.Game.Online;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Matchmaking.Events;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Overlays.Dialog;
using osu.Game.Rulesets;
using osu.Game.Screens.Footer;
using osu.Game.Screens.OnlinePlay.Matchmaking.Match.Gameplay;
using osu.Game.Screens.OnlinePlay.Multiplayer;
using osu.Game.Users;
using osuTK;
using osuTK.Input;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Match
{
    /// <summary>
    /// The main matchmaking screen which houses a custom <see cref="ScreenStack"/> through the life cycle of a single session.
    /// </summary>
    public partial class ScreenMatchmaking : OsuScreen, IPreviewTrackOwner, IHandlePresentBeatmap
    {
        /// <summary>
        /// Padding between rows of the content.
        /// </summary>
        private const float row_padding = 10;

        private static readonly Vector2 chat_size = new Vector2(550, 130);

        public override bool? ApplyModTrackAdjustments => true;

        public override bool DisallowExternalBeatmapRulesetChanges => true;

        public override bool ShowFooter => true;

        [Cached(typeof(OnlinePlayBeatmapAvailabilityTracker))]
        private readonly OnlinePlayBeatmapAvailabilityTracker beatmapAvailabilityTracker = new MultiplayerBeatmapAvailabilityTracker();

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        [Resolved]
        private BeatmapManager beatmapManager { get; set; } = null!;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        [Resolved]
        private BeatmapLookupCache beatmapLookupCache { get; set; } = null!;

        [Resolved]
        private BeatmapModelDownloader beatmapDownloader { get; set; } = null!;

        [Resolved]
        private IDialogOverlay dialogOverlay { get; set; } = null!;

        [Resolved]
        private AudioManager audio { get; set; } = null!;

        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

        [Resolved]
        private PreviewTrackManager previewTrackManager { get; set; } = null!;

        [Resolved]
        private MusicController music { get; set; } = null!;

        private readonly MultiplayerRoom room;
        private readonly MatchmakingChatDisplay chat;

        private Sample? sampleStart;
        private CancellationTokenSource? downloadCheckCancellation;
        private int? lastDownloadCheckedBeatmapId;

        public ScreenMatchmaking(MultiplayerRoom room)
        {
            this.room = room;

            Activity.Value = new UserActivity.InLobby(room);
            Padding = new MarginPadding { Horizontal = -HORIZONTAL_OVERFLOW_PADDING };

            chat = new MatchmakingChatDisplay(new Room(room))
            {
                Anchor = Anchor.BottomRight,
                Origin = Anchor.BottomRight,
                Size = chat_size,
                Margin = new MarginPadding
                {
                    Right = WaveOverlayContainer.WIDTH_PADDING - HORIZONTAL_OVERFLOW_PADDING,
                    Bottom = row_padding
                },
                Alpha = 0
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            sampleStart = audio.Samples.Get(@"SongSelect/confirm-selection");

            InternalChild = new OsuContextMenuContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = new PopoverContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        beatmapAvailabilityTracker,
                        new MultiplayerRoomSounds(),
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding
                            {
                                Horizontal = WaveOverlayContainer.WIDTH_PADDING,
                                Top = row_padding,
                            },
                            RowDimensions = new[]
                            {
                                new Dimension(),
                                new Dimension(GridSizeMode.Absolute, row_padding),
                                new Dimension(GridSizeMode.AutoSize),
                            },
                            Content = new Drawable[]?[]
                            {
                                [
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Masking = true,
                                        CornerRadius = 10,
                                        Children = new Drawable[]
                                        {
                                            new Box
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                Colour = colourProvider.Background6,
                                            },
                                            new ScreenStack(),
                                        }
                                    }
                                ],
                                null,
                                [
                                    new Container
                                    {
                                        Name = "Chat Area Space",
                                        Anchor = Anchor.TopRight,
                                        Origin = Anchor.TopRight,
                                        Size = new Vector2(550, 130),
                                        Margin = new MarginPadding { Bottom = row_padding }
                                    }
                                ]
                            }
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            client.RoomUpdated += onRoomUpdated;
            client.UserStateChanged += onUserStateChanged;
            client.SettingsChanged += onSettingsChanged;
            client.LoadRequested += onLoadRequested;

            beatmapAvailabilityTracker.Availability.BindValueChanged(onBeatmapAvailabilityChanged, true);

            Footer?.Add(new ChatContainer(chat));
        }

        private void onRoomUpdated()
        {
            if (this.IsCurrentScreen() && client.Room == null)
            {
                Logger.Log($"{this} exiting due to loss of room or connection");
                this.Exit();
            }
        }

        private void onUserStateChanged(MultiplayerRoomUser user, MultiplayerUserState state)
        {
            if (user.Equals(client.LocalUser) && state == MultiplayerUserState.Idle)
                this.MakeCurrent();
        }

        private void onSettingsChanged(MultiplayerRoomSettings _) => Scheduler.Add(() =>
        {
            checkForAutomaticDownload();
            updateGameplayState();
        });

        private void onBeatmapAvailabilityChanged(ValueChangedEvent<BeatmapAvailability> e) => Scheduler.Add(() =>
        {
            if (client.Room == null || client.LocalUser == null)
                return;

            client.ChangeBeatmapAvailability(e.NewValue).FireAndForget();

            switch (e.NewValue.State)
            {
                case DownloadState.NotDownloaded:
                case DownloadState.LocallyAvailable:
                    updateGameplayState();
                    break;
            }
        });

        private void updateGameplayState()
        {
            MultiplayerPlaylistItem item = client.Room!.CurrentPlaylistItem;

            if (item.Expired)
                return;

            RulesetInfo ruleset = rulesets.GetRuleset(item.RulesetID)!;
            Ruleset rulesetInstance = ruleset.CreateInstance();

            // Update global gameplay state to correspond to the new selection.
            // Retrieve the corresponding local beatmap, since we can't directly use the playlist's beatmap info
            var localBeatmap = beatmapManager.QueryBeatmap($@"{nameof(BeatmapInfo.OnlineID)} == $0 AND {nameof(BeatmapInfo.MD5Hash)} == {nameof(BeatmapInfo.OnlineMD5Hash)}", item.BeatmapID);

            if (localBeatmap != null)
            {
                Beatmap.Value = beatmapManager.GetWorkingBeatmap(localBeatmap);
                Ruleset.Value = ruleset;
                Mods.Value = item.RequiredMods.Select(m => m.ToMod(rulesetInstance)).ToArray();

                // Notify the server that the beatmap has been set and that we are ready to start gameplay.
                if (client.LocalUser!.State == MultiplayerUserState.Idle)
                    client.ChangeState(MultiplayerUserState.Ready).FireAndForget();
            }
            else
            {
                // Notify the server that we don't have the beatmap.
                if (client.LocalUser!.State == MultiplayerUserState.Ready)
                    client.ChangeState(MultiplayerUserState.Idle).FireAndForget();
            }

            client.ChangeBeatmapAvailability(beatmapAvailabilityTracker.Availability.Value).FireAndForget();
        }

        private void onLoadRequested() => Scheduler.Add(() =>
        {
            updateGameplayState();

            if (Beatmap.IsDefault)
            {
                Logger.Log("Aborting gameplay start - beatmap not downloaded.");
                return;
            }

            sampleStart?.Play();

            this.Push(new MultiplayerPlayerLoader(() => new ScreenGameplay(new Room(room), new PlaylistItem(client.Room!.CurrentPlaylistItem), room.Users.ToArray())));
        });

        private void checkForAutomaticDownload()
        {
            if (client.Room == null)
                return;

            MultiplayerPlaylistItem item = client.Room.CurrentPlaylistItem;

            // This method is called every time anything changes in the room.
            // This could result in download requests firing far too often, when we only expect them to fire once per beatmap.
            //
            // Without this check, we would see especially egregious behaviour when a user has hit the download rate limit.
            if (lastDownloadCheckedBeatmapId == item.BeatmapID)
                return;

            lastDownloadCheckedBeatmapId = item.BeatmapID;

            downloadCheckCancellation?.Cancel();

            if (beatmapManager.IsAvailableLocally(new APIBeatmap { OnlineID = item.BeatmapID }))
                return;

            // In a perfect world we'd use BeatmapAvailability, but there's no event-driven flow for when a selection changes.
            // ie. if selection changes from "not downloaded" to another "not downloaded" we wouldn't get a value changed raised.
            beatmapLookupCache
                .GetBeatmapAsync(item.BeatmapID, (downloadCheckCancellation = new CancellationTokenSource()).Token)
                .ContinueWith(resolved => Schedule(() =>
                {
                    APIBeatmapSet? beatmapSet = resolved.GetResultSafely()?.BeatmapSet;

                    if (beatmapSet == null)
                        return;

                    beatmapDownloader.Download(beatmapSet, config.Get<bool>(OsuSetting.PreferNoVideo));
                }));
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            switch (e.Key)
            {
                case Key.Space:
                    if (e.Repeat)
                        return true;

                    client.SendMatchRequest(new MatchmakingAvatarActionRequest { Action = MatchmakingAvatarAction.Jump }).FireAndForget();
                    return true;
            }

            return false;
        }

        public override IReadOnlyList<ScreenFooterButton> CreateFooterButtons() =>
        [
            new HistoryFooterButton(room)
        ];

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);

            chat.Appear();
            beginHandlingTrack();
        }

        public override void OnSuspending(ScreenTransitionEvent e)
        {
            chat.Disappear();
            endHandlingTrack();

            base.OnSuspending(e);
        }

        private bool exitConfirmed;

        public override bool OnExiting(ScreenExitEvent e)
        {
            if (exitConfirmed)
            {
                if (base.OnExiting(e))
                {
                    exitConfirmed = false;
                    return true;
                }

                chat.Disappear().Expire();
                endHandlingTrack();

                client.LeaveRoom().FireAndForget();
                return false;
            }

            if (dialogOverlay.CurrentDialog is ConfirmDialog confirmDialog)
                confirmDialog.PerformOkAction();
            else
            {
                dialogOverlay.Push(new ConfirmDialog("Are you sure you want to leave this multiplayer match?", () =>
                {
                    exitConfirmed = true;
                    if (this.IsCurrentScreen())
                        this.Exit();
                }));
            }

            return true;
        }

        public override void OnResuming(ScreenTransitionEvent e)
        {
            base.OnResuming(e);

            chat.Appear();
            beginHandlingTrack();

            if (e.Last is not MultiplayerPlayerLoader playerLoader)
                return;

            if (!playerLoader.GameplayPassed)
            {
                client.AbortGameplay().FireAndForget();
                return;
            }

            client.ChangeState(MultiplayerUserState.Idle).FireAndForget();
        }

        /// <summary>
        /// Handles changes in the track to keep it looping while active.
        /// </summary>
        private void beginHandlingTrack()
        {
            Beatmap.BindValueChanged(applyLoopingToTrack, true);
        }

        /// <summary>
        /// Stops looping the current track and stops handling further changes to the track.
        /// </summary>
        private void endHandlingTrack()
        {
            Beatmap.ValueChanged -= applyLoopingToTrack;
            Beatmap.Value.Track.Looping = false;

            previewTrackManager.StopAnyPlaying(this);
        }

        /// <summary>
        /// Invoked on changes to the beatmap to loop the track. See: <see cref="beginHandlingTrack"/>.
        /// </summary>
        /// <param name="beatmap">The beatmap change event.</param>
        private void applyLoopingToTrack(ValueChangedEvent<WorkingBeatmap> beatmap)
        {
            if (!this.IsCurrentScreen())
                return;

            beatmap.NewValue.PrepareTrackForPreview(true);
            music.EnsurePlayingSomething();
        }

        public void PresentBeatmap(WorkingBeatmap beatmap, RulesetInfo ruleset)
        {
            // Do nothing to prevent the user from potentially being kicked out
            // of gameplay due to the screen performer's internal processes.
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (client.IsNotNull())
            {
                client.RoomUpdated -= onRoomUpdated;
                client.UserStateChanged -= onUserStateChanged;
                client.SettingsChanged -= onSettingsChanged;
                client.LoadRequested -= onLoadRequested;
            }
        }

        // Contains the chat display and a context menu container for it. Shared lifetime with the chat display (expires along with it).
        private partial class ChatContainer : CompositeDrawable
        {
            public override double LifetimeStart => chat.LifetimeStart;
            public override double LifetimeEnd => chat.LifetimeEnd;

            private readonly MatchmakingChatDisplay chat;

            public ChatContainer(MatchmakingChatDisplay chat)
            {
                this.chat = chat;

                Anchor = Anchor.BottomRight;
                Origin = Anchor.BottomRight;

                // This component is added to the screen footer which is only about 50px high.
                // Therefore, it's given a large absolute size to give the context menu enough space to display correctly.
                Size = new Vector2(chat_size.X);

                InternalChild = new OsuContextMenuContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = chat
                };
            }
        }
    }
}
