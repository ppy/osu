// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Settings.Sections.Graphics
{
    public class DetailSettings : SettingsSubsection
    {
        protected override LocalisableString Header => "细节设置";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = "故事版 / 背景视频",
                    Current = config.GetBindable<bool>(OsuSetting.ShowStoryboard)
                },
                new SettingsCheckbox
                {
                    LabelText = "击打闪光",
                    Current = config.GetBindable<bool>(OsuSetting.HitLighting)
                },
                new SettingsEnumDropdown<ScreenshotFormat>
                {
                    LabelText = "截图格式",
                    Current = config.GetBindable<ScreenshotFormat>(OsuSetting.ScreenshotFormat)
                },
                new SettingsCheckbox
                {
                    LabelText = "在截图中显示光标",
                    Current = config.GetBindable<bool>(OsuSetting.ScreenshotCaptureMenuCursor)
                }
            };
        }
    }
}
