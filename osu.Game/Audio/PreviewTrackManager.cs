// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.IO.Stores;
using osu.Game.Beatmaps;

namespace osu.Game.Audio
{
    public class PreviewTrackManager : Component
    {
        private AudioManager audio;
        private TrackManager trackManager;
        private BindableDouble muteBindable;

        public PreviewTrack CurrentTrack { get; private set; }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, FrameworkConfigManager config)
        {
            trackManager = new TrackManager(new OnlineStore());
            muteBindable = new BindableDouble();

            this.audio = audio;
            audio.AddItem(trackManager);

            config.BindWith(FrameworkSetting.VolumeMusic, trackManager.Volume);
        }

        public PreviewTrack Get(BeatmapSetInfo beatmapSetInfo, OverlayContainer previewOwner)
        {
            var previewTrack = new PreviewTrack(
                trackManager.Get($"https://b.ppy.sh/preview/{beatmapSetInfo?.OnlineBeatmapSetID}.mp3"),
                previewOwner);

            previewTrack.Started += () =>
            {
                CurrentTrack?.Stop();
                CurrentTrack = previewTrack;
                audio.Track.AddAdjustment(AdjustableProperty.Volume, muteBindable);
            };

            previewTrack.Stopped += () =>
            {
                CurrentTrack = null;
                audio.Track.RemoveAdjustment(AdjustableProperty.Volume, muteBindable);
            };

            return previewTrack;
        }

        protected override void Update()
        {
            if (CurrentTrack?.Track.HasCompleted ?? false)
                CurrentTrack.Stop();

            base.Update();
        }
    }
}
