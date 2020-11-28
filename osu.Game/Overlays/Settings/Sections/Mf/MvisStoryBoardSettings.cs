// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Settings.Sections.General
{
    public class MvisStoryBoardSettings : SettingsSubsection
    {
        protected override string Header => "故事版";

        [BackgroundDependencyLoader]
        private void load(MfConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = "启用故事版/背景视频",
                    Current = config.GetBindable<bool>(MfSetting.MvisEnableStoryboard),
                }
            };
        }
    }
}
