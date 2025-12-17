// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Rooms;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Playlists
{
    public partial class PlaylistsSongSelectV2
    {
        public partial class PlaylistTray : CompositeDrawable
        {
            private readonly Room room;

            private OsuScrollContainer scroll = null!;
            private FillFlowContainer flow = null!;

            public PlaylistTray(Room room)
            {
                this.room = room;

                Size = new Vector2(400, 75);
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                Masking = true;
                CornerRadius = 20;

                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black,
                        Alpha = 0.9f
                    },
                    new GridContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.X,
                        Height = DrawableRoomPlaylistItem.HEIGHT,
                        Padding = new MarginPadding { Horizontal = 20 },
                        ColumnDimensions = new[]
                        {
                            new Dimension(GridSizeMode.AutoSize),
                            new Dimension()
                        },
                        Content = new[]
                        {
                            new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Text = "Playlist",
                                    Font = OsuFont.Default.With(size: 20)
                                },
                                scroll = new OsuScrollContainer(Direction.Horizontal)
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Margin = new MarginPadding { Left = 10 },
                                    ScrollbarVisible = false,
                                    Child = flow = new FillFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.Y,
                                        AutoSizeAxes = Axes.X,
                                        Direction = FillDirection.Horizontal
                                    }
                                }
                            },
                        }
                    }
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                room.PropertyChanged += onRoomPropertyChanged;
                updateRoomPlaylist();
            }

            private void onRoomPropertyChanged(object? sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == nameof(Room.Playlist))
                    updateRoomPlaylist();
            }

            private void updateRoomPlaylist()
            {
                if (room.Playlist.Count > 0)
                {
                    flow.Add(new DrawableRoomPlaylistItem(room.Playlist[^1], loadImmediately: true)
                    {
                        RelativeSizeAxes = Axes.None,
                        Width = 250,
                        AllowReordering = false,
                    });
                }

                scroll.ScrollToStart(animated: false);

                this.FadeIn(200);
                ScheduleAfterChildren(() => scroll.ScrollToEnd());
                this.Delay(1000).FadeOut(200);
            }

            // Disallow the user from interacting with the scrolling elements.
            public override bool PropagatePositionalInputSubTree => false;
            public override bool PropagateNonPositionalInputSubTree => false;
        }
    }
}
