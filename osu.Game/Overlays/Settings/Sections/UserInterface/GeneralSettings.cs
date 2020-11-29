// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Settings.Sections.UserInterface
{
    public class GeneralSettings : SettingsSubsection
    {
        protected override string Header => "General";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = "Rotate cursor when dragging",
                    Current = config.GetBindable<bool>(OsuSetting.CursorRotation)
                },
                new SettingsCheckbox
                {
                    LabelText = "Parallax",
                    Current = config.GetBindable<bool>(OsuSetting.MenuParallax)
                },
            };
        }
    }
}
