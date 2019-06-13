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
        public new Bindable<DateTimeOffset> Bindable
        {
            get => bindable;
            set
            {
                bindable = value.GetBoundCopy();
                bindable.BindValueChanged(dto =>
                    base.Bindable.Value = dto.NewValue.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"), true);
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
                    // reset textbox content to its last valid state on a parse failure.
                    bindable.TriggerChange();
                }
            };
        }
    }
}
