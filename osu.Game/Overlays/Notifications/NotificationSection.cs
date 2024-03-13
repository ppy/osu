// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Localisation;
using osuTK;

namespace osu.Game.Overlays.Notifications
{
    public partial class NotificationSection : AlwaysUpdateFillFlowContainer<Drawable>
    {
        /// <summary>
        /// All notifications currently being displayed in this section.
        /// </summary>
        public IEnumerable<Notification> Notifications => notifications;

        private OsuSpriteText countDrawable = null!;

        private FlowContainer<Notification> notifications = null!;

        public int DisplayedCount => notifications.Count(n => !n.WasClosed);
        public int UnreadCount => notifications.Count(n => !n.WasClosed && !n.Read);

        public void Add(Notification notification, float position)
        {
            notifications.Insert((int)position, notification);
        }

        /// <summary>
        /// Enumerable of notification types accepted in this section.
        /// If <see langword="null"/>, the section accepts any and all notifications.
        /// </summary>
        public IEnumerable<Type>? AcceptedNotificationTypes { get; }

        private readonly LocalisableString titleText;

        public NotificationSection(LocalisableString title, IEnumerable<Type>? acceptedNotificationTypes = null)
        {
            AcceptedNotificationTypes = acceptedNotificationTypes?.ToArray();

            titleText = title;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Direction = FillDirection.Vertical;

            Padding = new MarginPadding
            {
                Top = 10,
                Bottom = 5,
                Right = 20,
                Left = 20,
            };

            AddRangeInternal(new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        new ClearAllButton
                        {
                            Text = NotificationsStrings.ClearAll.ToUpper(),
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            Action = clearAll
                        },
                        new FillFlowContainer
                        {
                            Margin = new MarginPadding
                            {
                                Bottom = 5
                            },
                            Spacing = new Vector2(5, 0),
                            AutoSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Text = titleText.ToUpper(),
                                    Font = OsuFont.GetFont(weight: FontWeight.Bold)
                                },
                                countDrawable = new OsuSpriteText
                                {
                                    Text = "3",
                                    Colour = colours.Yellow,
                                    Font = OsuFont.GetFont(weight: FontWeight.Bold)
                                },
                            }
                        },
                    },
                },
                notifications = new AlwaysUpdateFillFlowContainer<Notification>
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    LayoutDuration = 150,
                    LayoutEasing = Easing.OutQuart,
                }
            });
        }

        private void clearAll() => notifications.Children.ForEach(c =>
        {
            if (c is not ProgressNotification p || !p.Ongoing)
                c.Close(true);
        });

        protected override void Update()
        {
            base.Update();

            countDrawable.Text = getVisibleCount().ToString();
        }

        private int getVisibleCount()
        {
            int count = 0;

            foreach (var c in notifications)
            {
                if (c.Alpha > 0.99f)
                    count++;
            }

            return count;
        }

        private partial class ClearAllButton : OsuClickableContainer
        {
            private readonly OsuSpriteText text;

            public ClearAllButton()
            {
                AutoSizeAxes = Axes.Both;

                Children = new[]
                {
                    text = new OsuSpriteText()
                };
            }

            public LocalisableString Text
            {
                get => text.Text;
                set => text.Text = value;
            }
        }

        public void MarkAllRead()
        {
            notifications.Children.ForEach(n => n.Read = true);
        }
    }

    public partial class AlwaysUpdateFillFlowContainer<T> : FillFlowContainer<T>
        where T : Drawable
    {
        // this is required to ensure correct layout and scheduling on children.
        // the layout portion of this is being tracked as a framework issue (https://github.com/ppy/osu-framework/issues/1297).
        protected override bool RequiresChildrenUpdate => true;
    }
}
