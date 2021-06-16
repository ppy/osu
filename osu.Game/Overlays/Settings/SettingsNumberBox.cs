// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;

namespace osu.Game.Overlays.Settings
{
    public class SettingsNumberBox : SettingsItem<string>
    {
        protected override Drawable CreateControl() => new NumberBox
        {
            Margin = new MarginPadding { Top = 5 },
            RelativeSizeAxes = Axes.X,
        };

        public class NumberBox : SettingsTextBox.TextBox
        {
            protected override bool CanAddCharacter(char character) => char.IsNumber(character);
        }
    }
}
