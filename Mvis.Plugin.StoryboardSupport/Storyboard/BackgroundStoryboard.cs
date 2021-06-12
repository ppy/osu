using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Skinning;
using osu.Game.Storyboards.Drawables;

namespace Mvis.Plugin.StoryboardSupport.Storyboard
{
    [LongRunningLoad]
    public class BackgroundStoryboard : BeatmapSkinProvidingContainer
    {
        public InterpolatingFramedClock RunningClock;
        private DrawableStoryboard drawableStoryboard;

        private readonly WorkingBeatmap working;
        //private readonly int id;

        public BackgroundStoryboard(WorkingBeatmap beatmap)
            : base(beatmap.Skin)
        {
            //id = beatmap.BeatmapInfo.ID;
            working = beatmap;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            //Logger.Log($"{id} - 创建{working}的故事版", LoggingTarget.Performance);

            drawableStoryboard = working.Storyboard.CreateDrawable();
            drawableStoryboard.Clock = RunningClock;

            //Logger.Log($"{id} - 加载{working}", LoggingTarget.Performance);
            LoadComponent(drawableStoryboard);
            //Logger.Log($"{id} - 添加...", LoggingTarget.Performance);
            Add(drawableStoryboard);

            //Logger.Log($"{id} - 完成", LoggingTarget.Performance);
        }

        public Drawable StoryboardProxy() => drawableStoryboard.OverlayLayer.CreateProxy();

        protected override void Dispose(bool isDisposing)
        {
            //Logger.Log($"{id} - 处理", LoggingTarget.Performance);
            drawableStoryboard?.Dispose();
            base.Dispose(isDisposing);
        }
    }
}
