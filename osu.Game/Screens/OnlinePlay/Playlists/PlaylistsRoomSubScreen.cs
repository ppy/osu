// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
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

namespace osu.Game.Screens.OnlinePlay.Playlists
{
    public partial class PlaylistsRoomSubScreen : RoomSubScreen
    {
        public override string Title { get; }

        public override string ShortTitle => "playlist";

        private readonly IBindable<bool> isIdle = new BindableBool();

        private MatchLeaderboard leaderboard;
        private SelectionPollingComponent selectionPollingComponent;

        private FillFlowContainer progressSection;

        public PlaylistsRoomSubScreen(Room room)
            : base(room, false) // Editing is temporarily not allowed.
        {
            Title = room.RoomID.Value == null ? "New playlist" : room.Name.Value;
            Activity.Value = new UserActivity.InLobby(room);
        }

        [BackgroundDependencyLoader(true)]
        private void load([CanBeNull] IdleTracker idleTracker)
        {
            if (idleTracker != null)
                isIdle.BindTo(idleTracker.IsIdle);

            AddInternal(selectionPollingComponent = new SelectionPollingComponent(Room));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            isIdle.BindValueChanged(_ => updatePollingRate(), true);
            RoomId.BindValueChanged(id =>
            {
                if (id.NewValue != null)
                {
                    // Set the first playlist item.
                    // This is scheduled since updating the room and playlist may happen in an arbitrary order (via Room.CopyFrom()).
                    Schedule(() => SelectedItem.Value = Room.Playlist.FirstOrDefault());
                }
            }, true);

            Room.MaxAttempts.BindValueChanged(_ => progressSection.Alpha = Room.MaxAttempts.Value != null ? 1 : 0, true);
        }

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
                        new Drawable[]
                        {
                            // Playlist items column
                            new GridContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                Padding = new MarginPadding { Right = 5 },
                                Content = new[]
                                {
                                    new Drawable[] { new OverlinedPlaylistHeader(), },
                                    new Drawable[]
                                    {
                                        new DrawableRoomPlaylist
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Items = { BindTarget = Room.Playlist },
                                            SelectedItem = { BindTarget = SelectedItem },
                                            AllowSelection = true,
                                            AllowShowingResults = true,
                                            RequestResults = item =>
                                            {
                                                Debug.Assert(RoomId.Value != null);
                                                ParentScreen?.Push(new PlaylistsResultsScreen(null, RoomId.Value.Value, item));
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
                                                new RoomLocalUserInfo(),
                                            }
                                        },
                                    },
                                    new Drawable[]
                                    {
                                        new OverlinedHeader("Leaderboard")
                                    },
                                    new Drawable[] { leaderboard = new MatchLeaderboard { RelativeSizeAxes = Axes.Both }, },
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

        protected override Drawable CreateFooter() => new PlaylistsRoomFooter
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
    }
}
