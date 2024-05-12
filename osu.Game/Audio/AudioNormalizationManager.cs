// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Game.Configuration;

namespace osu.Game.Audio
{
    public class AudioNormalizationManager
    {
        private readonly Bindable<bool> beatmapHitsoundBind;

        // drop track volume game-wide to leave some head-room for UI effects / samples.
        public readonly BindableDouble TrackNormalizeVolume = new BindableDouble(0.8);

        public readonly BindableDouble SampleNormalizeVolume = new BindableDouble(1.0);

        public AudioNormalizationManager(AudioManager audioManager, OsuConfigManager config)
        {
            audioManager.Tracks.AddAdjustment(AdjustableProperty.Volume, TrackNormalizeVolume);

            beatmapHitsoundBind = config.GetBindable<bool>(OsuSetting.BeatmapHitsounds);

            normalizeSampleIfTrue(beatmapHitsoundBind.Value);
            beatmapHitsoundBind.BindValueChanged(change => normalizeSampleIfTrue(change.NewValue));
        }

        private void normalizeSampleIfTrue(bool value)
        {
            if (value)
            {
                SampleNormalizeVolume.BindTo(TrackNormalizeVolume);
            }
            else
            {
                SampleNormalizeVolume.UnbindFrom(TrackNormalizeVolume);
                SampleNormalizeVolume.Value = 1.0;
            }
        }
    }
}
