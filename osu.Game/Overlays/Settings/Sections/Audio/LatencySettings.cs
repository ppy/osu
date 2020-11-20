// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Graphics;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Settings.Sections.Audio
{
    public class LatencySettings : SettingsSubsection
    {
        protected override string Header => "Latency";

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsSlider<int>
                {
                    LabelText = "Device update period",
                    Current = audio.DeviceUpdatePeriod,
                    KeyboardStep = 1,
                    TransferValueOnCommit = true
                },
                new SettingsSlider<int>
                {
                    LabelText = "Device buffer size",
                    Current = audio.DeviceBufferSize,
                    KeyboardStep = 1,
                    TransferValueOnCommit = true
                },
                new SettingsSlider<int>
                {
                    LabelText = "Playback buffer size",
                    Current = audio.PlaybackBufferSize,
                    KeyboardStep = 1,
                    TransferValueOnCommit = true
                }
            };
        }
    }
}
