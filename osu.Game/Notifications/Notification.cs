// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Configuration;
using osu.Game.Graphics;

namespace osu.Game.Notifications
{
    public class Notification
    {
        public string Text
        {
            get { return TextBinding.Value; }
            set { TextBinding.Value = value; }
        }

        public FontAwesome Icon
        {
            get { return IconBinding.Value; }
            set { IconBinding.Value = value; }
        }

        public Bindable<string> TextBinding { get; }
        public Bindable<FontAwesome> IconBinding { get; }
        public event Action OnActivate;

        public void Activate()
        {
            OnActivate?.Invoke();
        }

        public Notification(string text = "", FontAwesome icon = FontAwesome.fa_info_circle)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            TextBinding = new Bindable<string>(text);
            IconBinding = new Bindable<FontAwesome>(icon);
        }
    }
}
