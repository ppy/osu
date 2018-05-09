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
        public event Action PlaybackStarted;
        public event Action PlaybackStopped;

        private TrackManager trackManager;
        private BindableDouble muteBindable;

        public Track CurrentTrack { get; private set; }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, FrameworkConfigManager config)
        {
            trackManager = new TrackManager(new OnlineStore());

            muteBindable = new BindableDouble();

            audio.AddItem(trackManager);
            config.BindWith(FrameworkSetting.VolumeMusic, trackManager.Volume);

            PlaybackStarted += () => audio.Track.AddAdjustment(AdjustableProperty.Volume, muteBindable);
            PlaybackStopped += () => audio.Track.RemoveAdjustment(AdjustableProperty.Volume, muteBindable);
        }

        public Track Get(BeatmapSetInfo beatmapSetInfo) => trackManager.Get($"https://b.ppy.sh/preview/{beatmapSetInfo.OnlineBeatmapSetID}.mp3");

        protected override void Update()
        {
            if (CurrentTrack?.HasCompleted ?? false)
                PlaybackStopped?.Invoke();

            base.Update();
        }

        public void Play(Track track)
        {
            Stop();
            CurrentTrack = track;
            track.Restart();
            PlaybackStarted?.Invoke();
        }

        public void Stop()
        {
            if (CurrentTrack?.IsRunning ?? false)
            {
                CurrentTrack?.Stop();
                PlaybackStopped?.Invoke();
            }
        }
    }
}
