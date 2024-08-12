// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using osu.Game.Configuration;

namespace osu.Game.Audio
{
    /// <summary>
    /// Manages audio normalization.
    /// </summary>
    public class AudioNormalizationManager
    {
        /// <summary>
        /// Fallback volume for tracks that <see cref="TrackLoudness"/> failed to measure loudness.
        /// </summary>
        public const double FALLBACK_VOLUME = 0.8;

        private readonly Bindable<bool> audioNormalizationSetting;

        /// <summary>
        /// Samples assigned to hitobjects needs to bind to this bindable to normalize their volume in line with track.
        /// </summary>
        public readonly BindableDouble SampleNormalizeVolume = new BindableDouble(1.0);

        /// <summary>
        /// Creates a new <see cref="AudioNormalizationManager"/>.
        /// </summary>
        /// <param name="config">An osu config to get beatmap hitsounds bindable from.</param>
        /// <param name="beatmap">A bindable for beatmap.</param>
        public AudioNormalizationManager(OsuConfigManager config, IBindable<WorkingBeatmap> beatmap)
        {
            audioNormalizationSetting = config.GetBindable<bool>(OsuSetting.AudioNormalization);

            updateNormalization(audioNormalizationSetting.Value, beatmap.Value);
            audioNormalizationSetting.BindValueChanged(change => updateNormalization(change.NewValue, beatmap.Value));
            beatmap.BindValueChanged(ev => onBeatmapChanged(ev.OldValue, ev.NewValue));
        }

        private void updateNormalization(bool value, WorkingBeatmap current)
        {
            if (value)
            {
                SampleNormalizeVolume.BindTo(current.TrackNormalizeVolume);
                current.EnableTrackNormlization();
                Logger.Log($"Normalization value: {(int)(current.TrackNormalizeVolume.Value * 100)}%");
            }
            else
            {
                SampleNormalizeVolume.UnbindFrom(current.TrackNormalizeVolume);
                SampleNormalizeVolume.Value = 1.0;
                current.DisableTrackNormalization();
            }
        }

        private void onBeatmapChanged(WorkingBeatmap oldBeatmap, WorkingBeatmap newBeatmap)
        {
            SampleNormalizeVolume.UnbindFrom(oldBeatmap.TrackNormalizeVolume);
            updateNormalization(audioNormalizationSetting.Value, newBeatmap);
        }
    }
}
