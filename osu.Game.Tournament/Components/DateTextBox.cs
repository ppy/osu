// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;

namespace osu.Game.Tournament.Components
{
    public class DateTextBox : SettingsTextBox
    {
        public new Bindable<DateTimeOffset> Current
        {
            get => current;
            set
            {
                current = value.GetBoundCopy();
                current.BindValueChanged(dto =>
                    base.Current.Value = dto.NewValue.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"), true);
            }
        }

        // hold a reference to the provided bindable so we don't have to in every settings section.
        private Bindable<DateTimeOffset> current = new Bindable<DateTimeOffset>();

        public DateTextBox()
        {
            base.Current = new Bindable<string>();

            ((OsuTextBox)Control).OnCommit += (sender, newText) =>
            {
                try
                {
                    current.Value = DateTimeOffset.Parse(sender.Text);
                }
                catch
                {
                    // reset textbox content to its last valid state on a parse failure.
                    current.TriggerChange();
                }
            };
        }
    }
}
