// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Users.Drawables;
using osuTK;
using Container = osu.Framework.Graphics.Containers.Container;

namespace osu.Game.Screens.OnlinePlay.Lounge.Components
{
    public partial class DrawableRoomParticipantsList : CompositeDrawable
    {
        private const float avatar_size = 30;
        private const float height = 40f;

        private readonly Room room;

        private FillFlowContainer<CircularAvatar> avatarFlow = null!;
        private CircularAvatar hostAvatar = null!;
        private LinkFlowContainer hostText = null!;
        private HiddenUserCount hiddenUsers = null!;
        private OsuSpriteText totalCount = null!;

        public DrawableRoomParticipantsList(Room room)
        {
            this.room = room;

            AutoSizeAxes = Axes.X;
            Height = height;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colours)
        {
            InternalChildren = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    CornerRadius = 10,
                    Shear = OsuGame.SHEAR,
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colours.Background4,
                    }
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Y,
                    AutoSizeAxes = Axes.X,
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.Y,
                            AutoSizeAxes = Axes.X,
                            Spacing = new Vector2(4),
                            Padding = new MarginPadding
                            {
                                Left = 4,
                                Right = 16
                            },
                            Children = new Drawable[]
                            {
                                hostAvatar = new CircularAvatar
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                },
                                hostText = new LinkFlowContainer(s => s.Font = OsuFont.Style.Caption2)
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    AutoSizeAxes = Axes.Both
                                }
                            }
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.Y,
                            AutoSizeAxes = Axes.X,
                            Children = new Drawable[]
                            {
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Masking = true,
                                    CornerRadius = 10,
                                    Shear = OsuGame.SHEAR,
                                    Child = new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = colours.Background3,
                                    }
                                },
                                new FillFlowContainer
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Horizontal,
                                    Spacing = new Vector2(4),
                                    Padding = new MarginPadding
                                    {
                                        Left = 8,
                                        Right = 16
                                    },
                                    Children = new Drawable[]
                                    {
                                        new SpriteIcon
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            Size = new Vector2(12),
                                            Icon = FontAwesome.Solid.User,
                                        },
                                        totalCount = new OsuSpriteText
                                        {
                                            Font = OsuFont.Style.Caption2.With(weight: FontWeight.Bold),
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                        },
                                        avatarFlow = new FillFlowContainer<CircularAvatar>
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            AutoSizeAxes = Axes.Both,
                                            Direction = FillDirection.Horizontal,
                                            Spacing = new Vector2(4),
                                            Margin = new MarginPadding { Left = 4 },
                                        },
                                        hiddenUsers = new HiddenUserCount
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                        }
                                    }
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

            updateRoomHost();
            updateRoomParticipantCount();
            updateRoomParticipants();
        }

        private int numberOfCircles = 4;

        /// <summary>
        /// The maximum number of circles visible (including the "hidden count" circle in the overflow case).
        /// </summary>
        public int NumberOfCircles
        {
            get => numberOfCircles;
            set
            {
                numberOfCircles = value;

                if (LoadState < LoadState.Loaded)
                    return;

                // Reinitialising the list looks janky, but this is unlikely to be used in a setting where it's visible.
                clearUsers();
                foreach (var u in room.RecentParticipants)
                    addUser(u);

                updateHiddenUsers();
            }
        }

        private void updateRoomParticipants()
        {
            HashSet<APIUser> newUsers = room.RecentParticipants.ToHashSet();

            avatarFlow.RemoveAll(a =>
            {
                // Avatar with no user. Really shouldn't ever be the case but asserting it correctly is difficult.
                if (a.User == null)
                    return false;

                // User was previously and still is a participant. Keep them around but remove them from the new set.
                // This will be useful when we add all remaining users (now just the new participants) to the flow.
                if (newUsers.Contains(a.User))
                {
                    newUsers.Remove(a.User);
                    return false;
                }

                // User is no longer a participant. Remove them from the flow.
                return true;
            }, true);

            // Add all remaining users to the flow.
            foreach (var u in newUsers)
                addUser(u);

            updateHiddenUsers();
        }

        private int displayedCircles => avatarFlow.Count + (hiddenUsers.Count > 0 ? 1 : 0);

        private void addUser(APIUser user)
        {
            if (displayedCircles < NumberOfCircles)
                avatarFlow.Add(new CircularAvatar { User = user });
        }

        private void clearUsers()
        {
            avatarFlow.Clear();
            updateHiddenUsers();
        }

        private void updateHiddenUsers()
        {
            int hiddenCount = 0;
            if (room.RecentParticipants.Count > NumberOfCircles)
                hiddenCount = room.ParticipantCount - NumberOfCircles + 1;

            hiddenUsers.Count = hiddenCount;

            if (displayedCircles > NumberOfCircles)
                avatarFlow.Remove(avatarFlow.Last(), true);
            else if (displayedCircles < NumberOfCircles)
            {
                var nextUser = room.RecentParticipants.FirstOrDefault(u => avatarFlow.All(a => a.User != u));
                if (nextUser != null) addUser(nextUser);
            }
        }

        private void onRoomPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Room.Host):
                    updateRoomHost();
                    break;

                case nameof(Room.ParticipantCount):
                    updateRoomParticipantCount();
                    break;

                case nameof(Room.RecentParticipants):
                    updateRoomParticipants();
                    break;
            }
        }

        private void updateRoomHost()
        {
            hostAvatar.User = room.Host;
            hostText.Clear();

            if (room.Host != null)
            {
                hostText.AddText("hosted by ");
                hostText.AddUserLink(room.Host);
            }
        }

        private void updateRoomParticipantCount()
        {
            updateHiddenUsers();
            totalCount.Text = room.ParticipantCount.ToString();
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            room.PropertyChanged -= onRoomPropertyChanged;
        }

        private partial class CircularAvatar : CompositeDrawable
        {
            public APIUser? User
            {
                get => avatar.User;
                set => avatar.User = value;
            }

            private readonly UpdateableAvatar avatar = new UpdateableAvatar(showUserPanelOnHover: true) { RelativeSizeAxes = Axes.Both };

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colours)
            {
                Size = new Vector2(avatar_size);

                InternalChild = new CircularContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = colours.Background5,
                            RelativeSizeAxes = Axes.Both,
                        },
                        avatar
                    }
                };
            }
        }

        public partial class HiddenUserCount : CompositeDrawable
        {
            public int Count
            {
                get => count;
                set
                {
                    count = value;
                    countText.Text = $"+{count}";

                    if (count > 0)
                        Show();
                    else
                        Hide();
                }
            }

            private int count;

            private readonly SpriteText countText = new OsuSpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Font = OsuFont.Default.With(weight: FontWeight.Bold),
            };

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colours)
            {
                Size = new Vector2(avatar_size);
                Alpha = 0;

                InternalChild = new CircularContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colours.Background5,
                        },
                        countText
                    }
                };
            }
        }
    }
}
