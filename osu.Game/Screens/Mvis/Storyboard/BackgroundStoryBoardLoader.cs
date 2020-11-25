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

        private StoryboardClock storyboardClock = new StoryboardClock();
        private BackgroundStoryboard currentStoryboard;

        private readonly WorkingBeatmap currentBeatmap;

        [Resolved]
        private MusicController music { get; set; }

        public BackgroundStoryBoardLoader(WorkingBeatmap working)
        {
            RelativeSizeAxes = Axes.Both;
            currentBeatmap = working;
        }

        [BackgroundDependencyLoader]
        private void load(MfConfigManager config)
        {
            config.BindWith(MfSetting.MvisEnableStoryboard, enableSb);
        }

        protected override void LoadComplete()
        {
            enableSb.BindValueChanged(OnEnableSBChanged);
            UpdateStoryBoardAsync();
        }

        private Task loadTask;

        private void prepareStoryboard(WorkingBeatmap beatmap)
        {
            cancelAllTasks();
            sbLoaded.Value = false;
            NeedToHideTriangles.Value = false;
            StoryboardReplacesBackground.Value = false;

            storyboardClock.IsCoupled = false;
            storyboardClock.Stop();

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
                });
        }

        public void OnEnableSBChanged(ValueChangedEvent<bool> v)
        {
            if (v.NewValue)
            {
                if (!sbLoaded.Value)
                    UpdateStoryBoardAsync();
                else
                {
                    StoryboardReplacesBackground.Value = currentBeatmap.Storyboard.ReplacesBackground && currentBeatmap.Storyboard.HasDrawable;
                    NeedToHideTriangles.Value = currentBeatmap.Storyboard.HasDrawable;
                }

                currentStoryboard?.FadeIn(STORYBOARD_FADEIN_DURATION, Easing.OutQuint);
            }
            else
            {
                currentStoryboard?.FadeOut(STORYBOARD_FADEOUT_DURATION, Easing.OutQuint);
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

        public void UpdateStoryBoardAsync()
        {
            if (!enableSb.Value)
            {
                State.Value = StoryboardState.NotLoaded;
                return;
            }

            if (currentBeatmap == null)
                throw new InvalidOperationException("currentBeatmap 不能为 null");

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
        Loading,
        Failed,
        Success
    }
}
