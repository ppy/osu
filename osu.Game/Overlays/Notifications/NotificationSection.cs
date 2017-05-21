// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using OpenTK;

namespace osu.Game.Overlays.Notifications
{
    public class NotificationSection : FillFlowContainer
    {
        private OsuSpriteText titleText;
        private OsuSpriteText countText;

        private ClearAllButton clearButton;

        private FlowContainer<Notification> notifications;

        public void Add(Notification notification)
        {
            notifications.Add(notification);
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
                if (titleText != null) titleText.Text = title.ToUpper();
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

            AddInternal(new Drawable[]
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
                                    Text = title.ToUpper(),
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
                notifications = new FillFlowContainer<Notification>
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    LayoutDuration = 150,
                    LayoutEasing = EasingTypes.OutQuart,
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

        private class ClearAllButton : ClickableContainer
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
                set { text.Text = value.ToUpper(); }
            }
        }

        public void MarkAllRead()
        {
            notifications?.Children.ForEach(n => n.Read = true);
        }
    }
}