// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Audio;
using osu.Framework.Graphics.Containers;
using osu.Framework.Threading;
using osu.Game.Audio;
using osu.Game.Overlays;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Components
{
    public partial class BackgroundMusicManager : CompositeComponent
    {
        private const int hover_fade_duration = 250;

        private ScheduledDelegate? globalTrackFadeDelegate;

        private DrawableTrack bgm = null!;

        private bool shouldBePlaying;

        private Bindable<bool> isPlayingPreview = null!;

        [Resolved]
        private MusicController musicController { get; set; } = null!;

        [Resolved]
        private PreviewTrackManager previewTrackManager { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            AddInternal(bgm = new DrawableTrack(audio.Tracks.Get("rankedplay_bgm.ogg")));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            isPlayingPreview = previewTrackManager.IsPlayingPreview.GetBoundCopy();
            isPlayingPreview.BindValueChanged(playing =>
            {
                bgm.VolumeTo(playing.NewValue ? 0 : 1, hover_fade_duration);
            });
        }

        public void Play() => shouldBePlaying = true;

        public void Stop() => shouldBePlaying = false;

        protected override void Update()
        {
            base.Update();

            updatePlayingState();
        }

        private void updatePlayingState()
        {
            if (!bgm.IsLoaded)
                return;

            if (shouldBePlaying == bgm.IsRunning)
                return;

            if (shouldBePlaying)
            {
                const int track_fade_duration = 3000;

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
            else
            {
                globalTrackFadeDelegate?.Cancel();

                bgm.Stop();
                bgm.Reset();

                // return control of music to player and reset volume
                musicController.AllowTrackControl.Value = true;
                musicController.CurrentTrack.Volume.Value = 1;
                musicController.EnsurePlayingSomething();
            }
        }
    }
}
