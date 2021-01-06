// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Settings.Sections.Mf
{
    public class MvisAudioSettings : SettingsSubsection
    {
        protected override string Header => "settings.mvis.audio.header";

        [BackgroundDependencyLoader]
        private void load(MConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsSlider<double>
                {
                    LabelText = "settings.mvis.audio.mvisMusicSpeed",
                    Current = config.GetBindable<double>(MSetting.MvisMusicSpeed),
                    KeyboardStep = 0.01f,
                    DisplayAsPercentage = true,
                    TransferValueOnCommit = true
                },
                new SettingsCheckbox
                {
                    LabelText = "settings.mvis.audio.adjustPitch",
                    Current = config.GetBindable<bool>(MSetting.MvisAdjustMusicWithFreq),
                    TooltipText = "settings.mvis.audio.adjustPitch.tooltip"
                },
                new SettingsCheckbox
                {
                    LabelText = "settings.mvis.audio.nightcoreBeat",
                    Current = config.GetBindable<bool>(MSetting.MvisEnableNightcoreBeat),
                    TooltipText = "settings.mvis.audio.nightcoreBeat.tooltip"
                },
                new SettingsCheckbox
                {
                    LabelText = "settings.mvis.audio.playFromCollection",
                    Current = config.GetBindable<bool>(MSetting.MvisPlayFromCollection)
                }
            };
        }
    }
}
