using System;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Overlays;

namespace osu.Game.Screens.Mvis.Storyboard
{
    ///<summary>
    ///bug:
    ///快速切换时会有Storyboard Container不消失导致一直在那积累
    ///故事版会莫名奇妙地报个引用空对象错误
    ///故事版获取Overlay Proxy时会报错(???)
    ///</summary>
    public class BackgroundStoryBoardLoader : Container<BackgroundStoryboard>
    {
        private const float duration = 750;
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

        /// <summary>
        /// 当准备的故事版加载完毕时要调用的Action
        /// </summary>
        private Action onComplete;

        private StoryboardClock storyboardClock = new StoryboardClock();
        private BackgroundStoryboard currentStoryboard;

        private WorkingBeatmap currentBeatmap;

        [Resolved]
        private MusicController music { get; set; }

        public BackgroundStoryBoardLoader()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(MfConfigManager config)
        {
            config.BindWith(MfSetting.MvisEnableStoryboard, enableSb);
        }

        protected override void LoadComplete()
        {
            enableSb.BindValueChanged(OnEnableSBChanged);
        }

        private Task loadTask;

        private void prepareStoryboard(WorkingBeatmap beatmap)
        {
            State.Value = StoryboardState.Loading;

            storyboardClock = new StoryboardClock();
            storyboardClock.ChangeSource(beatmap.Track);
            Seek(beatmap.Track.CurrentTime);

            loadTask = LoadComponentAsync(
                currentStoryboard = new BackgroundStoryboard(beatmap)
                {
                    RunningClock = storyboardClock,
                    Alpha = 0.1f
                },
                onLoaded: newStoryboard =>
                {
                    sbLoaded.Value = true;
                    State.Value = StoryboardState.Success;
                    NeedToHideTriangles.Value = beatmap.Storyboard.HasDrawable;

                    Add(newStoryboard);

                    enableSb.TriggerChange();
                    onComplete?.Invoke();
                    onComplete = null;
                });
        }

        public void OnEnableSBChanged(ValueChangedEvent<bool> v)
        {
            if (v.NewValue)
            {
                if (!sbLoaded.Value)
                    UpdateStoryBoardAsync(currentBeatmap);
                else
                {
                    if (currentBeatmap != null)
                    {
                        StoryboardReplacesBackground.Value = currentBeatmap.Storyboard.ReplacesBackground && currentBeatmap.Storyboard.HasDrawable;
                        NeedToHideTriangles.Value = currentBeatmap.Storyboard.HasDrawable;
                    }
                    else
                    {
                        StoryboardReplacesBackground.Value = NeedToHideTriangles.Value = false;
                    }
                }

                currentStoryboard?.FadeIn(duration, Easing.OutQuint);
            }
            else
            {
                currentStoryboard?.FadeOut(duration / 2, Easing.OutQuint);
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
                loadTask?.ContinueWith(_ => b?.Expire());
            }
        }

        public void UpdateStoryBoardAsync(WorkingBeatmap b)
        {
            cancelAllTasks();
            currentBeatmap = b;
            State.Value = StoryboardState.Waiting;
            sbLoaded.Value = false;
            NeedToHideTriangles.Value = false;
            StoryboardReplacesBackground.Value = false;

            storyboardClock.IsCoupled = false;
            storyboardClock.Stop();

            if (!enableSb.Value)
            {
                State.Value = StoryboardState.NotLoaded;
                return;
            }

            Task.Run(async () =>
            {
                var task = Task.Run(() => prepareStoryboard(currentBeatmap));

                await task;
            });
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
        Waiting,
        Loading,
        Failed,
        Success
    }
}
