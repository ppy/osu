using System;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Screens.Play;
using osu.Game.Skinning;
using osu.Game.Storyboards.Drawables;

namespace osu.Game.Screens.Mvis.Storyboard
{
    public class BackgroundStoryboard : BeatmapSkinProvidingContainer
    {
        public Storyboards.Storyboard storyboard;
        private DrawableStoryboard currentLoading;
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        public Action onStoryboardReadyAction;
        public StoryboardClock RunningClock;

        public BackgroundStoryboard(Storyboards.Storyboard sb, ISkin skin)
            : base(skin)
        {
            storyboard = sb;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            currentLoading = storyboard.CreateDrawable();
            currentLoading.Clock = RunningClock;

            LoadComponentAsync(currentLoading, _ =>
            {
                AddInternal(currentLoading);
                onStoryboardReadyAction?.Invoke();
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