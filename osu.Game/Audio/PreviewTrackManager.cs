// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Configuration;
using osu.Framework.Graphics.Containers;
using osu.Framework.IO.Stores;
using osu.Game.Beatmaps;
using osu.Game.Overlays.Direct;

namespace osu.Game.Audio
{
    /// <summary>
    /// A central store for the retrieval of <see cref="PreviewTrack"/>s.
    /// </summary>
    public class PreviewTrackManager : CompositeDrawable
    {
        private readonly BindableDouble muteBindable = new BindableDouble();

        private AudioManager audio;
        private TrackManager trackManager;

        private TrackManagerPreviewTrack current;

        private readonly List<PlayButtonState> playButtonStates = new List<PlayButtonState>();

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, FrameworkConfigManager config)
        {
            trackManager = new TrackManager(new OnlineStore());

            this.audio = audio;
            audio.AddItem(trackManager);

            config.BindWith(FrameworkSetting.VolumeMusic, trackManager.Volume);
        }

        public PlayButtonState GetPlayButtonState(BeatmapSetInfo beatmapSet)
        {
            PlayButtonState result = playButtonStates.Find(playButtonState => playButtonState.BeatmapSet == beatmapSet);

            if (result == null)
            {
                playButtonStates.Add(result = new PlayButtonState(beatmapSet));

                result.StateChanged += state =>
                {
                    switch (state)
                    {
                        case PlayButtonState.PlaybackState.Playing:
                            AddInternal(result);
                            break;
                        case PlayButtonState.PlaybackState.Stopped:
                            RemoveInternal(result);
                            break;
                    }
                };
            }

            return result;
        }

        /// <summary>
        /// Retrieves a <see cref="PreviewTrack"/> for a <see cref="BeatmapSetInfo"/>.
        /// </summary>
        /// <param name="beatmapSetInfo">The <see cref="BeatmapSetInfo"/> to retrieve the preview track for.</param>
        /// <returns>The playable <see cref="PreviewTrack"/>.</returns>
        public PreviewTrack Get(BeatmapSetInfo beatmapSetInfo)
        {
            var track = CreatePreviewTrack(beatmapSetInfo, trackManager);

            track.Started += () =>
            {
                current?.Stop();
                current = track;
                audio.Track.AddAdjustment(AdjustableProperty.Volume, muteBindable);
            };

            track.Stopped += () =>
            {
                current = null;
                audio.Track.RemoveAdjustment(AdjustableProperty.Volume, muteBindable);
            };

            return track;
        }

        /// <summary>
        /// Creates the <see cref="TrackManagerPreviewTrack"/>.
        /// </summary>
        protected virtual TrackManagerPreviewTrack CreatePreviewTrack(BeatmapSetInfo beatmapSetInfo, TrackManager trackManager) => new TrackManagerPreviewTrack(beatmapSetInfo, trackManager);

        protected class TrackManagerPreviewTrack : PreviewTrack
        {
            private readonly BeatmapSetInfo beatmapSetInfo;
            private readonly TrackManager trackManager;

            public TrackManagerPreviewTrack(BeatmapSetInfo beatmapSetInfo, TrackManager trackManager)
            {
                this.beatmapSetInfo = beatmapSetInfo;
                this.trackManager = trackManager;
            }

            protected override Track GetTrack() => trackManager.Get($"https://b.ppy.sh/preview/{beatmapSetInfo?.OnlineBeatmapSetID}.mp3");
        }
    }
}
