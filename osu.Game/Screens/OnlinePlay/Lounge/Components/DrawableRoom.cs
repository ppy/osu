// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Components;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Lounge.Components
{
    public class DrawableRoom : OsuClickableContainer, IStateful<SelectionState>, IFilterable, IHasContextMenu
    {
        public const float SELECTION_BORDER_WIDTH = 4;
        private const float corner_radius = 10;
        private const float transition_duration = 60;
        private const float height = 100;

        public event Action<SelectionState> StateChanged;

        private Drawable selectionBox;

        [Resolved(canBeNull: true)]
        private OnlinePlayScreen parentScreen { get; set; }

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        public readonly Room Room;

        private SelectionState state;

        public SelectionState State
        {
            get => state;
            set
            {
                if (value == state)
                    return;

                state = value;

                if (selectionBox != null)
                {
                    if (state == SelectionState.Selected)
                        selectionBox.FadeIn(transition_duration);
                    else
                        selectionBox.FadeOut(transition_duration);
                }

                StateChanged?.Invoke(State);
            }
        }

        public IEnumerable<string> FilterTerms => new[] { Room.Name.Value };

        private bool matchingFilter;

        public bool MatchingFilter
        {
            get => matchingFilter;
            set
            {
                matchingFilter = value;

                if (!IsLoaded)
                    return;

                if (matchingFilter)
                    this.FadeIn(200);
                else
                    Hide();
            }
        }

        private int numberOfAvatars = 3;

        public int NumberOfAvatars
        {
            get => numberOfAvatars;
            set
            {
                numberOfAvatars = value;

                if (recentParticipantsList != null)
                    recentParticipantsList.NumberOfAvatars = value;
            }
        }

        private RecentParticipantsList recentParticipantsList;

        public bool FilteringActive { get; set; }

        public DrawableRoom(Room room)
        {
            Room = room;

            RelativeSizeAxes = Axes.X;
            Height = height;
            CornerRadius = corner_radius + SELECTION_BORDER_WIDTH / 2;
            Masking = true;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Children = new Drawable[]
            {
                new Container
                {
                    Name = @"Room content",
                    RelativeSizeAxes = Axes.Both,
                    Child = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        CornerRadius = corner_radius,
                        EdgeEffect = new EdgeEffectParameters
                        {
                            Type = EdgeEffectType.Shadow,
                            Colour = Color4.Black.Opacity(40),
                            Radius = 5,
                        },
                        Children = new Drawable[]
                        {
                            // This resolves 1px gaps due to applying the (parenting) corner radius and masking across multiple filling background sprites.
                            new BufferedContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = Color4Extensions.FromHex(@"#27302E"),
                                    },
                                    new Container
                                    {
                                        Anchor = Anchor.CentreRight,
                                        Origin = Anchor.CentreRight,
                                        RelativeSizeAxes = Axes.Both,
                                        FillMode = FillMode.Fill,
                                        Child = new OnlinePlayBackgroundSprite(BeatmapSetCoverType.List) { RelativeSizeAxes = Axes.Both }
                                    },
                                    new GridContainer
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        ColumnDimensions = new[]
                                        {
                                            new Dimension(GridSizeMode.Relative, 0.2f)
                                        },
                                        Content = new[]
                                        {
                                            new Drawable[]
                                            {
                                                new Box
                                                {
                                                    RelativeSizeAxes = Axes.Both,
                                                    Colour = Color4Extensions.FromHex(@"#27302E"),
                                                },
                                                new Box
                                                {
                                                    RelativeSizeAxes = Axes.Both,
                                                    Colour = ColourInfo.GradientHorizontal(Color4Extensions.FromHex(@"#27302E"), Color4Extensions.FromHex(@"#27302E").Opacity(0.3f))
                                                },
                                            }
                                        }
                                    },
                                }
                            },
                            new Container
                            {
                                Name = @"Left details",
                                RelativeSizeAxes = Axes.Both,
                                Padding = new MarginPadding
                                {
                                    Left = 20,
                                    Vertical = 5
                                },
                                Children = new Drawable[]
                                {
                                    new FillFlowContainer
                                    {
                                        AutoSizeAxes = Axes.Both,
                                        Direction = FillDirection.Horizontal,
                                        Spacing = new Vector2(4),
                                        Children = new Drawable[]
                                        {
                                            new RoomStatusPill
                                            {
                                                Anchor = Anchor.CentreLeft,
                                                Origin = Anchor.CentreLeft
                                            },
                                            new EndDateInfo
                                            {
                                                Anchor = Anchor.CentreLeft,
                                                Origin = Anchor.CentreLeft
                                            },
                                        }
                                    },
                                    new FillFlowContainer
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        AutoSizeAxes = Axes.Both,
                                        Direction = FillDirection.Vertical,
                                        Children = new Drawable[]
                                        {
                                            new RoomNameText(),
                                            new RoomHostText()
                                        }
                                    },
                                    new FillFlowContainer
                                    {
                                        Anchor = Anchor.BottomLeft,
                                        Origin = Anchor.BottomLeft,
                                        AutoSizeAxes = Axes.Both,
                                        Direction = FillDirection.Horizontal,
                                        Spacing = new Vector2(4),
                                        Children = new Drawable[]
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
                                                Scale = new Vector2(0.85f)
                                            }
                                        }
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
                                Padding = new MarginPadding
                                {
                                    Right = 10,
                                    Vertical = 5
                                },
                                Children = new Drawable[]
                                {
                                    recentParticipantsList = new RecentParticipantsList
                                    {
                                        Anchor = Anchor.CentreRight,
                                        Origin = Anchor.CentreRight,
                                        NumberOfAvatars = NumberOfAvatars
                                    }
                                }
                            }
                        },
                    },
                },
                new StatusColouredContainer(transition_duration)
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = selectionBox = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = state == SelectionState.Selected ? 1 : 0,
                        Masking = true,
                        CornerRadius = corner_radius,
                        BorderThickness = SELECTION_BORDER_WIDTH,
                        BorderColour = Color4.White,
                        Child = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0,
                            AlwaysPresent = true
                        }
                    }
                },
            };
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            return new CachedModelDependencyContainer<Room>(base.CreateChildDependencies(parent))
            {
                Model = { Value = Room }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (matchingFilter)
                this.FadeInFromZero(transition_duration);
            else
                Alpha = 0;
        }

        protected override bool ShouldBeConsideredForInput(Drawable child) => state == SelectionState.Selected;

        private class RoomNameText : OsuSpriteText
        {
            [Resolved(typeof(Room), nameof(Online.Rooms.Room.Name))]
            private Bindable<string> name { get; set; }

            public RoomNameText()
            {
                Font = OsuFont.GetFont(size: 24);
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                Current = name;
            }
        }

        private class RoomHostText : OnlinePlayComposite
        {
            private LinkFlowContainer hostText;

            public RoomHostText()
            {
                AutoSizeAxes = Axes.Both;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                InternalChild = hostText = new LinkFlowContainer(s => s.Font = OsuFont.GetFont(size: 14))
                {
                    AutoSizeAxes = Axes.Both
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                Host.BindValueChanged(host =>
                {
                    hostText.Clear();

                    if (host.NewValue != null)
                    {
                        hostText.AddText("hosted by ");
                        hostText.AddUserLink(host.NewValue);
                    }
                }, true);
            }
        }

        public MenuItem[] ContextMenuItems => new MenuItem[]
        {
            new OsuMenuItem("Create copy", MenuItemType.Standard, () =>
            {
                parentScreen?.OpenNewRoom(Room.CreateCopy());
            })
        };
    }
}
