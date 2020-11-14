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
        private BackgroundStoryboard clockContainer;
        private Task<bool> loadTask;

        private CancellationTokenSource cancellationTokenSource;
        private BackgroundStoryboard nextStoryboard;

        [Resolved]
        private IBindable<WorkingBeatmap> b { get; set; }

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

        public void OnEnableSBChanged(ValueChangedEvent<bool> v)
        {
            if (v.NewValue)
            {
                if (!sbLoaded.Value)
                    UpdateStoryBoardAsync(onComplete);
                else
                {
                    StoryboardReplacesBackground.Value = b.Value.Storyboard.ReplacesBackground && b.Value.Storyboard.HasDrawable;
                    NeedToHideTriangles.Value = b.Value.Storyboard.HasDrawable;
                }

                clockContainer?.FadeIn(duration, Easing.OutQuint);
            }
            else
            {
                clockContainer?.FadeOut(duration / 2, Easing.OutQuint);
                StoryboardReplacesBackground.Value = false;
                NeedToHideTriangles.Value = false;
                State.Value = StoryboardState.NotLoaded;
                CancelAllTasks();
            }
        }

        public bool LoadStoryboardFor(WorkingBeatmap beatmap)
        {
            try
            {
                LoadComponentAsync(nextStoryboard, newClockContainer =>
                {
                    storyboardClock.ChangeSource(beatmap.Track);
                    Seek(beatmap.Track.CurrentTime);

                    Add(newClockContainer);
                    clockContainer = newClockContainer;
                }, cancellationTokenSource.Token);
            }
            catch (Exception e)
            {
                Logger.Error(e, "加载故事版时出现错误。");
                State.Value = StoryboardState.Failed;
                return false;
            }

            return true;
        }

        public void Seek(double position) =>
            storyboardClock?.Seek(position);

        public void CancelAllTasks() =>
            cancellationTokenSource?.Cancel();

        public void UpdateStoryBoardAsync(Action onComplete = null)
        {
            CancelAllTasks();
            State.Value = StoryboardState.Loading;
            sbLoaded.Value = false;
            NeedToHideTriangles.Value = false;
            StoryboardReplacesBackground.Value = false;
            nextStoryboard = null;
            this.onComplete = onComplete;

            cancellationTokenSource = new CancellationTokenSource();

            storyboardClock.IsCoupled = false;
            storyboardClock.Stop();

            foreach (var item in this)
                item.Cleanup(duration);

            if (!enableSb.Value || b == null)
            {
                State.Value = StoryboardState.NotLoaded;
                return;
            }

            Task.Run(async () =>
            {
                nextStoryboard = new BackgroundStoryboard(b.Value, b.Value.Skin)
                {
                    RelativeSizeAxes = Axes.Both,
                    RunningClock = storyboardClock = new StoryboardClock(),
                    Alpha = 0.1f,
                    OnStoryboardReadyAction = () =>
                    {
                        sbLoaded.Value = true;
                        State.Value = StoryboardState.Success;
                        NeedToHideTriangles.Value = b.Value.Storyboard.HasDrawable;

                        enableSb.TriggerChange();
                        onComplete?.Invoke();
                        onComplete = null;
                    }
                };

                loadTask = Task.Run(() => LoadStoryboardFor(b.Value), cancellationTokenSource.Token);

                await loadTask;
            }, cancellationTokenSource.Token);
        }
    }

    public enum StoryboardState
    {
        NotLoaded,
        Loading,
        Failed,
        Success
    }
}
