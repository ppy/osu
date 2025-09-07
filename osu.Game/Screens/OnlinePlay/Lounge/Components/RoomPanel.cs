// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.Chat;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Screens.OnlinePlay.Components;
using osuTK;
using Container = osu.Framework.Graphics.Containers.Container;

namespace osu.Game.Screens.OnlinePlay.Lounge.Components
{
    public abstract partial class RoomPanel : CompositeDrawable, IHasContextMenu
    {
        protected const float CORNER_RADIUS = 10;
        private const float height = 80;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private OsuGame? game { get; set; }

        [Resolved]
        private BeatmapLookupCache beatmapLookupCache { get; set; } = null!;

        public readonly Room Room;

        protected readonly Bindable<PlaylistItem?> SelectedItem = new Bindable<PlaylistItem?>();
        protected Container ButtonsContainer { get; private set; } = null!;

        protected bool ShowExternalLink { get; init; } = true;

        private DrawableRoomParticipantsList? drawableRoomParticipantsList;
        private RoomSpecialCategoryPill? specialCategoryPill;
        private CornerIcon? passwordIcon;
        private CornerIcon? pinnedIcon;
        private EndDateInfo? endDateInfo;
        private RoomNameLine? roomName;
        private DelayedLoadWrapper wrapper = null!;
        private CancellationTokenSource? beatmapLookupCancellation;

        /// <summary>
        /// A fully-populated representation of the selected item's current beatmap.
        /// </summary>
        private readonly Bindable<IBeatmapInfo?> currentBeatmap = new Bindable<IBeatmapInfo?>();

        protected RoomPanel(Room room)
        {
            Room = room;

            RelativeSizeAxes = Axes.X;
            Height = height;

            Masking = true;
            CornerRadius = CORNER_RADIUS;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider, OsuColour colours)
        {
            ButtonsContainer = new Container
            {
                Anchor = Anchor.CentreRight,
                Origin = Anchor.CentreRight,
                RelativeSizeAxes = Axes.Y,
                AutoSizeAxes = Axes.X
            };

            EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Shadow,
                Colour = colourProvider.Background6.Opacity(0.4f),
                Radius = 4,
            };

            InternalChildren = new Drawable[]
            {
                // This resolves internal 1px gaps due to applying the (parenting) corner radius and masking across multiple filling background sprites.
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background5,
                },
                CreateBackground().With(d =>
                {
                    d.RelativeSizeAxes = Axes.Both;
                    d.Beatmap.BindTarget = currentBeatmap;
                }),
                wrapper = new DelayedLoadWrapper(() =>
                    new Container
                    {
                        Name = @"Room content",
                        RelativeSizeAxes = Axes.Both,
                        // This negative padding resolves 1px gaps between this background and the background above.
                        Padding = new MarginPadding { Left = 10, Vertical = -0.5f },
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
                                    Colour = colourProvider.Background5,
                                    Width = 0.2f,
                                },
                                new Box
                                {
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = ColourInfo.GradientHorizontal(colourProvider.Background5, colourProvider.Background5.Opacity(0.3f)),
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
                                                    Left = 10,
                                                    Right = 10,
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
                                                                    new RoomStatusPill(Room)
                                                                    {
                                                                        Anchor = Anchor.CentreLeft,
                                                                        Origin = Anchor.CentreLeft
                                                                    },
                                                                    specialCategoryPill = new RoomSpecialCategoryPill(Room)
                                                                    {
                                                                        Anchor = Anchor.CentreLeft,
                                                                        Origin = Anchor.CentreLeft
                                                                    },
                                                                    new FreestyleStatusPill(Room)
                                                                    {
                                                                        Anchor = Anchor.CentreLeft,
                                                                        Origin = Anchor.CentreLeft
                                                                    },
                                                                    endDateInfo = new EndDateInfo(Room)
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
                                                                    roomName = new RoomNameLine(),
                                                                    new RoomStatusText(Room)
                                                                    {
                                                                        Beatmap = { BindTarget = currentBeatmap }
                                                                    }
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
                                                    drawableRoomParticipantsList = new DrawableRoomParticipantsList(Room)
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
                                passwordIcon = new CornerIcon
                                {
                                    Alpha = 0,
                                    Background = { Colour = colours.Gray8, },
                                    Icon =
                                    {
                                        Icon = FontAwesome.Solid.Lock,
                                        Colour = colours.Gray3,
                                        Rotation = 45,
                                    },
                                },
                                pinnedIcon = new CornerIcon
                                {
                                    Alpha = 0,
                                    Background = { Colour = colours.Orange2 },
                                    Icon =
                                    {
                                        Icon = FontAwesome.Solid.Thumbtack,
                                        Colour = colours.Gray3,
                                        Rotation = 45,
                                    },
                                }
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

            Room.PropertyChanged += onRoomPropertyChanged;

            wrapper.DelayedLoadComplete += _ =>
            {
                Debug.Assert(specialCategoryPill != null);
                Debug.Assert(endDateInfo != null);
                Debug.Assert(passwordIcon != null);

                wrapper.FadeInFromZero(200);

                updateRoomID();
                updateRoomName();
                updateRoomCategory();
                updateRoomType();
                updateRoomHasPassword();
                updateRoomPinned();
            };

            SelectedItem.BindValueChanged(onSelectedItemChanged, true);
        }

        private void onRoomPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Room.RoomID):
                    updateRoomID();
                    break;

