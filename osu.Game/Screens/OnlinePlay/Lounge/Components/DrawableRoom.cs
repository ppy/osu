// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
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
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Input.Bindings;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Screens.OnlinePlay.Components;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Lounge.Components
{
    public class DrawableRoom : OsuClickableContainer, IStateful<SelectionState>, IFilterable, IHasContextMenu, IHasPopover, IKeyBindingHandler<GlobalAction>
    {
        public const float SELECTION_BORDER_WIDTH = 4;
        private const float corner_radius = 10;
        private const float transition_duration = 60;
        private const float height = 100;

        public event Action<SelectionState> StateChanged;

        protected override HoverSounds CreateHoverSounds(HoverSampleSet sampleSet) => new HoverSounds();

        private Drawable selectionBox;

        [Resolved(canBeNull: true)]
        private LoungeSubScreen loungeScreen { get; set; }

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        [Resolved(canBeNull: true)]
        private Bindable<Room> selectedRoom { get; set; }

        [Resolved(canBeNull: true)]
        private LoungeSubScreen lounge { get; set; }

        public readonly Room Room;

        private SelectionState state;

        private Sample sampleSelect;
        private Sample sampleJoin;

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

        private int numberOfAvatars = 7;

        public int NumberOfAvatars
        {
            get => numberOfAvatars;
            set
            {
                numberOfAvatars = value;

                if (recentParticipantsList != null)
                    recentParticipantsList.NumberOfCircles = value;
            }
        }

        private readonly Bindable<RoomCategory> roomCategory = new Bindable<RoomCategory>();

        private RecentParticipantsList recentParticipantsList;
        private RoomSpecialCategoryPill specialCategoryPill;

        public bool FilteringActive { get; set; }

        private PasswordProtectedIcon passwordIcon;

        private readonly Bindable<bool> hasPassword = new Bindable<bool>();

        public DrawableRoom(Room room)
        {
            Room = room;

            RelativeSizeAxes = Axes.X;
            Height = height;

            Masking = true;
            CornerRadius = corner_radius + SELECTION_BORDER_WIDTH / 2;
            EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Shadow,
                Colour = Color4.Black.Opacity(40),
                Radius = 5,
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colours, AudioManager audio)
        {
            Children = new Drawable[]
            {
                // This resolves internal 1px gaps due to applying the (parenting) corner radius and masking across multiple filling background sprites.
                new BufferedContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colours.Background5,
                        },
                        new OnlinePlayBackgroundSprite
                        {
                            RelativeSizeAxes = Axes.Both
                        },
                    }
                },
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
                        CornerRadius = corner_radius,
                        Children = new Drawable[]
                        {
                            // This resolves internal 1px gaps due to applying the (parenting) corner radius and masking across multiple filling background sprites.
                            new BufferedContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                Children = new Drawable[]
                                {
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
                                                    Colour = colours.Background5,
                                                },
                                                new Box
                                                {
                                                    RelativeSizeAxes = Axes.Both,
                                                    Colour = ColourInfo.GradientHorizontal(colours.Background5, colours.Background5.Opacity(0.3f))
                                                },
                                            }
                                        }
                                    },
                                },
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
                                                    new EndDateInfo
                                                    {
                                                        Anchor = Anchor.CentreLeft,
                                                        Origin = Anchor.CentreLeft
                                                    },
                                                }
                                            },
                                            new FillFlowContainer
                                            {
                                                AutoSizeAxes = Axes.Both,
                                                Padding = new MarginPadding { Top = 3 },
                                                Direction = FillDirection.Vertical,
                                                Children = new Drawable[]
                                                {
                                                    new RoomNameText(),
                                                    new RoomHostText(),
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
                                                Scale = new Vector2(0.8f)
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
                                        NumberOfCircles = NumberOfAvatars
                                    }
                                }
                            },
                            passwordIcon = new PasswordProtectedIcon { Alpha = 0 }
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

            sampleSelect = audio.Samples.Get($@"UI/{HoverSampleSet.Default.GetDescription()}-select");
            sampleJoin = audio.Samples.Get($@"UI/{HoverSampleSet.Submit.GetDescription()}-select");
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

            roomCategory.BindTo(Room.Category);
            roomCategory.BindValueChanged(c =>
            {
                if (c.NewValue == RoomCategory.Spotlight)
                    specialCategoryPill.Show();
                else
                    specialCategoryPill.Hide();
            }, true);

            hasPassword.BindTo(Room.HasPassword);
            hasPassword.BindValueChanged(v => passwordIcon.Alpha = v.NewValue ? 1 : 0, true);
        }

        public Popover GetPopover() => new PasswordEntryPopover(Room) { JoinRequested = lounge.Join };

        public MenuItem[] ContextMenuItems => new MenuItem[]
        {
            new OsuMenuItem("Create copy", MenuItemType.Standard, () =>
            {
                lounge?.Open(Room.DeepClone());
            })
        };

        public bool OnPressed(GlobalAction action)
        {
            if (selectedRoom.Value != Room)
                return false;

            switch (action)
            {
                case GlobalAction.Select:
                    TriggerClick();
                    return true;
            }

            return false;
        }

        public void OnReleased(GlobalAction action)
        {
        }

        protected override bool ShouldBeConsideredForInput(Drawable child) => state == SelectionState.Selected || child is HoverSounds;

        protected override bool OnClick(ClickEvent e)
        {
            if (Room != selectedRoom.Value)
            {
                sampleSelect?.Play();
                selectedRoom.Value = Room;
                return true;
            }

            if (Room.HasPassword.Value)
            {
                sampleJoin?.Play();
                this.ShowPopover();
                return true;
            }

            sampleJoin?.Play();
            lounge?.Join(Room, null);

            return base.OnClick(e);
        }

        private class RoomNameText : OsuSpriteText
        {
            [Resolved(typeof(Room), nameof(Online.Rooms.Room.Name))]
            private Bindable<string> name { get; set; }

            public RoomNameText()
            {
                Font = OsuFont.GetFont(size: 28);
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
                InternalChild = hostText = new LinkFlowContainer(s => s.Font = OsuFont.GetFont(size: 16))
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

        public class PasswordProtectedIcon : CompositeDrawable
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

        public class PasswordEntryPopover : OsuPopover
        {
            private readonly Room room;

            public Action<Room, string> JoinRequested;

            public PasswordEntryPopover(Room room)
            {
                this.room = room;
            }

            private OsuPasswordTextBox passwordTextbox;

            [BackgroundDependencyLoader]
            private void load()
            {
                Child = new FillFlowContainer
                {
                    Margin = new MarginPadding(10),
                    Spacing = new Vector2(5),
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Children = new Drawable[]
                    {
                        passwordTextbox = new OsuPasswordTextBox
                        {
                            Width = 200,
                        },
                        new TriangleButton
                        {
                            Width = 80,
                            Text = "Join Room",
                            Action = () => JoinRequested?.Invoke(room, passwordTextbox.Text)
                        }
                    }
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                Schedule(() => GetContainingInputManager().ChangeFocus(passwordTextbox));
                passwordTextbox.OnCommit += (_, __) => JoinRequested?.Invoke(room, passwordTextbox.Text);
            }
        }
    }
}
