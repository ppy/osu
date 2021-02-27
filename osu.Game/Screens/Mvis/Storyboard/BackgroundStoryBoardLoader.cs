using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Overlays;

namespace osu.Game.Screens.Mvis.Storyboard
{
    ///<summary>
    /// 负责故事版的异步加载功能
    ///</summary>
    public class BackgroundStoryBoardLoader : Container<BackgroundStoryboard>
    {
        public const float STORYBOARD_FADEIN_DURATION = 750;
        public const float STORYBOARD_FADEOUT_DURATION = STORYBOARD_FADEIN_DURATION / 2;
        private readonly BindableBool enableSb = new BindableBool();

        ///<summary>
        ///用于内部确定故事版是否已加载
        ///</summary>
        private readonly BindableBool sbLoaded = new BindableBool();

        ///<summary>
        ///用于对外提供该BindableBool用于检测故事版功能是否已经准备好了
        ///</summary>
        public readonly Bindable<StoryboardState> State = new Bindable<StoryboardState>();

        public readonly BindableBool NeedToHideTriangles = new BindableBool();
        public readonly BindableBool StoryboardReplacesBackground = new BindableBool();

        private DecoupleableInterpolatingFramedClock storyboardClock = new DecoupleableInterpolatingFramedClock();
        private BackgroundStoryboard currentStoryboard;

        private readonly WorkingBeatmap targetBeatmap;

        public Action OnNewStoryboardLoaded;

        [Resolved]
        private MusicController music { get; set; }

        [Resolved]
        private Bindable<WorkingBeatmap> currentBeatmap { get; set; }

        public BackgroundStoryBoardLoader(WorkingBeatmap working)
        {
            RelativeSizeAxes = Axes.Both;
            targetBeatmap = working;
        }

        [BackgroundDependencyLoader]
        private void load(MConfigManager config)
        {
            config.BindWith(MSetting.MvisEnableStoryboard, enableSb);
            currentBeatmap.BindValueChanged(v =>
            {
                if (v.NewValue == targetBeatmap)
                    storyboardClock.ChangeSource(v.NewValue.Track, false);
            });
        }

        protected override void LoadComplete()
        {
            enableSb.BindValueChanged(OnEnableSBChanged);
            updateStoryBoardAsync();
        }

        private Task loadTask;
        private CancellationTokenSource cancellationTokenSource;

        private void prepareStoryboard(WorkingBeatmap beatmap)
        {
            cancelAllTasks();
            sbLoaded.Value = false;
            NeedToHideTriangles.Value = false;
            StoryboardReplacesBackground.Value = false;

            storyboardClock.IsCoupled = false;
            storyboardClock.Stop(false);

            storyboardClock = new DecoupleableInterpolatingFramedClock();
            cancellationTokenSource = new CancellationTokenSource();

            loadTask = LoadComponentAsync(
                currentStoryboard = new BackgroundStoryboard(beatmap)
                {
                    RunningClock = storyboardClock,
                    Alpha = 0.1f
                },
                onLoaded: newStoryboard =>
                {
                    //bug: 过早Seek至歌曲时间会导致部分故事版加载过程僵死
                    storyboardClock.ChangeSource(beatmap.Track, false);
                    Seek(beatmap.Track.CurrentTime);

                    sbLoaded.Value = true;
                    State.Value = StoryboardState.Success;
                    NeedToHideTriangles.Value = beatmap.Storyboard.HasDrawable;

                    Add(newStoryboard);

                    setProxy(newStoryboard);

                    enableSb.TriggerChange();
                    OnNewStoryboardLoaded?.Invoke();
                }, cancellation: cancellationTokenSource.Token);
        }

        [CanBeNull]
        public Drawable StoryboardProxy;

        private void setProxy(BackgroundStoryboard storyboard)
        {
            if (storyboard != currentStoryboard) return;

            StoryboardProxy = storyboard.StoryboardProxy();
        }

        public void OnEnableSBChanged(ValueChangedEvent<bool> v)
        {
            if (v.NewValue)
            {
                if (sbLoaded.Value)
                {
                    StoryboardReplacesBackground.Value = targetBeatmap.Storyboard.ReplacesBackground && targetBeatmap.Storyboard.HasDrawable;
                    NeedToHideTriangles.Value = targetBeatmap.Storyboard.HasDrawable;
                    currentStoryboard?.FadeIn(STORYBOARD_FADEIN_DURATION, Easing.OutQuint);
                }
                else
                    updateStoryBoardAsync();
            }
            else
            {
                if (sbLoaded.Value)
                    currentStoryboard?.FadeOut(STORYBOARD_FADEOUT_DURATION, Easing.OutQuint);
                else
                    cancelAllTasks();

                StoryboardReplacesBackground.Value = false;
                NeedToHideTriangles.Value = false;
                State.Value = StoryboardState.NotLoaded;
            }
        }

        public void Seek(double position) =>
            storyboardClock?.Seek(position);

        private void cancelAllTasks()
        {
            if (loadTask != null)
            {
                var b = currentStoryboard;
                cancellationTokenSource.Cancel();
                b?.Dispose();
            }
        }

        private void updateStoryBoardAsync()
        {
            if (!enableSb.Value)
            {
                State.Value = StoryboardState.NotLoaded;
                return;
            }

            if (targetBeatmap == null)
                throw new InvalidOperationException("currentBeatmap 不能为 null");

            State.Value = StoryboardState.Loading;
            prepareStoryboard(targetBeatmap);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (isDisposing)
                cancelAllTasks();
        }
    }

    public enum StoryboardState
    {
        NotLoaded,
        Loading,
        Success
    }
}
