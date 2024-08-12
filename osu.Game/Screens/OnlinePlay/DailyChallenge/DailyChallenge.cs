// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Cursor;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Metadata;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.OnlinePlay.Components;
using osu.Game.Screens.OnlinePlay.DailyChallenge.Events;
using osu.Game.Screens.OnlinePlay.Match;
using osu.Game.Screens.OnlinePlay.Match.Components;
using osu.Game.Screens.OnlinePlay.Playlists;
using osu.Game.Screens.Play;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.DailyChallenge
{
    [Cached(typeof(IPreviewTrackOwner))]
    public partial class DailyChallenge : OsuScreen, IPreviewTrackOwner, IHandlePresentBeatmap
    {
        private readonly Room room;
        private readonly PlaylistItem playlistItem;

        /// <summary>
        /// Any mods applied by/to the local user.
        /// </summary>
        private readonly Bindable<IReadOnlyList<Mod>> userMods = new Bindable<IReadOnlyList<Mod>>(Array.Empty<Mod>());

        private readonly IBindable<APIState> apiState = new Bindable<APIState>();
        private readonly IBindable<DailyChallengeInfo?> dailyChallengeInfo = new Bindable<DailyChallengeInfo?>();

        private OnlinePlayScreenWaveContainer waves = null!;
        private DailyChallengeLeaderboard leaderboard = null!;
        private RoomModSelectOverlay userModsSelectOverlay = null!;
        private Sample? sampleStart;
        private IDisposable? userModsSelectOverlayRegistration;

        private DailyChallengeScoreBreakdown breakdown = null!;
        private DailyChallengeTotalsDisplay totals = null!;
        private DailyChallengeEventFeed feed = null!;

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Plum);

        [Cached(Type = typeof(IRoomManager))]
        private RoomManager roomManager { get; set; }

        [Cached]
        private readonly OnlinePlayBeatmapAvailabilityTracker beatmapAvailabilityTracker = new OnlinePlayBeatmapAvailabilityTracker();

        [Resolved]
        private OsuGame? game { get; set; }

        [Resolved]
        private BeatmapManager beatmapManager { get; set; } = null!;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        [Resolved]
        private MusicController musicController { get; set; } = null!;

        [Resolved]
        private IOverlayManager? overlayManager { get; set; }

        [Resolved]
        private MetadataClient metadataClient { get; set; } = null!;

        [Resolved]
        private UserLookupCache userLookupCache { get; set; } = null!;

        [Resolved]
        protected IAPIProvider API { get; private set; } = null!;

        [Resolved]
        private PreviewTrackManager previewTrackManager { get; set; } = null!;

        [Resolved]
        private INotificationOverlay? notificationOverlay { get; set; }

        public override bool DisallowExternalBeatmapRulesetChanges => true;

        public override bool? ApplyModTrackAdjustments => true;

        public DailyChallenge(Room room)
        {
            this.room = room;
            playlistItem = room.Playlist.Single();
            roomManager = new RoomManager();
            Padding = new MarginPadding { Horizontal = -HORIZONTAL_OVERFLOW_PADDING };
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            return new CachedModelDependencyContainer<Room>(base.CreateChildDependencies(parent))
            {
                Model = { Value = room }
            };
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            sampleStart = audio.Samples.Get(@"SongSelect/confirm-selection");

            FillFlowContainer footerButtons;

            InternalChild = waves = new OnlinePlayScreenWaveContainer
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    roomManager,
                    beatmapAvailabilityTracker,
                    new ScreenStack(new RoomBackgroundScreen(playlistItem))
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                    new Header(ButtonSystemStrings.DailyChallenge.ToSentence(), null),
                    new PopoverContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Child = new GridContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding
                            {
                                Horizontal = WaveOverlayContainer.WIDTH_PADDING,
                                Top = Header.HEIGHT,
                            },
                            RowDimensions =
                            [
                                new Dimension(GridSizeMode.AutoSize),
                                new Dimension(GridSizeMode.Absolute, 10),
                                new Dimension(),
                                new Dimension(GridSizeMode.Absolute, 30),
                                new Dimension(GridSizeMode.Absolute, 50)
                            ],
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    new DrawableRoomPlaylistItem(playlistItem)
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AllowReordering = false,
                                        Scale = new Vector2(1.4f),
                                        Width = 1 / 1.4f,
                                    }
                                },
                                null,
                                [
                                    new OsuContextMenuContainer
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Masking = true,
                                        CornerRadius = 10,
                                        Children = new Drawable[]
                                        {
                                            new Box
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                Colour = colourProvider.Background4,
                                            },
                                            new GridContainer
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                Padding = new MarginPadding(10),
                                                ColumnDimensions =
                                                [
                                                    new Dimension(),
                                                    new Dimension(GridSizeMode.Absolute, 10),
                                                    new Dimension(),
                                                    new Dimension(GridSizeMode.Absolute, 10),
                                                    new Dimension()
                                                ],
                                                Content = new[]
                                                {
                                                    new Drawable?[]
                                                    {
                                                        new GridContainer
                                                        {
                                                            RelativeSizeAxes = Axes.Both,
                                                            Anchor = Anchor.Centre,
                                                            Origin = Anchor.Centre,
                                                            RowDimensions =
                                                            [
                                                                new Dimension(),
                                                                new Dimension()
                                                            ],
                                                            Content = new[]
                                                            {
                                                                new Drawable[]
                                                                {
                                                                    new DailyChallengeCarousel
                                                                    {
                                                                        RelativeSizeAxes = Axes.Both,
                                                                        Anchor = Anchor.Centre,
                                                                        Origin = Anchor.Centre,
                                                                        Children = new Drawable[]
                                                                        {
                                                                            new DailyChallengeTimeRemainingRing(),
                                                                            breakdown = new DailyChallengeScoreBreakdown(),
                                                                            totals = new DailyChallengeTotalsDisplay(),
                                                                        }
                                                                    }
                                                                },
                                                                [
                                                                    feed = new DailyChallengeEventFeed
                                                                    {
                                                                        RelativeSizeAxes = Axes.Both,
                                                                        PresentScore = presentScore
                                                                    }
                                                                ],
                                                            },
                                                        },
                                                        null,
                                                        // Middle column (leaderboard)
                                                        leaderboard = new DailyChallengeLeaderboard(room, playlistItem)
                                                        {
                                                            RelativeSizeAxes = Axes.Both,
                                                            PresentScore = presentScore,
                                                            SelectedMods = { BindTarget = userMods },
                                                        },
                                                        // Spacer
                                                        null,
                                                        // Main right column
                                                        new GridContainer
                                                        {
                                                            RelativeSizeAxes = Axes.Both,
                                                            Content = new[]
                                                            {
                                                                new Drawable[]
                                                                {
                                                                    new SectionHeader("Chat")
                                                                },
                                                                [new MatchChatDisplay(room) { RelativeSizeAxes = Axes.Both }]
                                                            },
                                                            RowDimensions =
                                                            [
                                                                new Dimension(GridSizeMode.AutoSize),
                                                                new Dimension()
                                                            ]
                                                        },
                                                    }
                                                }
                                            }
                                        }
                                    }
                                ],
                                null,
                                [
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Padding = new MarginPadding
                                        {
                                            Horizontal = -WaveOverlayContainer.WIDTH_PADDING,
                                        },
                                        Children = new Drawable[]
                                        {
                                            new Box
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                Colour = colourProvider.Background5,
                                            },
                                            footerButtons = new FillFlowContainer
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                Direction = FillDirection.Horizontal,
                                                Padding = new MarginPadding(5),
                                                Spacing = new Vector2(10),
                                                Children = new Drawable[]
                                                {
                                                    new PlaylistsReadyButton
                                                    {
                                                        Anchor = Anchor.Centre,
                                                        Origin = Anchor.Centre,
                                                        RelativeSizeAxes = Axes.Y,
                                                        Size = new Vector2(250, 1),
                                                        Action = startPlay
                                                    }
                                                }
                                            },
                                        }
                                    }
                                ],
                            }
                        }
                    }
                }
            };

            LoadComponent(userModsSelectOverlay = new RoomModSelectOverlay
            {
                Beatmap = { BindTarget = Beatmap },
                SelectedMods = { BindTarget = userMods },
                IsValidMod = _ => false
            });

            if (playlistItem.AllowedMods.Any())
            {
                footerButtons.Insert(-1, new UserModSelectButton
                {
                    Text = "Free mods",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Y,
                    Size = new Vector2(250, 1),
                    Action = () => userModsSelectOverlay.Show(),
                });

                var rulesetInstance = rulesets.GetRuleset(playlistItem.RulesetID)!.CreateInstance();
                var allowedMods = playlistItem.AllowedMods.Select(m => m.ToMod(rulesetInstance));
                userModsSelectOverlay.IsValidMod = leaderboard.IsValidMod = m => allowedMods.Any(a => a.GetType() == m.GetType());
            }

            metadataClient.MultiplayerRoomScoreSet += onRoomScoreSet;
            dailyChallengeInfo.BindTo(metadataClient.DailyChallengeInfo);

            ((IBindable<MultiplayerScore?>)breakdown.UserBestScore).BindTo(leaderboard.UserBestScore);
        }

        private void presentScore(long id)
        {
            if (this.IsCurrentScreen())
                this.Push(new PlaylistItemScoreResultsScreen(room.RoomID.Value!.Value, playlistItem, id));
        }

        private void onRoomScoreSet(MultiplayerRoomScoreSetEvent e)
        {
            if (e.RoomID != room.RoomID.Value || e.PlaylistItemID != playlistItem.ID)
                return;

            userLookupCache.GetUserAsync(e.UserID).ContinueWith(t =>
            {
                if (t.Exception != null)
                {
                    Logger.Log($@"Could not display room score set event: {t.Exception}", LoggingTarget.Network);
                    return;
                }

                APIUser? user = t.GetResultSafely();
                if (user == null) return;

                var ev = new NewScoreEvent(e.ScoreID, user, e.TotalScore, e.NewRank);
                Schedule(() =>
                {
                    breakdown.AddNewScore(ev);
                    totals.AddNewScore(ev);
                    feed.AddNewScore(ev);

                    if (e.NewRank <= 50)
                        Scheduler.AddOnce(() => leaderboard.RefetchScores());
                });
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            beatmapAvailabilityTracker.SelectedItem.Value = playlistItem;
            beatmapAvailabilityTracker.Availability.BindValueChanged(_ => TrySetDailyChallengeBeatmap(this, beatmapManager, rulesets, musicController, playlistItem), true);

            userModsSelectOverlayRegistration = overlayManager?.RegisterBlockingOverlay(userModsSelectOverlay);
            userModsSelectOverlay.SelectedItem.Value = playlistItem;
            userMods.BindValueChanged(_ => Scheduler.AddOnce(updateMods), true);

            apiState.BindTo(API.State);
            apiState.BindValueChanged(onlineStateChanged, true);

            dailyChallengeInfo.BindValueChanged(dailyChallengeChanged);
        }

        private void onlineStateChanged(ValueChangedEvent<APIState> state) => Schedule(() =>
        {
            if (state.NewValue != APIState.Online)
                Schedule(forcefullyExit);
        });

        private void dailyChallengeChanged(ValueChangedEvent<DailyChallengeInfo?> change)
        {
            if (change.OldValue?.RoomID == room.RoomID.Value && change.NewValue == null)
            {
                notificationOverlay?.Post(new SimpleNotification { Text = DailyChallengeStrings.ChallengeEndedNotification });
            }
        }

        private void forcefullyExit()
        {
            Logger.Log(@$"{this} forcefully exiting due to loss of API connection");

            // This is temporary since we don't currently have a way to force screens to be exited
            // See also: `OnlinePlayScreen.forcefullyExit()`
            if (this.IsCurrentScreen())
            {
                while (this.IsCurrentScreen())
                    this.Exit();
            }
        }

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);

            waves.Show();
            roomManager.JoinRoom(room);
            startLoopingTrack(this, musicController);

            metadataClient.BeginWatchingMultiplayerRoom(room.RoomID.Value!.Value).ContinueWith(t =>
            {
                if (t.Exception != null)
                {
                    Logger.Error(t.Exception, @"Failed to subscribe to room updates", LoggingTarget.Network);
                    return;
                }

                MultiplayerPlaylistItemStats[] stats = t.GetResultSafely();
                var itemStats = stats.SingleOrDefault(item => item.PlaylistItemID == playlistItem.ID);

                if (itemStats == null) return;

                Schedule(() =>
                {
                    breakdown.SetInitialCounts(itemStats.TotalScoreDistribution);
                    totals.SetInitialCounts(itemStats.TotalScoreDistribution.Sum(c => c), itemStats.CumulativeScore);
                });
            }, TaskContinuationOptions.OnlyOnRanToCompletion);

            userModsSelectOverlay.SelectedItem.Value = playlistItem;

            TrySetDailyChallengeBeatmap(this, beatmapManager, rulesets, musicController, playlistItem);
        }

        public override void OnResuming(ScreenTransitionEvent e)
        {
            base.OnResuming(e);
            startLoopingTrack(this, musicController);
            // re-apply mods as they may have been changed by a child screen
            // (one known instance of this is showing a replay).
            updateMods();
        }

        public override void OnSuspending(ScreenTransitionEvent e)
        {
            base.OnSuspending(e);

            userModsSelectOverlay.Hide();
            cancelTrackLooping();
            previewTrackManager.StopAnyPlaying(this);
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            waves.Hide();
            userModsSelectOverlay.Hide();
            cancelTrackLooping();
            previewTrackManager.StopAnyPlaying(this);
            this.Delay(WaveContainer.DISAPPEAR_DURATION).FadeOut();

            roomManager.PartRoom();
            metadataClient.EndWatchingMultiplayerRoom(room.RoomID.Value!.Value).FireAndForget();

            return base.OnExiting(e);
        }

        public static void TrySetDailyChallengeBeatmap(OsuScreen screen, BeatmapManager beatmaps, RulesetStore rulesets, MusicController music, PlaylistItem item)
        {
            if (!screen.IsCurrentScreen())
                return;

            var beatmap = beatmaps.QueryBeatmap(b => b.OnlineID == item.Beatmap.OnlineID);

            screen.Beatmap.Value = beatmaps.GetWorkingBeatmap(beatmap); // this will gracefully fall back to dummy beatmap if missing locally.
            screen.Ruleset.Value = rulesets.GetRuleset(item.RulesetID);

            startLoopingTrack(screen, music);
        }

        private static void startLoopingTrack(OsuScreen screen, MusicController music)
        {
            if (!screen.IsCurrentScreen())
                return;

            var track = screen.Beatmap.Value?.Track;

            if (track != null)
            {
                screen.Beatmap.Value?.PrepareTrackForPreview(true);
                music.EnsurePlayingSomething();
            }
        }

        private void cancelTrackLooping()
        {
            var track = Beatmap.Value?.Track;

            if (track != null)
                track.Looping = false;
        }

        private void updateMods()
        {
            if (!this.IsCurrentScreen())
                return;

            Mods.Value = userMods.Value.Concat(playlistItem.RequiredMods.Select(m => m.ToMod(Ruleset.Value.CreateInstance()))).ToList();
        }

        private void startPlay()
        {
            sampleStart?.Play();
            this.Push(new PlayerLoader(() => new PlaylistsPlayer(room, playlistItem)
            {
                Exited = () => Scheduler.AddOnce(() => leaderboard.RefetchScores())
            }));
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            userModsSelectOverlayRegistration?.Dispose();

            if (metadataClient.IsNotNull())
                metadataClient.MultiplayerRoomScoreSet -= onRoomScoreSet;
        }

        public void PresentBeatmap(WorkingBeatmap beatmap, RulesetInfo ruleset)
        {
            if (!this.IsCurrentScreen())
                return;

            // We can only handle the current daily challenge beatmap.
            // If the import was for a different beatmap, pass the duty off to global handling.
            if (beatmap.BeatmapSetInfo.OnlineID != playlistItem.Beatmap.BeatmapSet!.OnlineID)
            {
                this.Exit();
                game?.PresentBeatmap(beatmap.BeatmapSetInfo, b => b.ID == beatmap.BeatmapInfo.ID);
            }

            // And if we're handling, we don't really have much to do here.
        }
    }
}
