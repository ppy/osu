// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.GameTypes;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Multi.Match.Components;
using osu.Game.Screens.Multi.Play;
using osu.Game.Screens.Select;
using PlaylistItem = osu.Game.Online.Multiplayer.PlaylistItem;

namespace osu.Game.Screens.Multi.Match
{
    [Cached(typeof(IPreviewTrackOwner))]
    public class MatchSubScreen : MultiplayerSubScreen, IPreviewTrackOwner
    {
        public override bool DisallowExternalBeatmapRulesetChanges => true;

        public override string Title { get; }

        public override string ShortTitle => "room";

        [Resolved(typeof(Room), nameof(Room.RoomID))]
        private Bindable<int?> roomId { get; set; }

        [Resolved(typeof(Room), nameof(Room.Name))]
        private Bindable<string> name { get; set; }

        [Resolved(typeof(Room), nameof(Room.Type))]
        private Bindable<GameType> type { get; set; }

        [Resolved(typeof(Room))]
        protected BindableList<PlaylistItem> Playlist { get; private set; }

        [Resolved(typeof(Room))]
        protected Bindable<PlaylistItem> CurrentItem { get; private set; }

        [Resolved]
        private BeatmapManager beatmapManager { get; set; }

        [Resolved]
        private PreviewTrackManager previewTrackManager { get; set; }

        [Resolved(CanBeNull = true)]
        private OsuGame game { get; set; }

        private MatchLeaderboard leaderboard;

        public MatchSubScreen(Room room)
        {
            Title = room.RoomID.Value == null ? "New room" : room.Name.Value;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Components.Header header;
            Info info;
            GridContainer bottomRow;
            MatchSettingsOverlay settings;

            InternalChildren = new Drawable[]
            {
                new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            header = new Components.Header
                            {
                                Depth = -1,
                                RequestBeatmapSelection = () =>
                                {
                                    this.Push(new MatchSongSelect
                                    {
                                        Selected = item =>
                                        {
                                            Playlist.Clear();
                                            Playlist.Add(item);
                                        }
                                    });
                                }
                            }
                        },
                        new Drawable[] { info = new Info { OnStart = onStart } },
                        new Drawable[]
                        {
                            bottomRow = new GridContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                Content = new[]
                                {
                                    new Drawable[]
                                    {
                                        leaderboard = new MatchLeaderboard
                                        {
                                            Padding = new MarginPadding
                                            {
                                                Left = 10 + HORIZONTAL_OVERFLOW_PADDING,
                                                Right = 10,
                                                Vertical = 10,
                                            },
                                            RelativeSizeAxes = Axes.Both
                                        },
                                        new Container
                                        {
                                            Padding = new MarginPadding
                                            {
                                                Left = 10,
                                                Right = 10 + HORIZONTAL_OVERFLOW_PADDING,
                                                Vertical = 10,
                                            },
                                            RelativeSizeAxes = Axes.Both,
                                            Child = new MatchChatDisplay
                                            {
                                                RelativeSizeAxes = Axes.Both
                                            }
                                        },
                                    },
                                },
                            }
                        },
                    },
                    RowDimensions = new[]
                    {
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(GridSizeMode.Distributed),
                    }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Top = Components.Header.HEIGHT },
                    Child = settings = new MatchSettingsOverlay { RelativeSizeAxes = Axes.Both },
                },
            };

            header.Tabs.Current.BindValueChanged(tab =>
            {
                const float fade_duration = 500;

                var settingsDisplayed = tab.NewValue is SettingsMatchPage;

                header.ShowBeatmapPanel.Value = !settingsDisplayed;
                settings.State.Value = settingsDisplayed ? Visibility.Visible : Visibility.Hidden;
                info.FadeTo(settingsDisplayed ? 0 : 1, fade_duration, Easing.OutQuint);
                bottomRow.FadeTo(settingsDisplayed ? 0 : 1, fade_duration, Easing.OutQuint);
            }, true);

            beatmapManager.ItemAdded += beatmapAdded;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            CurrentItem.BindValueChanged(currentItemChanged, true);
        }

        public override bool OnExiting(IScreen next)
        {
            RoomManager?.PartRoom();
            Mods.Value = Array.Empty<Mod>();
            previewTrackManager.StopAnyPlaying(this);

            return base.OnExiting(next);
        }

        /// <summary>
        /// Handles propagation of the current playlist item's content to game-wide mechanisms.
        /// </summary>
        private void currentItemChanged(ValueChangedEvent<PlaylistItem> e)
        {
            // Retrieve the corresponding local beatmap, since we can't directly use the playlist's beatmap info
            var localBeatmap = e.NewValue?.Beatmap == null ? null : beatmapManager.QueryBeatmap(b => b.OnlineBeatmapID == e.NewValue.Beatmap.OnlineBeatmapID);

            Beatmap.Value = beatmapManager.GetWorkingBeatmap(localBeatmap);
            Mods.Value = e.NewValue?.RequiredMods?.ToArray() ?? Array.Empty<Mod>();

            if (e.NewValue?.Ruleset != null)
                Ruleset.Value = e.NewValue.Ruleset;

            previewTrackManager.StopAnyPlaying(this);
        }

        /// <summary>
        /// Handle the case where a beatmap is imported (and can be used by this match).
        /// </summary>
        private void beatmapAdded(BeatmapSetInfo model) => Schedule(() =>
        {
            if (Beatmap.Value != beatmapManager.DefaultBeatmap)
                return;

            if (CurrentItem.Value == null)
                return;

            // Try to retrieve the corresponding local beatmap
            var localBeatmap = beatmapManager.QueryBeatmap(b => b.OnlineBeatmapID == CurrentItem.Value.Beatmap.OnlineBeatmapID);

            if (localBeatmap != null)
                Beatmap.Value = beatmapManager.GetWorkingBeatmap(localBeatmap);
        });

        [Resolved(canBeNull: true)]
        private Multiplayer multiplayer { get; set; }

        private void onStart()
        {
            previewTrackManager.StopAnyPlaying(this);

            switch (type.Value)
            {
                default:
                case GameTypeTimeshift _:
                    multiplayer?.Start(() => new TimeshiftPlayer(CurrentItem.Value)
                    {
                        Exited = () => leaderboard.RefreshScores()
                    });
                    break;
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (beatmapManager != null)
                beatmapManager.ItemAdded -= beatmapAdded;
        }
    }
}
