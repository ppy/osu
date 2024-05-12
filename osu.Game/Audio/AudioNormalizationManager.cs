// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Logging;
using osu.Game.Configuration;
using osu.Game.Overlays;

namespace osu.Game.Audio
{
    /// <summary>
    /// Provides bindables for track/sample volume.
    /// </summary>
    public class AudioNormalizationManager
    {
        private readonly Bindable<bool> beatmapHitsoundBind;

        /// <summary>
        /// Default <see cref="ITrackStore"/> will bind to this bindable.
        /// <see cref="MusicController"/> updates this on every track change.
        /// It's set to 0.8 by default to leave some head-room for UI effects / samples.
        /// </summary>
        public readonly BindableDouble TrackNormalizeVolume = new BindableDouble(0.8);

        /// <summary>
        /// Samples assigned to hitobjects needs to bind to this bindable to normalize their volume in line with track.
        /// </summary>
        public readonly BindableDouble SampleNormalizeVolume = new BindableDouble(1.0);

        /// <summary>
        /// Creates a new <see cref="AudioNormalizationManager"/>.
        /// </summary>
        /// <param name="audioManager"></param>
        /// <param name="config"></param>
        public AudioNormalizationManager(AudioManager audioManager, OsuConfigManager config)
        {
            audioManager.Tracks.AddAdjustment(AdjustableProperty.Volume, TrackNormalizeVolume);

            beatmapHitsoundBind = config.GetBindable<bool>(OsuSetting.BeatmapHitsounds);

            normalizeSampleIfTrue(beatmapHitsoundBind.Value);
            beatmapHitsoundBind.BindValueChanged(change => normalizeSampleIfTrue(change.NewValue));

            TrackNormalizeVolume.BindValueChanged(va => Logger.Log($"New track loudness normalization value is {va.NewValue}", level: LogLevel.Debug));
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
