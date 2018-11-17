// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;

namespace osu.Game.Tournament.Components
{
    public class DateTextBox : SettingsTextBox
    {
        public new Bindable<DateTimeOffset> Bindable
        {
            get { return bindable; }

            set
            {
                bindable = value;
                bindable.BindValueChanged(dto =>
                    base.Bindable.Value = dto.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"), true);
            }
        }

        // hold a reference to the provided bindable so we don't have to in every settings section.
        private Bindable<DateTimeOffset> bindable;

        public DateTextBox()
        {
            base.Bindable = new Bindable<string>();
            ((OsuTextBox)Control).OnCommit = (sender, newText) =>
            {
                try
                {
                    bindable.Value = DateTimeOffset.Parse(sender.Text);
                }
                catch
                {
                    bindable.TriggerChange();
                }
            };
        }
    }
}
