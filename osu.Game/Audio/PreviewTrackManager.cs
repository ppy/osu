// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Configuration;
using osu.Framework.IO.Stores;

namespace osu.Game.Audio
{
    public class PreviewTrackManager : TrackManager
    {
        private AudioManager audio;
        private Track currentTrack;
        private readonly BindableDouble muteBindable;

        public PreviewTrackManager()
            : base(new OnlineStore())
        {
            muteBindable = new BindableDouble();
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, FrameworkConfigManager config)
        {
            this.audio = audio;

            audio.AddItem(this);

            config.BindWith(FrameworkSetting.VolumeMusic, Volume);
        }

        protected override void UpdateState()
        {
            if (currentTrack?.HasCompleted ?? false)
                onStop();

            base.UpdateState();
        }

        public void Play(Track track)
        {
            currentTrack?.Stop();
            currentTrack = track;
            currentTrack.Restart();
            onPlay();
        }

        private void onPlay() => audio.Track.AddAdjustment(AdjustableProperty.Volume, muteBindable);

        public void Stop()
        {
            currentTrack?.Stop();
            onStop();
        }

        private void onStop() => audio.Track.RemoveAdjustment(AdjustableProperty.Volume, muteBindable);
    }
}
