// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Settings.Sections.Graphics
{
    public class RendererSettings : SettingsSubsection
    {
        protected override string Header => "渲染";

        [BackgroundDependencyLoader]
        private void load(FrameworkConfigManager config, OsuConfigManager osuConfig)
        {
            // NOTE: Compatability mode omitted
            Children = new Drawable[]
            {
                // TODO: this needs to be a custom dropdown at some point
                new SettingsEnumDropdown<FrameSync>
                {
                    LabelText = "帧数限制",
                    Bindable = config.GetBindable<FrameSync>(FrameworkSetting.FrameSync)
                },
                new SettingsCheckbox
                {
                    LabelText = "显示FPS",
                    Bindable = osuConfig.GetBindable<bool>(OsuSetting.ShowFpsDisplay)
                },
            };
        }
    }
}
