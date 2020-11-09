using System;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Skinning;
using osu.Game.Storyboards.Drawables;

namespace osu.Game.Screens.Mvis.Storyboard
{
    public class BackgroundStoryboard : BeatmapSkinProvidingContainer
    {
        private readonly WorkingBeatmap beatmap;
        private bool storyboardLoaded;
        public Storyboards.Storyboard Storyboard;
        private DrawableStoryboard currentLoading;
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        public Action OnStoryboardReadyAction;
        public StoryboardClock RunningClock;

        [Resolved]
        private IBindable<WorkingBeatmap> b { get; set; }

        public BackgroundStoryboard(WorkingBeatmap beatmap, ISkin skin)
            : base(skin)
        {
            this.beatmap = beatmap;
            Storyboard = beatmap.Storyboard;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            currentLoading = Storyboard.CreateDrawable();
            currentLoading.Clock = RunningClock;

            b.BindValueChanged(v =>
            {
                if (v.NewValue != beatmap && !storyboardLoaded)
                {
                    Expire();
                }
            });

            LoadComponentAsync(currentLoading, _ =>
            {
                if (b.Value == beatmap)
                {
                    AddInternal(currentLoading);
                    OnStoryboardReadyAction?.Invoke();

                    storyboardLoaded = true;
                }
            }, cancellationTokenSource.Token);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            cancellationTokenSource.Cancel();
            currentLoading?.Dispose();
        }

        public void Cleanup(float duration)
        {
            this.FadeOut(duration, Easing.OutQuint).Finally(_ => Expire());
        }
    }
}
