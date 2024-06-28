// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.OnlinePlay.Components;
using osu.Game.Screens.OnlinePlay.Match;
using osu.Game.Screens.OnlinePlay.Match.Components;
using osu.Game.Screens.OnlinePlay.Playlists;
using osu.Game.Screens.Play;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.DailyChallenge
{
    public partial class DailyChallenge : OsuScreen
    {
        private readonly Room room;
        private readonly PlaylistItem playlistItem;

        /// <summary>
        /// Any mods applied by/to the local user.
        /// </summary>
        private readonly Bindable<IReadOnlyList<Mod>> userMods = new Bindable<IReadOnlyList<Mod>>(Array.Empty<Mod>());

        private OnlinePlayScreenWaveContainer waves = null!;
        private MatchLeaderboard leaderboard = null!;
        private RoomModSelectOverlay userModsSelectOverlay = null!;
        private Sample? sampleStart;
        private IDisposable? userModsSelectOverlayRegistration;

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Plum);

        [Cached(Type = typeof(IRoomManager))]
        private RoomManager roomManager { get; set; }

        [Cached]
        private readonly OnlinePlayBeatmapAvailabilityTracker beatmapAvailabilityTracker = new OnlinePlayBeatmapAvailabilityTracker();

        [Resolved]
        private BeatmapManager beatmapManager { get; set; } = null!;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        [Resolved]
        private MusicController musicController { get; set; } = null!;

        [Resolved]
        private IOverlayManager? overlayManager { get; set; }

        public override bool DisallowExternalBeatmapRulesetChanges => true;

        public DailyChallenge(Room room)
        {
            this.room = room;
            playlistItem = room.Playlist.Single();
            roomManager = new RoomManager();
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
                    new GridContainer
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
                                            Colour = Color4Extensions.FromHex(@"3e3a44") // Temporary.
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
                                                    new DailyChallengeTimeRemainingRing
                                                    {
                                                        RelativeSizeAxes = Axes.Both,
                                                    },
                                                    null,
                                                    // Middle column (leaderboard)
                                                    new GridContainer
                                                    {
                                                        RelativeSizeAxes = Axes.Both,
                                                        Content = new[]
                                                        {
                                                            new Drawable[]
                                                            {
                                                                new SectionHeader("Leaderboard")
                                                            },
                                                            [leaderboard = new MatchLeaderboard { RelativeSizeAxes = Axes.Both }],
                                                        },
                                                        RowDimensions = new[]
                                                        {
                                                            new Dimension(GridSizeMode.AutoSize),
                                                            new Dimension(),
                                                        }
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
                                            Colour = Color4Extensions.FromHex(@"28242d") // Temporary.
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
            };

            LoadComponent(userModsSelectOverlay = new RoomModSelectOverlay
            {
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
                userModsSelectOverlay.IsValidMod = m => allowedMods.Any(a => a.GetType() == m.GetType());
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            beatmapAvailabilityTracker.SelectedItem.Value = playlistItem;
            beatmapAvailabilityTracker.Availability.BindValueChanged(_ => trySetDailyChallengeBeatmap(), true);

            userModsSelectOverlayRegistration = overlayManager?.RegisterBlockingOverlay(userModsSelectOverlay);
            userModsSelectOverlay.SelectedItem.Value = playlistItem;
            userMods.BindValueChanged(_ => Scheduler.AddOnce(updateMods), true);
        }

        private void trySetDailyChallengeBeatmap()
        {
            var beatmap = beatmapManager.QueryBeatmap(b => b.OnlineID == playlistItem.Beatmap.OnlineID);
            Beatmap.Value = beatmapManager.GetWorkingBeatmap(beatmap); // this will gracefully fall back to dummy beatmap if missing locally.
            Ruleset.Value = rulesets.GetRuleset(playlistItem.RulesetID);
        }

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);

            waves.Show();
            roomManager.JoinRoom(room);
            applyLoopingToTrack();
        }

        public override void OnResuming(ScreenTransitionEvent e)
        {
            base.OnResuming(e);
            applyLoopingToTrack();
        }

        public override void OnSuspending(ScreenTransitionEvent e)
        {
            base.OnSuspending(e);

            userModsSelectOverlay.Hide();
            cancelTrackLooping();
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            waves.Hide();
            userModsSelectOverlay.Hide();
            cancelTrackLooping();
            this.Delay(WaveContainer.DISAPPEAR_DURATION).FadeOut();

            roomManager.PartRoom();

            return base.OnExiting(e);
        }

        private void applyLoopingToTrack()
        {
            if (!this.IsCurrentScreen())
                return;

            var track = Beatmap.Value?.Track;

            if (track != null)
            {
                Beatmap.Value?.PrepareTrackForPreview(true);
                musicController.EnsurePlayingSomething();
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
                Exited = () => leaderboard.RefetchScores()
            }));
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            userModsSelectOverlayRegistration?.Dispose();
        }
    }
}
