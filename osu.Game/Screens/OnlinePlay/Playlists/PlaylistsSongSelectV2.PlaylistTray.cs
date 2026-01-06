// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osuTK;
using Container = osu.Framework.Graphics.Containers.Container;

namespace osu.Game.Screens.OnlinePlay.Playlists
{
    public partial class PlaylistsSongSelectV2
    {
        public partial class PlaylistTray : CompositeDrawable
        {
            private readonly Room room;

            private OsuScrollContainer scroll = null!;
            private FillFlowContainer flow = null!;
            private OsuSpriteText text = null!;

            private const float item_width = 250;

            public PlaylistTray(Room room)
            {
                this.room = room;
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                Size = new Vector2(500, 75);

                Masking = true;
                CornerRadius = 20;
                EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Shadow,
                    Colour = colourProvider.Background6.Opacity(0.2f),
                    Offset = new Vector2(2),
                    Radius = 8,
                };

                InternalChild = new BufferedContainer(pixelSnapping: true)
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colourProvider.Background3,
                        },
                        new GridContainer
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.X,
                            Height = DrawableRoomPlaylistItem.HEIGHT,
                            Padding = new MarginPadding { Horizontal = 10 },
                            ColumnDimensions = new[]
                            {
                                new Dimension(GridSizeMode.AutoSize),
                                new Dimension()
                            },
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    new Container
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        AutoSizeAxes = Axes.Both,
                                        Children = new Drawable[]
                                        {
                                            text = new OsuSpriteText
                                            {
                                                Font = OsuFont.Style.Heading2,
                                            },
                                            new OsuSpriteText
                                            {
                                                Y = 20,
                                                Font = OsuFont.Style.Caption2,
                                                Text = "Manage items on previous screen"
                                            },
                                        }
                                    },
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Children = new Drawable[]
                                        {
                                            scroll = new OsuScrollContainer(Direction.Horizontal)
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                ScrollbarVisible = false,
                                                Child = flow = new FillFlowContainer
                                                {
                                                    RelativeSizeAxes = Axes.Y,
                                                    AutoSizeAxes = Axes.X,
                                                    Padding = new MarginPadding { Left = item_width },
                                                    Spacing = new Vector2(5),
                                                    Direction = FillDirection.Horizontal
                                                }
                                            },
                                            new Box
                                            {
                                                Colour = ColourInfo.GradientHorizontal(colourProvider.Background3, colourProvider.Background3.Opacity(0)),
                                                RelativeSizeAxes = Axes.Y,
                                                X = -1,
                                                Width = 60,
                                            },
                                        }
                                    },
                                },
                            }
                        }
                    }
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                room.PropertyChanged += onRoomPropertyChanged;
                updateRoomPlaylist();

                this.FadeOut();
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
                    var newItem = new DrawableRoomPlaylistItem(room.Playlist[^1], loadImmediately: true)
                    {
                        RelativeSizeAxes = Axes.None,
                        Width = item_width,
                        AllowReordering = false,
                    };

                    if (flow.Count > 1)
                        flow[0].Expire();

                    flow.Add(newItem);

                    if (scroll.IsLoaded)
                        scroll.ScrollToStart(animated: false);
                    ScheduleAfterChildren(() => scroll.ScrollToEnd());

                    Scheduler.AddDelayed(() => text.Text = $"{room.Playlist.Count} item(s)", 100);
                }

                this.FadeIn(200)
                    .Delay(2000)
                    .FadeOut(200);
            }

            // Disallow the user from interacting with the scrolling elements.
            public override bool PropagatePositionalInputSubTree => false;
            public override bool PropagateNonPositionalInputSubTree => false;
        }
    }
}
