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
    public class BackgroundStoryBoardLoader : Container
    {
        private const float DURATION = 750;
        public Container sbContainer;
        public ClockContainer sbClock;
        private CancellationTokenSource ChangeSB;
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
        /// This will log which beatmap's storyboard we are loading
        /// </summary>
        private Task LogTask;

        /// <summary>
        /// This will invoke LoadSBTask and run asyncly
        /// </summary>
        private Task LoadSBAsyncTask;

        /// <summary>
        /// This will be invoked by LoadSBAsyncTask and loads the current beatmap's storyboard
        /// </summary>
        private Task LoadSBTask;

        private readonly Func<BackgroundStoryboardContainer> createSB;

        private BackgroundStoryboardContainer SBContainer;

        [Resolved]
        private IBindable<WorkingBeatmap> b { get; set; }

        public BackgroundStoryBoardLoader()
        {
            this.createSB = () => new BackgroundStoryboardContainer();

            RelativeSizeAxes = Axes.Both;
            Child = sbContainer = new Container
            {
                RelativeSizeAxes = Axes.Both
            };
        }

        [BackgroundDependencyLoader]
        private void load(MfConfigManager config)
        {
            config.BindWith(MfSetting.MvisEnableStoryboard, EnableSB);
        }

        protected override void LoadComplete()
        {
            EnableSB.ValueChanged += _ => UpdateVisuals();
        }

        protected override void Update()
        {
            if ( IsReady.Value )
                sbClock?.Seek(b.Value.Track.CurrentTime);
        }

        public void UpdateVisuals()
        {
            if ( EnableSB.Value )
            {
                if ( !SBLoaded.Value )
                    UpdateStoryBoardAsync();
                else
                {
                    storyboardReplacesBackground.Value = b.Value.Storyboard.ReplacesBackground && b.Value.Storyboard.HasDrawable;
                    NeedToHideTriangles.Value = b.Value.Storyboard.HasDrawable;
                }

                sbClock?.FadeIn(DURATION, Easing.OutQuint);
            }
            else
            {
                sbClock?.FadeOut(DURATION / 2, Easing.OutQuint);
                storyboardReplacesBackground.Value = false;
                NeedToHideTriangles.Value = false;
                IsReady.Value = true;
                CancelAllTasks();
            }
        }

        public bool UpdateComponent(WorkingBeatmap beatmap)
        {
            try
            {
                SBContainer = createSB();

                sbClock?.FadeOut(DURATION, Easing.OutQuint);
                sbClock?.Expire();

                LoadSBTask = LoadComponentAsync(new ClockContainer(beatmap, 0)
                {
                    Name = "ClockContainer",
                    Alpha = 0,
                    Child = new BackgroundStoryboardContainer(),
                }, newsbClock =>
                {
                    sbClock = newsbClock;

                    sbContainer.Add(sbClock);

                    if ( beatmap.Track.IsRunning == true )
                        sbClock.Start();
                    else
                        sbClock.Stop();

                    sbClock.Seek(beatmap.Track.CurrentTime);

                    SBLoaded.Value = true;
                    IsReady.Value = true;
                    NeedToHideTriangles.Value = beatmap.Storyboard.HasDrawable;

                    UpdateVisuals();

                    Logger.Log($"Load Storyboard for Beatmap \"{beatmap.BeatmapSetInfo}\" complete!");
                }, (ChangeSB = new CancellationTokenSource()).Token);
            }
            catch (Exception e)
            {
                Logger.Error(e, "加载Storyboard时出现错误! 请检查你的谱面!");
                return false;
            }

            return true;
        }

        public void CancelAllTasks()
        {
            ChangeSB?.Cancel();

            LoadSBTask = null;
            LoadSBAsyncTask = null;
            LogTask = null;
        }

        public void UpdateStoryBoardAsync( float displayDelay = 0 )
        {
            if ( b == null )
                return;

            CancelAllTasks();
            IsReady.Value = false;
            SBLoaded.Value = false;
            NeedToHideTriangles.Value = false;

            if ( !EnableSB.Value )
            {
                IsReady.Value = true;
                return;
            }

            Schedule(() =>
            {
                LoadSBAsyncTask = Task.Run( async () =>
                {
                    Logger.Log($"Loading Storyboard for Beatmap \"{b.Value.BeatmapSetInfo}\"...");

                    storyboardReplacesBackground.Value = false;

                    LogTask = Task.Run( () => 
                    {
                        UpdateComponent(b.Value);
                    });

                    await LogTask;
                });
            });
        }
    }
}