// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.IO.Stores;
using osu.Framework.Threading;
using osu.Game.Overlays;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Components
{
    public partial class BackgroundMusicManager : Component
    {
        private const int unhover_debounce_duration = 500;
        private const int hover_fade_duration = 250;
        private const int track_fade_duration = 3000;

        private ScheduledDelegate? unduckDebounceDelegate;
        private ScheduledDelegate? globalTrackFadeDelegate;

        private readonly BindableDouble bgmVolumeBindable = new BindableDouble(1);

        private Track bgmTrack = null!;

        [Resolved]
        private MusicController musicController { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, OsuGameBase game)
        {
            // workaround to play BGM through `TrackMixer` instead of `SampleMixer`, so it inherits players' music volume settings, etc.
            var store = audio.GetTrackStore(new NamespacedResourceStore<byte[]>(game.Resources, @"Samples"));
            bgmTrack = store.Get(@"Multiplayer/Matchmaking/Ranked/rankedplay_bgm");
            bgmTrack.AddAdjustment(AdjustableProperty.Volume, bgmVolumeBindable);
        }

        public void Duck()
        {
            unduckDebounceDelegate?.Cancel();
            this.TransformBindableTo(bgmVolumeBindable, 0, hover_fade_duration);
        }

        public void Unduck()
        {
            unduckDebounceDelegate?.Cancel();
            unduckDebounceDelegate = Scheduler.AddDelayed(() =>
            {
                this.TransformBindableTo(bgmVolumeBindable, 1, hover_fade_duration);
            }, unhover_debounce_duration);
        }

        public void Play()
        {
            if (bgmTrack.IsRunning)
                return;

            // remove music control from player, to prevent overlapping music
            musicController.AllowTrackControl.Value = false;
            globalTrackFadeDelegate?.Cancel();

            // cross-fade if global track is playing something
            if (musicController.IsPlaying)
            {
                var track = musicController.CurrentTrack;
                track.VolumeTo(0, track_fade_duration, Easing.OutCubic);
                globalTrackFadeDelegate = Scheduler.AddDelayed(() =>
                {
                    musicController.Stop();
                    track.VolumeTo(1);
                }, track_fade_duration);
            }

            bgmVolumeBindable.Value = 0;
            this.TransformBindableTo(bgmVolumeBindable, 1, track_fade_duration, Easing.InCubic);

            bgmTrack.Looping = true;
            bgmTrack.Start();
        }

        public void Stop()
        {
            unduckDebounceDelegate?.Cancel();
            globalTrackFadeDelegate?.Cancel();

            bgmTrack.Stop();
            bgmTrack.Reset();

            // return control of music to player and reset volume
            musicController.AllowTrackControl.Value = true;
            musicController.CurrentTrack.Volume.Value = 1;
            musicController.EnsurePlayingSomething();
        }

        protected override void Dispose(bool isDisposing)
        {
            Stop();

            base.Dispose(isDisposing);
        }
    }
}
