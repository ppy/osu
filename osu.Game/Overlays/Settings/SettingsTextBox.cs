// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;

namespace osu.Game.Overlays.Settings
{
    public class SettingsTextBox : SettingsItem<string>
    {
        protected override Drawable CreateControl() => new OutlinedTextBox
        {
            RelativeSizeAxes = Axes.X,
            CommitOnFocusLost = true
        };

        public override Bindable<string> Current
        {
            get => base.Current;
            set
            {
                if (value.Default == null)
                    throw new InvalidOperationException($"Bindable settings of type {nameof(Bindable<string>)} should have a non-null default value.");

                base.Current = value;
            }
        }
    }
}
