// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Game.Graphics.Cursor;
using osu.Game.Input;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Components;
using osu.Game.Screens.OnlinePlay.Match;
using osu.Game.Screens.OnlinePlay.Match.Components;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.HUD;
using osu.Game.Users;
using osuTK;
using Container = osu.Framework.Graphics.Containers.Container;

namespace osu.Game.Screens.OnlinePlay.Playlists
{
    public partial class PlaylistsRoomSubScreen : RoomSubScreen
    {
        public override string Title { get; }

        public override string ShortTitle => "playlist";

        private readonly IBindable<bool> isIdle = new BindableBool();

        [Resolved(CanBeNull = true)]
        private IdleTracker? idleTracker { get; set; }

        private MatchLeaderboard leaderboard = null!;
        private SelectionPollingComponent selectionPollingComponent = null!;
        private FillFlowContainer progressSection = null!;
        private DrawableRoomPlaylist drawablePlaylist = null!;

        public PlaylistsRoomSubScreen(Room room)
            : base(room, false) // Editing is temporarily not allowed.
        {
            Title = room.RoomID == null ? "New playlist" : room.Name;
            Activity.Value = new UserActivity.InLobby(room);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (idleTracker != null)
                isIdle.BindTo(idleTracker.IsIdle);

            AddInternal(selectionPollingComponent = new SelectionPollingComponent(Room));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            isIdle.BindValueChanged(_ => updatePollingRate(), true);

            Room.PropertyChanged += onRoomPropertyChanged;
            updateSetupState();
            updateRoomMaxAttempts();
            updateRoomPlaylist();
        }

        private void onRoomPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Room.RoomID):
                    updateSetupState();
                    break;

                case nameof(Room.MaxAttempts):
                    updateRoomMaxAttempts();
                    break;

                case nameof(Room.Playlist):
                    updateRoomPlaylist();
                    break;
            }
        }

        private void updateSetupState()
        {
            if (Room.RoomID != null)
            {
                // Set the first playlist item.
                // This is scheduled since updating the room and playlist may happen in an arbitrary order (via Room.CopyFrom()).
                Schedule(() => SelectedItem.Value = Room.Playlist.FirstOrDefault());
            }
        }

        private void updateRoomMaxAttempts()
            => progressSection.Alpha = Room.MaxAttempts != null ? 1 : 0;

        private void updateRoomPlaylist()
            => drawablePlaylist.Items.ReplaceRange(0, drawablePlaylist.Items.Count, Room.Playlist);

        protected override Drawable CreateMainContent() => new Container
        {
            RelativeSizeAxes = Axes.Both,
            Padding = new MarginPadding { Horizontal = 5, Vertical = 10 },
            Child = new OsuContextMenuContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    ColumnDimensions = new[]
                    {
                        new Dimension(),
                        new Dimension(GridSizeMode.Absolute, 10),
                        new Dimension(),
                        new Dimension(GridSizeMode.Absolute, 10),
                        new Dimension(),
                    },
                    Content = new[]
                    {
                        new Drawable?[]
                        {
                            // Playlist items column
                            new GridContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                Padding = new MarginPadding { Right = 5 },
                                Content = new[]
                                {
                                    new Drawable[] { new OverlinedPlaylistHeader(Room), },
                                    new Drawable[]
                                    {
                                        drawablePlaylist = new DrawableRoomPlaylist
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            SelectedItem = { BindTarget = SelectedItem },
                                            AllowSelection = true,
                                            AllowShowingResults = true,
                                            RequestResults = item =>
                                            {
                                                Debug.Assert(Room.RoomID != null);
                                                ParentScreen?.Push(new PlaylistItemUserResultsScreen(null, Room.RoomID.Value, item));
                                            }
                                        }
                                    },
                                },
                                RowDimensions = new[]
                                {
                                    new Dimension(GridSizeMode.AutoSize),
                                    new Dimension(),
                                }
                            },
                            // Spacer
                            null,
                            // Middle column (mods and leaderboard)
                            new GridContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                Content = new[]
                                {
                                    new[]
                                    {
                                        UserModsSection = new FillFlowContainer
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            Alpha = 0,
                                            Margin = new MarginPadding { Bottom = 10 },
                                            Children = new Drawable[]
                                            {
                                                new OverlinedHeader("Extra mods"),
                                                new FillFlowContainer
                                                {
                                                    AutoSizeAxes = Axes.Both,
                                                    Direction = FillDirection.Horizontal,
                                                    Spacing = new Vector2(10, 0),
                                                    Children = new Drawable[]
                                                    {
                                                        new UserModSelectButton
                                                        {
                                                            Anchor = Anchor.CentreLeft,
                                                            Origin = Anchor.CentreLeft,
                                                            Width = 90,
                                                            Text = "Select",
                                                            Action = ShowUserModSelect,
                                                        },
                                                        new ModDisplay
                                                        {
                                                            Anchor = Anchor.CentreLeft,
                                                            Origin = Anchor.CentreLeft,
                                                            Current = UserMods,
                                                            Scale = new Vector2(0.8f),
                                                        },
                                                    }
                                                }
                                            }
                                        },
                                    },
                                    new Drawable[]
                                    {
                                        progressSection = new FillFlowContainer
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            Alpha = 0,
                                            Margin = new MarginPadding { Bottom = 10 },
                                            Direction = FillDirection.Vertical,
                                            Children = new Drawable[]
                                            {
                                                new OverlinedHeader("Progress"),
                                                new RoomLocalUserInfo(Room),
                                            }
                                        },
                                    },
                                    new Drawable[]
                                    {
                                        new OverlinedHeader("Leaderboard")
                                    },
                                    new Drawable[] { leaderboard = new MatchLeaderboard(Room) { RelativeSizeAxes = Axes.Both }, },
                                },
                                RowDimensions = new[]
                                {
                                    new Dimension(GridSizeMode.AutoSize),
                                    new Dimension(GridSizeMode.AutoSize),
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
                                    new Drawable[] { new OverlinedHeader("Chat") },
                                    new Drawable[] { new MatchChatDisplay(Room) { RelativeSizeAxes = Axes.Both } }
                                },
                                RowDimensions = new[]
                                {
                                    new Dimension(GridSizeMode.AutoSize),
                                    new Dimension(),
                                }
                            },
                        },
                    },
                }
            }
        };

        protected override Drawable CreateFooter() => new PlaylistsRoomFooter(Room)
        {
            OnStart = StartPlay
        };

        protected override RoomSettingsOverlay CreateRoomSettingsOverlay(Room room) => new PlaylistsRoomSettingsOverlay(room)
        {
            EditPlaylist = () =>
            {
                if (this.IsCurrentScreen())
                    this.Push(new PlaylistsSongSelect(Room));
            },
        };

        private void updatePollingRate()
        {
            selectionPollingComponent.TimeBetweenPolls.Value = isIdle.Value ? 30000 : 5000;
            Logger.Log($"Polling adjusted (selection: {selectionPollingComponent.TimeBetweenPolls.Value})");
        }

        protected override Screen CreateGameplayScreen() => new PlayerLoader(() => new PlaylistsPlayer(Room, SelectedItem.Value)
        {
            Exited = () => leaderboard.RefetchScores()
        });

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            Room.PropertyChanged -= onRoomPropertyChanged;
        }
    }
}
