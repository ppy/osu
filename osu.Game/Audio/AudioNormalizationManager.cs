// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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
        /// <summary>
        /// Fallback volume for tracks that <see cref="TrackLoudness"/> failed to measure loudness.
        /// </summary>
        public const double FALLBACK_VOLUME = 0.8;

        private readonly Bindable<bool> beatmapHitsoundBind;

        /// <summary>
        /// Default <see cref="ITrackStore"/> will bind to this bindable.
        /// <see cref="MusicController"/> updates this on every track change.
        /// It's set to 0.8 by default to leave some head-room for UI effects / samples.
        /// </summary>
        public readonly BindableDouble TrackNormalizeVolume = new BindableDouble(FALLBACK_VOLUME);

        /// <summary>
        /// Samples assigned to hitobjects needs to bind to this bindable to normalize their volume in line with track.
        /// </summary>
        public readonly BindableDouble SampleNormalizeVolume = new BindableDouble(1.0);

        /// <summary>
        /// Creates a new <see cref="AudioNormalizationManager"/>.
        /// </summary>
        /// <param name="audioManager">An audio manager with tracks that need normalization.</param>
        /// <param name="config">An osu config to get beatmap hitsounds bindable from.</param>
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

        /// <summary>
        /// Notify <see cref="AudioNormalizationManager"/> that normalization is needed, and run a provided action if needed.
        /// </summary>
        /// <param name="audioNormalization">An <see cref="AudioNormalization"/> to get volume offset from.</param>
        /// <param name="action">An action to run if needed.</param>
        public void SetTrackNormalizationVolume(AudioNormalization? audioNormalization, Action<BindableDouble, double> action)
        {
            double? volume = audioNormalization?.IntegratedLoudnessInVolumeOffset;

            if (volume == null)
            {
                volume = FALLBACK_VOLUME;
                Logger.Log($"Normalization status: {Math.Round((double)volume * 100)}% (fallback)");
            }
            else
            {
                Logger.Log($"Normalization status: {Math.Round((double)volume * 100)}%");
            }

            if (volume == TrackNormalizeVolume.Value)
                return;

            action(TrackNormalizeVolume, (double)volume);
        }
    }
}
