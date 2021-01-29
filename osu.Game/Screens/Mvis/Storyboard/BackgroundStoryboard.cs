using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Skinning;
using osu.Game.Storyboards.Drawables;

namespace osu.Game.Screens.Mvis.Storyboard
{
    [LongRunningLoad]
    public class BackgroundStoryboard : BeatmapSkinProvidingContainer
    {
        public StoryboardClock RunningClock;
        private readonly DrawableStoryboard drawableStoryboard;

        public BackgroundStoryboard(WorkingBeatmap beatmap)
            : base(beatmap.Skin)
        {
            Child = drawableStoryboard = beatmap.Storyboard.CreateDrawable();
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            drawableStoryboard.Clock = RunningClock;
        }

        public Drawable StoryboardProxy() => drawableStoryboard.OverlayLayer.CreateProxy();

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            drawableStoryboard?.Dispose();
        }

        public void Cleanup(float duration)
        {
            this.FadeOut(duration, Easing.OutQuint).Finally(_ => Expire());
        }
    }
}
