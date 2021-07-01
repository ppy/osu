// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Settings.Sections.Graphics
{
    public class DetailSettings : SettingsSubsection
    {
        protected override string Header => "Detail Settings";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = "Storyboard / Video",
                    Current = config.GetBindable<bool>(OsuSetting.ShowStoryboard)
                },
                new SettingsCheckbox
                {
                    LabelText = "Hit Lighting",
                    Current = config.GetBindable<bool>(OsuSetting.HitLighting)
                },
                new SettingsEnumDropdown<ScreenshotFormat>
                {
                    LabelText = "Screenshot format",
                    Current = config.GetBindable<ScreenshotFormat>(OsuSetting.ScreenshotFormat)
                },
                new SettingsCheckbox
                {
                    LabelText = "Show menu cursor in screenshots",
                    Current = config.GetBindable<bool>(OsuSetting.ScreenshotCaptureMenuCursor)
                }
            };
        }
    }
}
