// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Overlays;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens;
using osu.Game.Storyboards.Drawables;

namespace osu.Game.Graphics.Backgrounds
{
    public partial class BeatmapBackgroundWithStoryboard : BeatmapBackground
    {
        private readonly InterpolatingFramedClock storyboardClock;

        public readonly AudioContainer Storyboard;

        private DrawableStoryboard? drawableStoryboard;
        private CancellationTokenSource? loadCancellationSource = new CancellationTokenSource();

        public Action? StoryboardLoaded { get; set; }

        public readonly BindableBool ShowStoryboard = new BindableBool(true);

        [Resolved(CanBeNull = true)]
        private MusicController? musicController { get; set; }

        [Resolved]
        private IBindable<IReadOnlyList<Mod>> mods { get; set; } = null!;

        public BeatmapBackgroundWithStoryboard(WorkingBeatmap beatmap, string fallbackTextureName = "Backgrounds/bg1")
            : base(beatmap, fallbackTextureName)
        {
            storyboardClock = new InterpolatingFramedClock();

            AddInternal(Storyboard = new AudioContainer
            {
                RelativeSizeAxes = Axes.Both,
                Volume = { Value = 0 },
            });
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            LoadStoryboard(false);
        }

        public void LoadStoryboard(bool async = true)
        {
            Debug.Assert(drawableStoryboard == null);

            if (!Beatmap.Storyboard.HasDrawable)
                return;

            drawableStoryboard = new DrawableStoryboard(Beatmap.Storyboard, mods.Value)
            {
                Clock = storyboardClock
            };

            if (async)
                LoadComponentAsync(drawableStoryboard, finishLoad, (loadCancellationSource = new CancellationTokenSource()).Token);
            else
            {
                LoadComponent(drawableStoryboard);
                finishLoad(drawableStoryboard);
            }

            void finishLoad(DrawableStoryboard s)
            {
                Storyboard.FadeInFromZero(BackgroundScreen.TRANSITION_LENGTH, Easing.OutQuint);
                Storyboard.Add(s);

                StoryboardLoaded?.Invoke();
                updateStoryboardVisibility();
            }
        }

        public void UnloadStoryboard()
        {
            if (drawableStoryboard == null)
                return;

            loadCancellationSource?.Cancel();
            loadCancellationSource = null;

            // clear is intentionally used here for the storyboard to be disposed asynchronously.
            Storyboard.Clear();

            drawableStoryboard = null;
            updateStoryboardVisibility();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            if (musicController != null)
                musicController.TrackChanged += onTrackChanged;

            updateStoryboardClockSource(Beatmap);

            ShowStoryboard.BindValueChanged(_ => updateStoryboardVisibility(), true);
        }

        private void updateStoryboardVisibility()
        {
            bool showStoryboard = drawableStoryboard != null && ShowStoryboard.Value;
            bool showBackground = !showStoryboard || !Beatmap.Storyboard.ReplacesBackground;

            Storyboard.FadeTo(showStoryboard ? 1 : 0, BackgroundScreen.TRANSITION_LENGTH, Easing.OutQuint);
            Sprite.FadeTo(showBackground ? 1 : 0, BackgroundScreen.TRANSITION_LENGTH, Easing.OutQuint);
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