                case nameof(Room.Name):
                    updateRoomName();
                    break;

                case nameof(Room.Category):
                    updateRoomCategory();
                    break;

                case nameof(Room.Type):
                    updateRoomType();
                    break;

                case nameof(Room.HasPassword):
                    updateRoomHasPassword();
                    break;

                case nameof(Room.Pinned):
                    updateRoomPinned();
                    break;
            }
        }

        private void onSelectedItemChanged(ValueChangedEvent<PlaylistItem?> item)
        {
            if (item.NewValue?.Beatmap.OnlineID == item.OldValue?.Beatmap.OnlineID)
                return;

            beatmapLookupCancellation?.Cancel();
            beatmapLookupCancellation?.Dispose();
            beatmapLookupCancellation = null;

            if (item.NewValue?.Beatmap == null)
            {
                currentBeatmap.Value = null;
                return;
            }

            var cancellationSource = beatmapLookupCancellation = new CancellationTokenSource();

            beatmapLookupCache.GetBeatmapAsync(item.NewValue.Beatmap.OnlineID, cancellationSource.Token)
                              .ContinueWith(task => Schedule(() =>
                              {
                                  if (!cancellationSource.IsCancellationRequested)
                                      currentBeatmap.Value = task.GetResultSafely();
                              }), cancellationSource.Token);
        }

        private void updateRoomID()
        {
            if (roomName != null && ShowExternalLink)
                roomName.Link = Room.GetOnlineURL(api);
        }

        private void updateRoomName()
        {
            if (roomName != null)
                roomName.Text = Room.Name;
        }

        private void updateRoomCategory()
        {
            if (Room.Category > RoomCategory.Normal)
                specialCategoryPill?.Show();
            else
                specialCategoryPill?.Hide();
        }

        private void updateRoomType()
        {
            if (endDateInfo != null)
                endDateInfo.Alpha = Room.Type == MatchType.Playlists ? 1 : 0;
        }

        private void updateRoomHasPassword()
        {
            if (passwordIcon != null)
                passwordIcon.Alpha = Room.HasPassword ? 1 : 0;
        }

        private void updateRoomPinned()
        {
            if (pinnedIcon != null)
                pinnedIcon.Alpha = Room.Pinned ? 1 : 0;
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

        public virtual MenuItem[] ContextMenuItems
        {
            get
            {
                var items = new List<MenuItem>();

                string? url = Room.GetOnlineURL(api);

                if (url != null)
                {
                    items.AddRange([
                        new OsuMenuItem("View in browser", MenuItemType.Standard, () => game?.OpenUrlExternally(url)),
                        new OsuMenuItem("Copy link", MenuItemType.Standard, () => game?.CopyToClipboard(url))
                    ]);
                }

                return items.ToArray();
            }
        }

        protected virtual UpdateableBeatmapBackgroundSprite CreateBackground() => new UpdateableBeatmapBackgroundSprite();

        protected virtual IEnumerable<Drawable> CreateBottomDetails()
        {
            var pills = new List<Drawable>();

            if (Room.Type != MatchType.Playlists)
            {
                pills.AddRange(new Drawable[]
                {
                    new MatchTypePill(Room),
                    new QueueModePill(Room),
                });
            }

            pills.AddRange(new Drawable[]
            {
                new PlaylistCountPill(Room)
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                },
                new StarRatingRangeDisplay(Room)
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Scale = new Vector2(0.8f)
                }
            });

            return pills;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            Room.PropertyChanged -= onRoomPropertyChanged;
        }

        private partial class RoomStatusText : CompositeDrawable
        {
            public readonly Bindable<IBeatmapInfo?> Beatmap = new Bindable<IBeatmapInfo?>();

            [Resolved]
            private OsuColour colours { get; set; } = null!;

            private readonly Room room;
            private SpriteText statusText = null!;
            private LinkFlowContainer beatmapText = null!;

            public RoomStatusText(Room room)
            {
                this.room = room;
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
                                Font = OsuFont.Style.Caption2,
                                Colour = colours.Lime1
                            },
                            beatmapText = new LinkFlowContainer(s =>
                            {
                                s.Font = OsuFont.Style.Caption2;
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

                Beatmap.BindValueChanged(onBeatmapChanged, true);
            }

            private void onBeatmapChanged(ValueChangedEvent<IBeatmapInfo?> beatmap)
            {
                beatmapText.Clear();

                if (room.Type == MatchType.Playlists)
                {
                    statusText.Text = "Ready to play";
                    return;
                }

                statusText.Text = "Currently playing ";

                if (beatmap.NewValue != null)
                {
                    beatmapText.AddLink(beatmap.NewValue.GetDisplayTitleRomanisable(),
                        LinkAction.OpenBeatmap,
                        beatmap.NewValue.OnlineID.ToString(),
                        creationParameters: s => s.Truncate = true);
                }
                else
                    beatmapText.AddText("unknown beatmap");
            }
        }

        public partial class CornerIcon : CompositeDrawable
        {
            public SpriteIcon Icon { get; }
            public Box Background { get; }

            public CornerIcon()
            {
                Anchor = Anchor.TopRight;
                Origin = Anchor.TopRight;

                Size = new Vector2(32);

                InternalChildren = new Drawable[]
                {
                    Background = new Box
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopCentre,
                        Rotation = 45,
                        RelativeSizeAxes = Axes.Both,
                        Width = 2,
                    },
                    Icon = new SpriteIcon
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.Centre,
                        Position = new Vector2(-13, 13),
                        Margin = new MarginPadding(6),
                        Size = new Vector2(14),
                    }
                };
            }
        }

        public partial class RoomNameLine : FillFlowContainer
        {
            private readonly TruncatingSpriteText spriteText;
            private readonly ExternalLinkButton linkButton;

            public LocalisableString Text
            {
                get => spriteText.Text;
                set => spriteText.Text = value;
            }

            private string? link;

            public string? Link
            {
                get => link;
                set
                {
                    link = value;
                    updateLink();
                }
            }

            public RoomNameLine()
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                Direction = FillDirection.Horizontal;

                Children = new Drawable[]
                {
                    spriteText = new TruncatingSpriteText
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        Font = OsuFont.Style.Heading2,
                    },
                    linkButton = new ExternalLinkButton
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        Margin = new MarginPadding { Horizontal = 6, Bottom = 4 },
                        Alpha = 0f,
                    },
                };
            }

            private void updateLink()
            {
                if (link == null)
                    linkButton.Hide();
                else
                {
                    linkButton.Show();
                    linkButton.Link = link;
                }
            }

            protected override void Update()
            {
                base.Update();
                spriteText.MaxWidth = DrawWidth - linkButton.LayoutSize.X;
            }
        }
    }
}
