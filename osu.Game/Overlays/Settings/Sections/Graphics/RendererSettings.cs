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
        protected override string Header => "Renderer";

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
                    LabelText = "Frame limiter",
                    Current = config.GetBindable<FrameSync>(FrameworkSetting.FrameSync)
                },
                new SettingsEnumDropdown<ExecutionMode>
                {
                    LabelText = "Threading mode",
                    Current = config.GetBindable<ExecutionMode>(FrameworkSetting.ExecutionMode)
                },
                new SettingsCheckbox
                {
                    LabelText = "Show FPS",
                    Current = osuConfig.GetBindable<bool>(OsuSetting.ShowFpsDisplay)
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            frameLimiterDropdown.Current.BindValueChanged(limit =>
            {
                const string unlimited_frames_note = "Using unlimited frame limiter can lead to stutters, bad performance and overheating. It will not improve perceived latency. \"2x refresh rate\" is recommended.";

                frameLimiterDropdown.WarningText = limit.NewValue == FrameSync.Unlimited ? unlimited_frames_note : string.Empty;
            }, true);
        }
    }
}
