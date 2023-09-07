// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Chat;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Screens.OnlinePlay.Components;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Lounge.Components
{
    public partial class DrawableRoom : CompositeDrawable
    {
        protected const float CORNER_RADIUS = 10;
        private const float height = 100;

        public readonly Room Room;

        protected Container ButtonsContainer { get; private set; } = null!;

        private readonly Bindable<MatchType> roomType = new Bindable<MatchType>();
        private readonly Bindable<RoomCategory> roomCategory = new Bindable<RoomCategory>();
        private readonly Bindable<bool> hasPassword = new Bindable<bool>();

        private DrawableRoomParticipantsList? drawableRoomParticipantsList;
        private RoomSpecialCategoryPill specialCategoryPill = null!;
        private PasswordProtectedIcon passwordIcon = null!;
        private EndDateInfo endDateInfo = null!;

        private DelayedLoadWrapper wrapper = null!;

        public DrawableRoom(Room room)
        {
            Room = room;

            RelativeSizeAxes = Axes.X;
            Height = height;

            Masking = true;
            CornerRadius = CORNER_RADIUS;
            EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Shadow,
                Colour = Color4.Black.Opacity(40),
                Radius = 5,
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colours)
        {
            ButtonsContainer = new Container
            {
                Anchor = Anchor.CentreRight,
                Origin = Anchor.CentreRight,
                RelativeSizeAxes = Axes.Y,
                AutoSizeAxes = Axes.X
            };

            InternalChildren = new[]
            {
                // This resolves internal 1px gaps due to applying the (parenting) corner radius and masking across multiple filling background sprites.
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colours.Background5,
                },
                CreateBackground().With(d =>
                {
                    d.RelativeSizeAxes = Axes.Both;
                }),
                wrapper = new DelayedLoadWrapper(() =>
                    new Container
                    {
                        Name = @"Room content",
                        RelativeSizeAxes = Axes.Both,
                        // This negative padding resolves 1px gaps between this background and the background above.
                        Padding = new MarginPadding { Left = 20, Vertical = -0.5f },
                        Child = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Masking = true,
                            CornerRadius = CORNER_RADIUS,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = colours.Background5,
                                    Width = 0.2f,
                                },
                                new Box
                                {
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = ColourInfo.GradientHorizontal(colours.Background5, colours.Background5.Opacity(0.3f)),
                                    Width = 0.8f,
                                },
                                new GridContainer
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    ColumnDimensions = new[]
                                    {
                                        new Dimension(),
                                        new Dimension(GridSizeMode.AutoSize),
                                    },
                                    Content = new[]
                                    {
                                        new Drawable[]
                                        {
                                            new Container
                                            {
                                                Name = @"Left details",
                                                RelativeSizeAxes = Axes.Both,
                                                Padding = new MarginPadding
                                                {
                                                    Left = 20,
                                                    Right = DrawableRoomParticipantsList.SHEAR_WIDTH,
                                                    Vertical = 5
                                                },
                                                Children = new Drawable[]
                                                {
                                                    new FillFlowContainer
                                                    {
                                                        RelativeSizeAxes = Axes.X,
                                                        AutoSizeAxes = Axes.Y,
                                                        Direction = FillDirection.Vertical,
                                                        Children = new Drawable[]
                                                        {
                                                            new FillFlowContainer
                                                            {
                                                                AutoSizeAxes = Axes.Both,
                                                                Direction = FillDirection.Horizontal,
                                                                Spacing = new Vector2(5),
                                                                Children = new Drawable[]
                                                                {
                                                                    new RoomStatusPill
                                                                    {
                                                                        Anchor = Anchor.CentreLeft,
                                                                        Origin = Anchor.CentreLeft
                                                                    },
                                                                    specialCategoryPill = new RoomSpecialCategoryPill
                                                                    {
                                                                        Anchor = Anchor.CentreLeft,
                                                                        Origin = Anchor.CentreLeft
                                                                    },
                                                                    endDateInfo = new EndDateInfo
                                                                    {
                                                                        Anchor = Anchor.CentreLeft,
                                                                        Origin = Anchor.CentreLeft,
                                                                    },
                                                                }
                                                            },
                                                            new FillFlowContainer
                                                            {
                                                                RelativeSizeAxes = Axes.X,
                                                                AutoSizeAxes = Axes.Y,
                                                                Padding = new MarginPadding { Top = 3 },
                                                                Direction = FillDirection.Vertical,
                                                                Children = new Drawable[]
                                                                {
                                                                    new TruncatingSpriteText
                                                                    {
                                                                        RelativeSizeAxes = Axes.X,
                                                                        Font = OsuFont.GetFont(size: 28),
                                                                        Current = { BindTarget = Room.Name }
                                                                    },
                                                                    new RoomStatusText()
                                                                }
                                                            }
                                                        },
                                                    },
                                                    new FillFlowContainer
                                                    {
                                                        Anchor = Anchor.BottomLeft,
                                                        Origin = Anchor.BottomLeft,
                                                        AutoSizeAxes = Axes.Both,
                                                        Direction = FillDirection.Horizontal,
                                                        Spacing = new Vector2(5),
                                                        ChildrenEnumerable = CreateBottomDetails()
                                                    }
                                                }
                                            },
                                            new FillFlowContainer
                                            {
                                                Name = "Right content",
                                                Anchor = Anchor.CentreRight,
                                                Origin = Anchor.CentreRight,
                                                AutoSizeAxes = Axes.X,
                                                RelativeSizeAxes = Axes.Y,
                                                Spacing = new Vector2(5),
                                                Padding = new MarginPadding
                                                {
                                                    Right = 10,
                                                    Vertical = 20,
                                                },
                                                Children = new Drawable[]
                                                {
                                                    ButtonsContainer,
                                                    drawableRoomParticipantsList = new DrawableRoomParticipantsList
                                                    {
                                                        Anchor = Anchor.CentreRight,
                                                        Origin = Anchor.CentreRight,
                                                        NumberOfCircles = NumberOfAvatars
                                                    }
                                                }
                                            },
                                        }
                                    }
                                },
                                passwordIcon = new PasswordProtectedIcon { Alpha = 0 }
                            },
                        },
                    }, 0)
                {
                    RelativeSizeAxes = Axes.Both,
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            wrapper.DelayedLoadComplete += _ =>
            {
                wrapper.FadeInFromZero(200);

                roomCategory.BindTo(Room.Category);
                roomCategory.BindValueChanged(c =>
                {
                    if (c.NewValue > RoomCategory.Normal)
                        specialCategoryPill.Show();
                    else
                        specialCategoryPill.Hide();
                }, true);

                roomType.BindTo(Room.Type);
                roomType.BindValueChanged(t =>
                {
                    endDateInfo.Alpha = t.NewValue == MatchType.Playlists ? 1 : 0;
                }, true);

                hasPassword.BindTo(Room.HasPassword);
                hasPassword.BindValueChanged(v => passwordIcon.Alpha = v.NewValue ? 1 : 0, true);
            };
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            return new CachedModelDependencyContainer<Room>(base.CreateChildDependencies(parent))
            {
                Model = { Value = Room }
            };
        }

        private int numberOfAvatars = 7;

        public int NumberOfAvatars
        {
            get => numberOfAvatars;
            set
            {
                numberOfAvatars = value;

                if (drawableRoomParticipantsList != null)
                    drawableRoomParticipantsList.NumberOfCircles = value;
            }
        }

        protected virtual Drawable CreateBackground() => new OnlinePlayBackgroundSprite();

        protected virtual IEnumerable<Drawable> CreateBottomDetails()
        {
            var pills = new List<Drawable>();

            if (Room.Type.Value != MatchType.Playlists)
            {
                pills.AddRange(new OnlinePlayComposite[]
                {
                    new MatchTypePill(),
                    new QueueModePill(),
                });
            }

            pills.AddRange(new Drawable[]
            {
                new PlaylistCountPill
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                },
                new StarRatingRangeDisplay
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Scale = new Vector2(0.8f)
                }
            });

            return pills;
        }

        private partial class RoomStatusText : OnlinePlayComposite
        {
            [Resolved]
            private OsuColour colours { get; set; } = null!;

            [Resolved]
            private BeatmapLookupCache beatmapLookupCache { get; set; } = null!;

            private SpriteText statusText = null!;
            private LinkFlowContainer beatmapText = null!;

            public RoomStatusText()
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                InternalChild = new GridContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    ColumnDimensions = new[]
                    {
                        new Dimension(GridSizeMode.AutoSize),
                    },
                    RowDimensions = new[]
                    {
                        new Dimension(GridSizeMode.AutoSize)
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            statusText = new OsuSpriteText
                            {
                                Font = OsuFont.Default.With(size: 16),
                                Colour = colours.Lime1
                            },
                            beatmapText = new LinkFlowContainer(s =>
                            {
                                s.Font = OsuFont.Default.With(size: 16);
                                s.Colour = colours.Lime1;
                            })
                            {
                                RelativeSizeAxes = Axes.X,
                                // workaround to ensure only the first line of text shows, emulating truncation (but without ellipsis at the end).
                                // TODO: remove when text/link flow can support truncation with ellipsis natively.
                                Height = 16,
                                Masking = true
                            }
                        }
                    }
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                CurrentPlaylistItem.BindValueChanged(onSelectedItemChanged, true);
            }

            private CancellationTokenSource? beatmapLookupCancellation;

            private void onSelectedItemChanged(ValueChangedEvent<PlaylistItem> item)
            {
                beatmapLookupCancellation?.Cancel();
                beatmapText.Clear();

                if (Type.Value == MatchType.Playlists)
                {
                    statusText.Text = "Ready to play";
                    return;
                }

                var beatmap = item.NewValue?.Beatmap;
                if (beatmap == null)
                    return;

                var cancellationSource = beatmapLookupCancellation = new CancellationTokenSource();
                beatmapLookupCache.GetBeatmapAsync(beatmap.OnlineID, cancellationSource.Token)
                                  .ContinueWith(task => Schedule(() =>
                                  {
                                      if (cancellationSource.IsCancellationRequested)
                                          return;

                                      var retrievedBeatmap = task.GetResultSafely();

                                      statusText.Text = "Currently playing ";

                                      if (retrievedBeatmap != null)
                                      {
                                          beatmapText.AddLink(retrievedBeatmap.GetDisplayTitleRomanisable(),
                                              LinkAction.OpenBeatmap,
                                              retrievedBeatmap.OnlineID.ToString(),
                                              creationParameters: s => s.Truncate = true);
                                      }
                                      else
                                          beatmapText.AddText("unknown beatmap");
                                  }), cancellationSource.Token);
            }
        }

        public partial class PasswordProtectedIcon : CompositeDrawable
        {
            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                Anchor = Anchor.TopRight;
                Origin = Anchor.TopRight;

                Size = new Vector2(32);

                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopCentre,
                        Colour = colours.Gray5,
                        Rotation = 45,
                        RelativeSizeAxes = Axes.Both,
                        Width = 2,
                    },
                    new SpriteIcon
                    {
                        Icon = FontAwesome.Solid.Lock,
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        Margin = new MarginPadding(6),
                        Size = new Vector2(14),
                    }
                };
            }
        }
    }
}
