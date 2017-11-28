// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Configuration;
using osu.Framework.Graphics.Colour;
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
        /// Changes the background color of the notification. Defaults to white.
        /// </summary>
        public ColourInfo BackgroundColour
        {
            get { return BackgroundColourBinding.Value; }
            set { BackgroundColourBinding.Value = value; }
        }

        /// <summary>
        /// The icon with the notification.
        /// </summary>
        public NotificationIcon NotificationIcon
        {
            get { return NotificationIconBinding.Value; }
            set { NotificationIconBinding.Value = value; }
        }

        public Bindable<NotificationIcon> NotificationIconBinding { get; } = new Bindable<NotificationIcon>(new NotificationIcon());
        public Bindable<ColourInfo> BackgroundColourBinding { get; } = new Bindable<ColourInfo>(OsuColour.FromHex("FFF"));
        public Bindable<string> TextBinding { get; } = new Bindable<string>();

        /// <summary>
        /// An event, that gets triggered when the user activates the notification.
        /// </summary>
        public event Action OnActivate;

        /// <summary>
        /// Activates this notification.
        /// </summary>
        public void TriggerActivate()
        {
            OnActivate?.Invoke();
        }

        public Notification(string text = "", FontAwesome icon = FontAwesome.fa_info_circle , Action onActivate = null)
        {
            Text = text;

            NotificationIcon = new NotificationIcon(icon);

            if (onActivate != null)
                OnActivate += onActivate;
        }
    }
}
