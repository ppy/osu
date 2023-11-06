// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Users.Drawables;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Lounge.Components
{
    public partial class DrawableRoomParticipantsList : OnlinePlayComposite
    {
        public const float SHEAR_WIDTH = 12f;

        private const float avatar_size = 36;

        private const float height = 60f;

        private static readonly Vector2 shear = new Vector2(SHEAR_WIDTH / height, 0);

        private FillFlowContainer<CircularAvatar> avatarFlow;

        private CircularAvatar hostAvatar;
        private LinkFlowContainer hostText;
        private HiddenUserCount hiddenUsers;
        private OsuSpriteText totalCount;

        public DrawableRoomParticipantsList()
        {
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
                    Shear = shear,
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
                            Spacing = new Vector2(8),
                            Padding = new MarginPadding
                            {
                                Left = 8,
                                Right = 16
                            },
                            Children = new Drawable[]
                            {
                                hostAvatar = new CircularAvatar
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                },
                                hostText = new LinkFlowContainer
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
                                    Shear = shear,
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
                                            Size = new Vector2(16),
                                            Icon = FontAwesome.Solid.User,
                                        },
                                        totalCount = new OsuSpriteText
                                        {
                                            Font = OsuFont.Default.With(weight: FontWeight.Bold),
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

            RecentParticipants.BindCollectionChanged(onParticipantsChanged, true);
            ParticipantCount.BindValueChanged(_ =>
            {
                updateHiddenUsers();
                totalCount.Text = ParticipantCount.Value.ToString();
            }, true);

            Host.BindValueChanged(onHostChanged, true);
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
                foreach (var u in RecentParticipants)
                    addUser(u);

                updateHiddenUsers();
            }
        }

        private void onParticipantsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Debug.Assert(e.NewItems != null);

                    foreach (var added in e.NewItems.OfType<APIUser>())
                        addUser(added);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    Debug.Assert(e.OldItems != null);

                    foreach (var removed in e.OldItems.OfType<APIUser>())
                        removeUser(removed);
                    break;

                case NotifyCollectionChangedAction.Reset:
                    clearUsers();
                    break;

                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move:
                    // Easiest is to just reinitialise the whole list. These are unlikely to ever be use cases.
                    clearUsers();
                    foreach (var u in RecentParticipants)
                        addUser(u);
                    break;
            }

            updateHiddenUsers();
        }

        private int displayedCircles => avatarFlow.Count + (hiddenUsers.Count > 0 ? 1 : 0);

        private void addUser(APIUser user)
        {
            if (displayedCircles < NumberOfCircles)
                avatarFlow.Add(new CircularAvatar { User = user });
        }

        private void removeUser(APIUser user)
        {
            avatarFlow.RemoveAll(a => a.User == user, true);
        }

        private void clearUsers()
        {
            avatarFlow.Clear();
            updateHiddenUsers();
        }

        private void updateHiddenUsers()
        {
            int hiddenCount = 0;
            if (RecentParticipants.Count > NumberOfCircles)
                hiddenCount = ParticipantCount.Value - NumberOfCircles + 1;

            hiddenUsers.Count = hiddenCount;

            if (displayedCircles > NumberOfCircles)
                avatarFlow.Remove(avatarFlow.Last(), true);
            else if (displayedCircles < NumberOfCircles)
            {
                var nextUser = RecentParticipants.FirstOrDefault(u => avatarFlow.All(a => a.User != u));
                if (nextUser != null) addUser(nextUser);
            }
        }

        private void onHostChanged(ValueChangedEvent<APIUser> host)
        {
            hostAvatar.User = host.NewValue;
            hostText.Clear();

            if (host.NewValue != null)
            {
                hostText.AddText("hosted by ");
                hostText.AddUserLink(host.NewValue);
            }
        }

        private partial class CircularAvatar : CompositeDrawable
        {
            public APIUser User
            {
                get => avatar.User;
                set => avatar.User = value;
            }

            private readonly UpdateableAvatar avatar = new UpdateableAvatar { RelativeSizeAxes = Axes.Both };

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
