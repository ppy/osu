// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Notifications;
using OpenTK;

namespace osu.Game.Overlays.Notifications
{
    public class SimpleNotificationContainer : NotificationContainer
    {
        public Notification Notification { get; }

        private readonly TextFlowContainer textDrawable;
        private readonly SpriteIcon iconDrawable;

        protected Box IconBackgound;

        public SimpleNotificationContainer(Notification notification)
        {
            if (notification == null)
                throw new ArgumentNullException(nameof(notification));

            Notification = notification;

            Notification.IconBinding.ValueChanged += value => iconDrawable.Icon = value;
            Notification.TextBinding.ValueChanged += value => textDrawable.Text = value;

            IconContent.AddRange(new Drawable[]
            {
                IconBackgound = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = ColourInfo.GradientVertical(OsuColour.Gray(0.2f), OsuColour.Gray(0.6f))
                },
                iconDrawable = new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Icon = Notification.Icon,
                    Size = new Vector2(20),
                }
            });

            Content.Add(textDrawable = new TextFlowContainer(t => t.TextSize = 16)
            {
                Colour = OsuColour.Gray(128),
                AutoSizeAxes = Axes.Y,
                RelativeSizeAxes = Axes.X,
                Text = Notification.Text
            });

            Activated = () =>
            {
                Notification.Activate();
                return true;
            };
        }


        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Light.Colour = colours.Green;
        }

        public override bool Read
        {
            get
            {
                return base.Read;
            }

            set
            {
                base.Read = value;
                Light.FadeTo(value ? 1 : 0, 100);
            }
        }
    }
}
