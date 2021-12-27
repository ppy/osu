// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Overlays;
using osu.Game.Storyboards.Drawables;

namespace osu.Game.Graphics.Backgrounds
{
    public class BeatmapBackgroundWithStoryboard : BeatmapBackground
    {
        private InterpolatingFramedClock storyboardClock = null!;

        [Resolved(CanBeNull = true)]
        private MusicController? musicController { get; set; }

        public BeatmapBackgroundWithStoryboard(WorkingBeatmap beatmap, string fallbackTextureName = "Backgrounds/bg1")
            : base(beatmap, fallbackTextureName)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (!Beatmap.Storyboard.HasDrawable)
                return;

            if (Beatmap.Storyboard.ReplacesBackground)
                Sprite.Alpha = 0;

            LoadComponentAsync(new AudioContainer
            {
                RelativeSizeAxes = Axes.Both,
                Volume = { Value = 0 },
                Child = new DrawableStoryboard(Beatmap.Storyboard) { Clock = storyboardClock = new InterpolatingFramedClock(Beatmap.Track) }
            }, AddInternal);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            if (musicController != null)
                musicController.TrackChanged += onTrackChanged;
        }

        private void onTrackChanged(WorkingBeatmap newBeatmap, TrackChangeDirection _)
        {
            if (newBeatmap != Beatmap)
                return;

            // `MusicController` will sometimes reload the track, even when the working beatmap technically hasn't changed.
            // ensure that the storyboard's clock is always using the latest track instance.
            storyboardClock.ChangeSource(newBeatmap.Track);
            storyboardClock.ProcessFrame();
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            if (musicController != null)
                musicController.TrackChanged -= onTrackChanged;
        }
    }
}
