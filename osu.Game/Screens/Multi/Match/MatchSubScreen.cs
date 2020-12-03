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
using osu.Game.Users;
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

        private MatchSettingsOverlay settingsOverlay;
        private MatchLeaderboard leaderboard;

        private IBindable<WeakReference<BeatmapSetInfo>> managerUpdated;
        private OverlinedHeader participantsHeader;

        public MatchSubScreen(Room room)
        {
            Title = room.RoomID.Value == null ? "New room" : room.Name.Value;
            Activity.Value = new UserActivity.InLobby(room);
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
                                    RowDimensions = new[]
                                    {
                                        new Dimension(GridSizeMode.AutoSize),
                                        new Dimension(GridSizeMode.AutoSize),
                                        new Dimension(GridSizeMode.AutoSize),
                                        new Dimension(),
                                    },
                                    Content = new[]
                                    {
                                        new Drawable[] { new Components.Header() },
                                        new Drawable[]
                                        {
                                            participantsHeader = new OverlinedHeader("Participants")
                                            {
                                                ShowLine = false
                                            }
                                        },
                                        new Drawable[]
                                        {
                                            new Container
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                AutoSizeAxes = Axes.Y,
                                                Margin = new MarginPadding { Top = 5 },
                                                Child = new ParticipantsDisplay(Direction.Horizontal)
                                                {
                                                    Details = { BindTarget = participantsHeader.Details }
                                                }
                                            }
                                        },
                                        new Drawable[]
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
                                                            Padding = new MarginPadding { Right = 5 },
                                                            Child = new GridContainer
                                                            {
                                                                RelativeSizeAxes = Axes.Both,
                                                                Content = new[]
                                                                {
                                                                    new Drawable[] { new OverlinedPlaylistHeader(), },
                                                                    new Drawable[]
                                                                    {
                                                                        new DrawableRoomPlaylistWithResults
                                                                        {
                                                                            RelativeSizeAxes = Axes.Both,
                                                                            Items = { BindTarget = playlist },
                                                                            SelectedItem = { BindTarget = SelectedItem },
                                                                            RequestShowResults = item =>
                                                                            {
                                                                                Debug.Assert(roomId.Value != null);
                                                                                multiplayer?.Push(new TimeshiftResultsScreen(null, roomId.Value.Value, item, false));
                                                                            }
                                                                        }
                                                                    },
                                                                },
                                                                RowDimensions = new[]
                                                                {
                                                                    new Dimension(GridSizeMode.AutoSize),
                                                                    new Dimension(),
                                                                }
                                                            }
                                                        },
                                                        null,
                                                        new GridContainer
                                                        {
                                                            RelativeSizeAxes = Axes.Both,
                                                            Content = new[]
                                                            {
                                                                new Drawable[] { new OverlinedHeader("Leaderboard"), },
                                                                new Drawable[] { leaderboard = new MatchLeaderboard { RelativeSizeAxes = Axes.Both }, },
                                                                new Drawable[] { new OverlinedHeader("Chat"), },
                                                                new Drawable[] { new MatchChatDisplay { RelativeSizeAxes = Axes.Both } }
                                                            },
                                                            RowDimensions = new[]
                                                            {
                                                                new Dimension(GridSizeMode.AutoSize),
                                                                new Dimension(),
                                                                new Dimension(GridSizeMode.AutoSize),
                                                                new Dimension(GridSizeMode.Relative, size: 0.4f, minSize: 120),
                                                            }
                                                        },
                                                        null
                                                    },
                                                },
                                                ColumnDimensions = new[]
                                                {
                                                    new Dimension(GridSizeMode.Relative, size: 0.5f, maxSize: 400),
                                                    new Dimension(),
                                                    new Dimension(GridSizeMode.Relative, size: 0.5f, maxSize: 600),
                                                    new Dimension(),
                                                }
                                            }
                                        }
                                    },
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
                        Exited = () => leaderboard.RefreshScores()
                    }));
                    break;
            }
        }
    }
}
