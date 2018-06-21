// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.IO.Stores;
using osu.Game.Beatmaps;

namespace osu.Game.Audio
{
    public class PreviewTrackManager : Component
    {
        private readonly BindableDouble muteBindable = new BindableDouble();

        private AudioManager audio;
        private TrackManager trackManager;

        private TrackManagerPreviewTrack current;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, FrameworkConfigManager config)
        {
            trackManager = new TrackManager(new OnlineStore());

            this.audio = audio;
            audio.AddItem(trackManager);

            config.BindWith(FrameworkSetting.VolumeMusic, trackManager.Volume);
        }

        public PreviewTrack Get(BeatmapSetInfo beatmapSetInfo)
        {
            var track = new TrackManagerPreviewTrack(beatmapSetInfo, trackManager);

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

        public void Stop(IPreviewTrackOwner source)
        {
            if (current?.Owner != source)
                return;

            current?.Stop();
            current = null;
        }

        private class TrackManagerPreviewTrack : PreviewTrack
        {
            public IPreviewTrackOwner Owner { get; private set; }

            private readonly BeatmapSetInfo beatmapSetInfo;
            private readonly TrackManager trackManager;

            public TrackManagerPreviewTrack(BeatmapSetInfo beatmapSetInfo, TrackManager trackManager)
            {
                this.beatmapSetInfo = beatmapSetInfo;
                this.trackManager = trackManager;
            }

            [BackgroundDependencyLoader]
            private void load(IPreviewTrackOwner owner)
            {
                Owner = owner;
            }

            protected override Track GetTrack() => trackManager.Get($"https://b.ppy.sh/preview/{beatmapSetInfo?.OnlineBeatmapSetID}.mp3");
        }
    }
}
