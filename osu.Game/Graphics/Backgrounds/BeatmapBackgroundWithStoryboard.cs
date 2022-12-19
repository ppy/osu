// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Overlays;
using osu.Game.Rulesets.Mods;
using osu.Game.Storyboards.Drawables;

namespace osu.Game.Graphics.Backgrounds
{
    public partial class BeatmapBackgroundWithStoryboard : BeatmapBackground
    {
        private readonly InterpolatingFramedClock storyboardClock;

        [Resolved(CanBeNull = true)]
        private MusicController? musicController { get; set; }

        [Resolved]
        private IBindable<IReadOnlyList<Mod>> mods { get; set; } = null!;

        public BeatmapBackgroundWithStoryboard(WorkingBeatmap beatmap, string fallbackTextureName = "Backgrounds/bg1")
            : base(beatmap, fallbackTextureName)
        {
            storyboardClock = new InterpolatingFramedClock();
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
                Child = new DrawableStoryboard(Beatmap.Storyboard, mods.Value) { Clock = storyboardClock }
            }, AddInternal);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            if (musicController != null)
                musicController.TrackChanged += onTrackChanged;

            updateStoryboardClockSource(Beatmap);
        }

        private void onTrackChanged(WorkingBeatmap newBeatmap, TrackChangeDirection _) => updateStoryboardClockSource(newBeatmap);

        private void updateStoryboardClockSource(WorkingBeatmap newBeatmap)
        {
            if (newBeatmap != Beatmap)
                return;

            // `MusicController` will sometimes reload the track, even when the working beatmap technically hasn't changed.
            // ensure that the storyboard's clock is always using the latest track instance.
            storyboardClock.ChangeSource(newBeatmap.Track);
            // more often than not, the previous source track's time will be in the future relative to the new source track.
            // explicitly process a single frame so that `InterpolatingFramedClock`'s interpolation logic is bypassed
            // and the storyboard clock is correctly rewound to the source track's time exactly.
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
