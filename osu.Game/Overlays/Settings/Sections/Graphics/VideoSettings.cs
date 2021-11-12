// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Video;
using osu.Framework.Localisation;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Settings.Sections.Graphics
{
    public class VideoSettings : SettingsSubsection
    {
        protected override LocalisableString Header => GraphicsSettingsStrings.VideoHeader;

        private Bindable<HardwareVideoDecoder> hardwareVideoDecoder;
        private SettingsCheckbox hwAccelCheckbox;

        [BackgroundDependencyLoader]
        private void load(FrameworkConfigManager config)
        {
            hardwareVideoDecoder = config.GetBindable<HardwareVideoDecoder>(FrameworkSetting.HardwareVideoDecoder);

            Children = new Drawable[]
            {
                hwAccelCheckbox = new SettingsCheckbox
                {
                    LabelText = GraphicsSettingsStrings.UseHardwareAcceleration,
                },
            };

            hwAccelCheckbox.Current.Default = hardwareVideoDecoder.Default != HardwareVideoDecoder.None;
            hwAccelCheckbox.Current.Value = hardwareVideoDecoder.Value != HardwareVideoDecoder.None;

            hwAccelCheckbox.Current.BindValueChanged(val =>
            {
                hardwareVideoDecoder.Value = val.NewValue ? HardwareVideoDecoder.Any : HardwareVideoDecoder.None;
            });
        }
    }
}
