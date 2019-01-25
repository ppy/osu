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
        private void load(GameConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = "Storyboards",
                    Bindable = config.GetBindable<bool>(GameSetting.ShowStoryboard)
                },
                new SettingsCheckbox
                {
                    LabelText = "Rotate cursor when dragging",
                    Bindable = config.GetBindable<bool>(GameSetting.CursorRotation)
                },
                new SettingsEnumDropdown<ScreenshotFormat>
                {
                    LabelText = "Screenshot format",
                    Bindable = config.GetBindable<ScreenshotFormat>(GameSetting.ScreenshotFormat)
                },
                new SettingsCheckbox
                {
                    LabelText = "Show menu cursor in screenshots",
                    Bindable = config.GetBindable<bool>(GameSetting.ScreenshotCaptureMenuCursor)
                }
            };
        }
    }
}
