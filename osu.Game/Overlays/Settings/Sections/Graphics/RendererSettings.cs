// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Settings.Sections.Graphics
{
    public class RendererSettings : SettingsSubsection
    {
        protected override string Header => "渲染";

        private SettingsEnumDropdown<FrameSync> frameLimiterDropdown;

        [BackgroundDependencyLoader]
        private void load(FrameworkConfigManager config, OsuConfigManager osuConfig)
        {
            // NOTE: Compatability mode omitted
            Children = new Drawable[]
            {
                // TODO: this needs to be a custom dropdown at some point
                frameLimiterDropdown = new SettingsEnumDropdown<FrameSync>
                {
                    LabelText = "帧数限制",
                    Current = config.GetBindable<FrameSync>(FrameworkSetting.FrameSync)
                },
                new SettingsEnumDropdown<ExecutionMode>
                {
                    LabelText = "渲染(运行)模式",
                    Current = config.GetBindable<ExecutionMode>(FrameworkSetting.ExecutionMode)
                },
                new SettingsCheckbox
                {
                    LabelText = "显示FPS",
                    Current = osuConfig.GetBindable<bool>(OsuSetting.ShowFpsDisplay)
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            frameLimiterDropdown.Current.BindValueChanged(limit =>
            {
                const string unlimited_frames_note = "使用无限制会导致顿卡, 性能下降和设备过热, 并不会改善感知到的延迟。 我们建议使用\"2倍刷新率\"。";

                frameLimiterDropdown.WarningText = limit.NewValue == FrameSync.Unlimited ? unlimited_frames_note : string.Empty;
            }, true);
        }
    }
}
