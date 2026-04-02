// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Audio;
using osu.Framework.Graphics.Containers;
using osu.Framework.IO.Stores;
using osu.Framework.Threading;
using osu.Game.Overlays;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Components
{
    public partial class BackgroundMusicManager : CompositeComponent
    {
        public const int DELAY_BEFORE_UNDUCK = 500;

        private const int hover_fade_duration = 250;
        private const int track_fade_duration = 3000;

        private ScheduledDelegate? unduckDebounceDelegate;
        private ScheduledDelegate? globalTrackFadeDelegate;

        private ITrackStore store = null!;
        private DrawableTrack bgm = null!;

        [Resolved]
        private MusicController musicController { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, OsuGameBase game)
        {
            // workaround to play BGM through `TrackMixer` instead of `SampleMixer`, so it inherits players' music volume settings, etc.
            store = audio.GetTrackStore(new NamespacedResourceStore<byte[]>(game.Resources, @"Samples"));
            var track = store.Get(@"Multiplayer/Matchmaking/Ranked/rankedplay_bgm.ogg");

            AddInternal(bgm = new DrawableTrack(track));
        }

        public void Duck()
        {
            unduckDebounceDelegate?.Cancel();

            bgm.VolumeTo(0, hover_fade_duration);
        }

        public void Unduck()
        {
            unduckDebounceDelegate?.Cancel();
            unduckDebounceDelegate = Scheduler.AddDelayed(() =>
            {
                bgm.VolumeTo(1, hover_fade_duration);
            }, DELAY_BEFORE_UNDUCK);
        }

        public void Play()
        {
            if (bgm.IsRunning)
                return;

            // remove music control from player, to prevent overlapping music
            musicController.AllowTrackControl.Value = false;
            globalTrackFadeDelegate?.Cancel();

            // cross-fade if global track is playing something
            if (musicController.IsPlaying)
            {
                var globalTrack = musicController.CurrentTrack;

                globalTrack.VolumeTo(0, track_fade_duration, Easing.OutCubic);
                globalTrackFadeDelegate = Scheduler.AddDelayed(() =>
                {
                    musicController.Stop();
                    globalTrack.VolumeTo(1);
                }, track_fade_duration);
            }

            bgm.VolumeTo(0)
               .VolumeTo(1, track_fade_duration, Easing.InCubic);

            bgm.Looping = true;
            bgm.Start();
        }

        public void Stop()
        {
            unduckDebounceDelegate?.Cancel();
            globalTrackFadeDelegate?.Cancel();

            bgm.Stop();
            bgm.Reset();

            // return control of music to player and reset volume
            musicController.AllowTrackControl.Value = true;
            musicController.CurrentTrack.Volume.Value = 1;
            musicController.EnsurePlayingSomething();
        }

        protected override void Dispose(bool isDisposing)
        {
            Stop();

            bgm.Dispose();
            store.Dispose();

            base.Dispose(isDisposing);
        }
    }
}
