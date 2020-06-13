// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.GameTypes;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Multi.Components;
using osu.Game.Screens.Multi.Match.Components;
using osu.Game.Screens.Multi.Play;
using osu.Game.Screens.Multi.Ranking;
using osu.Game.Screens.Play;
using osu.Game.Screens.Select;
using Footer = osu.Game.Screens.Multi.Match.Components.Footer;

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

        [Resolved(typeof(Room), nameof(Room.Type))]
        private Bindable<GameType> type { get; set; }

        [Resolved(typeof(Room), nameof(Room.Playlist))]
        private BindableList<PlaylistItem> playlist { get; set; }

        [Resolved]
        private BeatmapManager beatmapManager { get; set; }

        [Resolved(canBeNull: true)]
        private Multiplayer multiplayer { get; set; }

        protected readonly Bindable<PlaylistItem> SelectedItem = new Bindable<PlaylistItem>();

        private LeaderboardChatDisplay leaderboardChatDisplay;
        private MatchSettingsOverlay settingsOverlay;

        private IBindable<WeakReference<BeatmapSetInfo>> managerUpdated;

        public MatchSubScreen(Room room)
        {
            Title = room.RoomID.Value == null ? "New room" : room.Name.Value;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Padding = new MarginPadding
                                {
                                    Horizontal = 105,
                                    Vertical = 20
                                },
                                Child = new GridContainer
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Content = new[]
                                    {
                                        new Drawable[] { new Components.Header() },
                                        new Drawable[]
                                        {
                                            new Container
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                Padding = new MarginPadding { Top = 65 },
                                                Child = new GridContainer
                                                {
                                                    ColumnDimensions = new[]
                                                    {
                                                        new Dimension(minSize: 160),
                                                        new Dimension(minSize: 360),
                                                        new Dimension(minSize: 400),
                                                    },
                                                    RelativeSizeAxes = Axes.Both,
                                                    Content = new[]
                                                    {
                                                        new Drawable[]
                                                        {
                                                            new Container
                                                            {
                                                                RelativeSizeAxes = Axes.Both,
                                                                Padding = new MarginPadding { Right = 5 },
                                                                Child = new OverlinedParticipants(Direction.Vertical) { RelativeSizeAxes = Axes.Both }
                                                            },
                                                            new Container
                                                            {
                                                                RelativeSizeAxes = Axes.Both,
                                                                Padding = new MarginPadding { Horizontal = 5 },
                                                                Child = new GridContainer
                                                                {
                                                                    RelativeSizeAxes = Axes.Both,
                                                                    Content = new[]
                                                                    {
                                                                        new Drawable[]
                                                                        {
                                                                            new OverlinedPlaylist(true) // Temporarily always allow selection
                                                                            {
                                                                                RelativeSizeAxes = Axes.Both,
                                                                                SelectedItem = { BindTarget = SelectedItem }
                                                                            }
                                                                        },
                                                                        null,
                                                                        new Drawable[]
                                                                        {
                                                                            new TriangleButton
                                                                            {
                                                                                RelativeSizeAxes = Axes.X,
                                                                                Text = "Show beatmap results",
                                                                                Action = showBeatmapResults
                                                                            }
                                                                        }
                                                                    },
                                                                    RowDimensions = new[]
                                                                    {
                                                                        new Dimension(),
                                                                        new Dimension(GridSizeMode.Absolute, 5),
                                                                        new Dimension(GridSizeMode.AutoSize)
                                                                    }
                                                                }
                                                            },
                                                            new Container
                                                            {
                                                                RelativeSizeAxes = Axes.Both,
                                                                Padding = new MarginPadding { Left = 5 },
                                                                Child = leaderboardChatDisplay = new LeaderboardChatDisplay()
                                                            }
                                                        },
                                                    }
                                                }
                                            }
                                        }
                                    },
                                    RowDimensions = new[]
                                    {
                                        new Dimension(GridSizeMode.AutoSize),
                                        new Dimension(),
                                    }
                                }
                            }
                        },
                        new Drawable[]
                        {
                            new Footer
                            {
                                OnStart = onStart,
                                SelectedItem = { BindTarget = SelectedItem }
                            }
                        }
                    },
                    RowDimensions = new[]
                    {
                        new Dimension(),
                        new Dimension(GridSizeMode.AutoSize),
                    }
                },
                settingsOverlay = new MatchSettingsOverlay
                {
                    RelativeSizeAxes = Axes.Both,
                    EditPlaylist = () => this.Push(new MatchSongSelect()),
                    State = { Value = roomId.Value == null ? Visibility.Visible : Visibility.Hidden }
                }
            };
        }

        [Resolved]
        private IAPIProvider api { get; set; }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            roomId.BindValueChanged(id =>
            {
                if (id.NewValue == null)
                    settingsOverlay.Show();
                else
                {
                    settingsOverlay.Hide();

                    // Set the first playlist item.
                    // This is scheduled since updating the room and playlist may happen in an arbitrary order (via Room.CopyFrom()).
                    Schedule(() => SelectedItem.Value = playlist.FirstOrDefault());
                }
            }, true);

            SelectedItem.BindValueChanged(_ => Scheduler.AddOnce(selectedItemChanged));
            SelectedItem.Value = playlist.FirstOrDefault();

            managerUpdated = beatmapManager.ItemUpdated.GetBoundCopy();
            managerUpdated.BindValueChanged(beatmapUpdated);
        }

        public override bool OnExiting(IScreen next)
        {
            RoomManager?.PartRoom();
            Mods.Value = Array.Empty<Mod>();

            return base.OnExiting(next);
        }

        private void selectedItemChanged()
        {
            updateWorkingBeatmap();

            var item = SelectedItem.Value;

            Mods.Value = item?.RequiredMods?.ToArray() ?? Array.Empty<Mod>();

            if (item?.Ruleset != null)
                Ruleset.Value = item.Ruleset.Value;
        }

        private void beatmapUpdated(ValueChangedEvent<WeakReference<BeatmapSetInfo>> weakSet) => Schedule(updateWorkingBeatmap);

        private void updateWorkingBeatmap()
        {
            var beatmap = SelectedItem.Value?.Beatmap.Value;

            // Retrieve the corresponding local beatmap, since we can't directly use the playlist's beatmap info
            var localBeatmap = beatmap == null ? null : beatmapManager.QueryBeatmap(b => b.OnlineBeatmapID == beatmap.OnlineBeatmapID);

            Beatmap.Value = beatmapManager.GetWorkingBeatmap(localBeatmap);
        }

        private void onStart()
        {
            switch (type.Value)
            {
                default:
                case GameTypeTimeshift _:
                    multiplayer?.Push(new PlayerLoader(() => new TimeshiftPlayer(SelectedItem.Value)
                    {
                        Exited = () => leaderboardChatDisplay.RefreshScores()
                    }));
                    break;
            }
        }

        private void showBeatmapResults()
        {
            Debug.Assert(roomId.Value != null);
            multiplayer?.Push(new TimeshiftResultsScreen(null, roomId.Value.Value, SelectedItem.Value, false));
        }
    }
}
