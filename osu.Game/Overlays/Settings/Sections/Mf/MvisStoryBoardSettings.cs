// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Settings.Sections.General
{
    public class MvisStoryBoardSettings : SettingsSubsection
    {
        protected override string Header => "故事版(不稳定)";

        [BackgroundDependencyLoader]
        private void load(MfConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = "启用故事版/背景视频",
                    Bindable = config.GetBindable<bool>(MfSetting.MvisEnableStoryboard),
                },
                new SettingsCheckbox
                {
                    LabelText = "在无故事版可用时显示背景动画",
                    Bindable = config.GetBindable<bool>(MfSetting.MvisEnableBgTriangles),
                },
                new SettingsCheckbox
                {
                    LabelText = "允许故事版的Overlay层显示在Mvis面板上方",
                    TooltipText = "这会将故事版的Overlay层放置在Mvis面板容器中，随面板的隐藏而隐藏，且不受背景暗化的影响",
                    Bindable = config.GetBindable<bool>(MfSetting.MvisEnableSBOverlayProxy),
                }
            };
        }
    }
}
