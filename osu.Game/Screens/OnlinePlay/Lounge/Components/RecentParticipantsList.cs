// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Users;
using osu.Game.Users.Drawables;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Lounge.Components
{
    public class RecentParticipantsList : OnlinePlayComposite
    {
        private const float avatar_size = 36;

        private bool canDisplayMoreUsers = true;

        private FillFlowContainer<CircularAvatar> avatarFlow;
        private HiddenUserCount hiddenUsers;

        public RecentParticipantsList()
        {
            AutoSizeAxes = Axes.X;
            Height = 60;
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
                    Shear = new Vector2(0.2f, 0),
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colours.Background4,
                    }
                },
                new FillFlowContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(4),
                    Padding = new MarginPadding { Right = 16 },
                    Children = new Drawable[]
                    {
                        new SpriteIcon
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Size = new Vector2(16),
                            Margin = new MarginPadding(8),
                            Icon = FontAwesome.Solid.User,
                        },
                        avatarFlow = new FillFlowContainer<CircularAvatar>
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(4)
                        },
                        hiddenUsers = new HiddenUserCount
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            RecentParticipants.BindCollectionChanged(onParticipantsChanged, true);
            ParticipantCount.BindValueChanged(_ => updateHiddenUserCount(), true);
        }

        private int numberOfCircles = 3;

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

                updateHiddenUserCount();
            }
        }

        private void onParticipantsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var added in e.NewItems.OfType<User>())
                        addUser(added);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (var removed in e.OldItems.OfType<User>())
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

            updateHiddenUserCount();
        }

        private void addUser(User user)
        {
            if (!canDisplayMoreUsers)
                return;

            if (avatarFlow.Count < NumberOfCircles)
                avatarFlow.Add(new CircularAvatar { User = user });
            else if (avatarFlow.Count == NumberOfCircles)
                avatarFlow.Remove(avatarFlow.Last());
        }

        private void removeUser(User user)
        {
            var nextUser = RecentParticipants.FirstOrDefault(u => avatarFlow.All(a => a.User != u));
            if (nextUser == null)
                return;

            if (canDisplayMoreUsers || NumberOfCircles == RecentParticipants.Count)
                avatarFlow.Add(new CircularAvatar { User = nextUser });
        }

        private void clearUsers()
        {
            canDisplayMoreUsers = true;
            avatarFlow.Clear();
            updateHiddenUserCount();
        }

        private void updateHiddenUserCount()
        {
            int count = ParticipantCount.Value - avatarFlow.Count;

            Debug.Assert(count != 1);

            hiddenUsers.Count = count;
        }

        private class CircularAvatar : CompositeDrawable
        {
            public User User
            {
                get => avatar.User;
                set => avatar.User = value;
            }

            private readonly UpdateableAvatar avatar;

            public CircularAvatar()
            {
                Size = new Vector2(avatar_size);

                InternalChild = new CircularContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Child = avatar = new UpdateableAvatar(showUsernameTooltip: true) { RelativeSizeAxes = Axes.Both }
                };
            }
        }

        public class HiddenUserCount : CompositeDrawable
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
