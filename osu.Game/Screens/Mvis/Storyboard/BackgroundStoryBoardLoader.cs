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
    public class BackgroundStoryBoardLoader : Container
    {
        private const float DURATION = 750;
        private BindableBool EnableSB = new BindableBool();
        ///<summary>
        ///用于内部确定故事版是否已加载
        ///</summary>
        private BindableBool SBLoaded = new BindableBool();

        ///<summary>
        ///用于对外提供该BindableBool用于检测故事版功能是否已经准备好了
        ///</summary>
        public readonly BindableBool IsReady = new BindableBool();
        public readonly BindableBool NeedToHideTriangles = new BindableBool();
        public readonly BindableBool storyboardReplacesBackground = new BindableBool();

        /// <summary>
        /// 当准备的故事版加载完毕时要调用的Action
        /// </summary>
        private Action OnComplete;

        private StoryboardClock StoryboardClock = new StoryboardClock();
        private BackgroundStoryboard ClockContainer;
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
            config.BindWith(MfSetting.MvisEnableStoryboard, EnableSB);
        }

        protected override void LoadComplete()
        {
            EnableSB.BindValueChanged(OnEnableSBChanged);
        }

        public void OnEnableSBChanged(ValueChangedEvent<bool> v)
        {
            if (v.NewValue)
            {
                if (!SBLoaded.Value)
                    UpdateStoryBoardAsync(this.OnComplete);
                else
                {
                    storyboardReplacesBackground.Value = b.Value.Storyboard.ReplacesBackground && b.Value.Storyboard.HasDrawable;
                    NeedToHideTriangles.Value = b.Value.Storyboard.HasDrawable;
                }

                ClockContainer?.FadeIn(DURATION, Easing.OutQuint);
            }
            else
            {
                ClockContainer?.FadeOut(DURATION / 2, Easing.OutQuint);
                storyboardReplacesBackground.Value = false;
                NeedToHideTriangles.Value = false;
                IsReady.Value = true;
                CancelAllTasks();
            }
        }

        public bool LoadStoryboardFor(WorkingBeatmap beatmap)
        {
            try
            {
                LoadComponentAsync(nextStoryboard, newClockContainer =>
                {
                    StoryboardClock.ChangeSource(beatmap.Track);
                    Seek(beatmap.Track.CurrentTime);

                    this.Add(newClockContainer);
                    ClockContainer = newClockContainer;
                }, cancellationTokenSource.Token);
            }
            catch (Exception e)
            {
                Logger.Error(e, "加载故事版时出现错误。");
                return false;
            }

            return true;
        }

        public void Seek(double position) =>
            StoryboardClock?.Seek(position);

        public void CancelAllTasks() =>
            cancellationTokenSource?.Cancel();

        public void UpdateStoryBoardAsync(Action OnComplete = null)
        {
            CancelAllTasks();
            IsReady.Value = false;
            SBLoaded.Value = false;
            NeedToHideTriangles.Value = false;
            storyboardReplacesBackground.Value = false;
            nextStoryboard = null;
            this.OnComplete = OnComplete;

            cancellationTokenSource = new CancellationTokenSource();

            StoryboardClock.IsCoupled = false;
            StoryboardClock.Stop();

            foreach (var item in this)
                (item as BackgroundStoryboard).Cleanup(DURATION);

            if (!EnableSB.Value || b == null)
            {
                IsReady.Value = true;
                return;
            }

            Task.Run(async () =>
            {
                nextStoryboard = new BackgroundStoryboard(b.Value.Storyboard, b.Value.Skin)
                {
                    RelativeSizeAxes = Axes.Both,
                    RunningClock = StoryboardClock = new StoryboardClock(),
                    Alpha = 0.1f,
                    onStoryboardReadyAction = () =>
                    {
                        SBLoaded.Value = true;
                        IsReady.Value = true;
                        NeedToHideTriangles.Value = b.Value.Storyboard.HasDrawable;

                        EnableSB.TriggerChange();
                        OnComplete?.Invoke();
                        OnComplete = null;
                    }
                };

                loadTask = Task.Run(() => LoadStoryboardFor(b.Value), cancellationTokenSource.Token);

                await loadTask;
            }, cancellationTokenSource.Token);
        }
    }
}