// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Settings
{
    public class SettingsCheckbox : SettingsItem<bool>
    {
        private OsuCheckbox checkbox;

        private string labelText;

        protected override Drawable CreateControl() => checkbox = new OsuCheckbox();

        public override string LabelText
        {
            get => labelText;
            set => checkbox.LabelText = labelText = value;
        }
    }
}
