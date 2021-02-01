// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Settings
{
    public class SettingsCheckbox : SettingsItem<bool>
    {
        protected override Drawable CreateControl() => new OsuCheckbox();

        public override string LabelText
        {
            get => ((OsuCheckbox)Control).LabelText;
            set => ((OsuCheckbox)Control).LabelText = value;
        }
    }
}
