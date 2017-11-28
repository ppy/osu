// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Notifications;
using OpenTK;

namespace osu.Game.Overlays.Notifications
{
    public class SimpleDrawableNotification : DrawableNotification
    {
        private readonly Notification notification;

        private readonly TextFlowContainer textDrawable;
        private readonly SpriteIcon iconDrawable;

        private Box iconBackgound;

        public SimpleDrawableNotification(Notification notification)
        {
            if (notification == null)
                throw new ArgumentNullException(nameof(notification));

            this.notification = notification;
            notification.BackgroundColourBinding.ValueChanged += value => Schedule(() => Colour = value);
            notification.TextBinding.ValueChanged += value => Schedule(() => textDrawable.Text = value);
            notification.NotificationIconBinding.ValueChanged += value => Schedule(() =>
            {
                iconDrawable.Icon = value.Icon;
                iconBackgound.Colour = value.BackgroundColour;
            });

            IconContent.AddRange(new Drawable[]
            {
                iconBackgound = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = notification.NotificationIcon.BackgroundColour
                },
                iconDrawable = new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Icon = notification.NotificationIcon.Icon,
                    Size = new Vector2(20),
                }
            });

            Content.Add(textDrawable = new TextFlowContainer(t => t.TextSize = 16)
            {
                Colour = OsuColour.Gray(128),
                AutoSizeAxes = Axes.Y,
                RelativeSizeAxes = Axes.X,
                Text = this.notification.Text
            });

            Activated = () =>
            {
                notification.TriggerActivate();
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
