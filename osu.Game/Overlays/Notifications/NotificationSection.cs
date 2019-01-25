﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;
using osu.Game.Graphics.Containers;

namespace osu.Game.Overlays.Notifications
{
    public class NotificationSection : AlwaysUpdateFillFlowContainer<Drawable>
    {
        private OsuSpriteText titleText;
        private OsuSpriteText countText;

        private ClearAllButton clearButton;

        private FlowContainer<Notification> notifications;

        public int DisplayedCount => notifications.Count(n => !n.WasClosed);
        public int UnreadCount => notifications.Count(n => !n.WasClosed && !n.Read);

        public void Add(Notification notification, float position)
        {
            notifications.Add(notification);
            notifications.SetLayoutPosition(notification, position);
        }

        public IEnumerable<Type> AcceptTypes;

        private string clearText;

        public string ClearText
        {
            get { return clearText; }
            set
            {
                clearText = value;
                if (clearButton != null) clearButton.Text = clearText;
            }
        }

        private string title;

        public string Title
        {
            get { return title; }
            set
            {
                title = value;
                if (titleText != null) titleText.Text = title.ToUpperInvariant();
            }
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
                        clearButton = new ClearAllButton
                        {
                            Text = clearText,
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
                                titleText = new OsuSpriteText
                                {
                                    Text = title.ToUpperInvariant(),
                                    Font = @"Exo2.0-Black",
                                },
                                countText = new OsuSpriteText
                                {
                                    Text = "3",
                                    Colour = colours.Yellow,
                                    Font = @"Exo2.0-Black",
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
                    Spacing = new Vector2(3),
                }
            });
        }

        private void clearAll()
        {
            notifications.Children.ForEach(c => c.Close());
        }

        protected override void Update()
        {
            base.Update();

            countText.Text = notifications.Children.Count(c => c.Alpha > 0.99f).ToString();
        }

        private class ClearAllButton : OsuClickableContainer
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

            public string Text
            {
                get { return text.Text; }
                set { text.Text = value.ToUpperInvariant(); }
            }
        }

        public void MarkAllRead()
        {
            notifications?.Children.ForEach(n => n.Read = true);
        }
    }

    public class AlwaysUpdateFillFlowContainer<T> : FillFlowContainer<T>
        where T : Drawable
    {
        // this is required to ensure correct layout and scheduling on children.
        // the layout portion of this is being tracked as a framework issue (https://github.com/ppy/osu-framework/issues/1297).
        protected override bool RequiresChildrenUpdate => true;
    }
}
