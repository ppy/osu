// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
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
        private Action<PreviewTrack> onTrackStart;
        private Action onTrackStop;

        private TrackManager trackManager;
        private BindableDouble muteBindable;

        public PreviewTrack CurrentTrack { get; private set; }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, FrameworkConfigManager config)
        {
            trackManager = new TrackManager(new OnlineStore());

            muteBindable = new BindableDouble();

            audio.AddItem(trackManager);
            config.BindWith(FrameworkSetting.VolumeMusic, trackManager.Volume);

            onTrackStart = track =>
            {
                CurrentTrack?.Stop();
                audio.Track.AddAdjustment(AdjustableProperty.Volume, muteBindable);
                CurrentTrack = track;
            };
            onTrackStop = () =>
            {
                audio.Track.RemoveAdjustment(AdjustableProperty.Volume, muteBindable);
                CurrentTrack = null;
            };
        }

        public PreviewTrack Get(BeatmapSetInfo beatmapSetInfo) =>
            new PreviewTrack(
                trackManager.Get($"https://b.ppy.sh/preview/{beatmapSetInfo?.OnlineBeatmapSetID}.mp3"),
                onTrackStart,
                onTrackStop);

        protected override void Update()
        {
            if (CurrentTrack?.Track.HasCompleted ?? false)
                CurrentTrack.Stop();

            base.Update();
        }
    }
}
