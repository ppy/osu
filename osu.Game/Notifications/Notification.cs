// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Configuration;
using osu.Game.Graphics;

namespace osu.Game.Notifications
{
    public class Notification
    {
        /// <summary>
        /// The text displayed in the notification.
        /// </summary>
        public string Text
        {
            get { return TextBinding.Value; }
            set { TextBinding.Value = value; }
        }

        /// <summary>
        /// The icon displayed in the notification.
        /// </summary>
        public FontAwesome Icon
        {
            get { return IconBinding.Value; }
            set { IconBinding.Value = value; }
        }

        /// <summary>
        /// Custom colors for the notification. If null default colors will be used.
        /// </summary>
        public NotificationColors CustomColors
        {
            get { return CustomColorsBinding.Value; }
            set { CustomColorsBinding.Value = value; }
        }

        public Bindable<NotificationColors> CustomColorsBinding { get; }
        public Bindable<string> TextBinding { get; }
        public Bindable<FontAwesome> IconBinding { get; }

        /// <summary>
        /// An event, that gets triggered when the user activates the notification.
        /// </summary>
        public event Action OnActivate;

        /// <summary>
        /// Activates this notification.
        /// </summary>
        public void Activate()
        {
            OnActivate?.Invoke();
        }

        public Notification(string text = "", FontAwesome icon = FontAwesome.fa_info_circle, Action onActivate = null)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            TextBinding = new Bindable<string>(text);
            IconBinding = new Bindable<FontAwesome>(icon);
            CustomColorsBinding = new Bindable<NotificationColors>(new NotificationColors());
            if (onActivate != null)
                OnActivate += onActivate;
        }
    }
}
