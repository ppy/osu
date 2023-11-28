// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Globalization;
using osu.Framework.Bindables;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;

namespace osu.Game.Tournament.Components
{
    public partial class DateTextBox : SettingsTextBox
    {
        private readonly BindableWithCurrent<DateTimeOffset> current = new BindableWithCurrent<DateTimeOffset>(DateTimeOffset.Now);

        public new Bindable<DateTimeOffset>? Current
        {
            get => current;
            set => current.Current = value!;
        }

        public DateTextBox()
        {
            base.Current = new Bindable<string>(string.Empty);

            current.BindValueChanged(dto =>
                base.Current.Value = dto.NewValue.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", DateTimeFormatInfo.InvariantInfo), true);

            ((OsuTextBox)Control).OnCommit += (sender, _) =>
            {
                try
                {
                    current.Value = DateTimeOffset.Parse(sender.Text, DateTimeFormatInfo.InvariantInfo);
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
