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

namespace osu.Game.Screens.Mvis
{
    public class BackgroundStoryBoard : Container
    {
        private const float DURATION = 750;
        public Container sbContainer;
        public ClockContainer sbClock;
        private CancellationTokenSource ChangeSB;
        private DimmableStoryboard dimmableStoryboard;
        private Bindable<bool> EnableSB = new Bindable<bool>();
        public readonly Bindable<bool> IsReady = new Bindable<bool>();
        public readonly Bindable<bool> storyboardReplacesBackground = new Bindable<bool>();

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

        [Resolved]
        private IBindable<WorkingBeatmap> b { get; set; }

        public BackgroundStoryBoard()
        {
            RelativeSizeAxes = Axes.Both;
            Child = sbContainer = new Container
            {
                RelativeSizeAxes = Axes.Both
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            config.BindWith(OsuSetting.MvisEnableStoryboard, EnableSB);
        }

        protected override void LoadComplete()
        {
            EnableSB.ValueChanged += _ => UpdateVisuals();
            dimmableStoryboard?.StoryboardReplacesBackground.BindTo(storyboardReplacesBackground);
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
                storyboardReplacesBackground.Value = b.Value.Storyboard.ReplacesBackground && b.Value.Storyboard.HasDrawable;;
                sbContainer.FadeIn(DURATION, Easing.OutQuint);
            }
            else
            {
                storyboardReplacesBackground.Value = false;
                sbContainer.FadeOut(DURATION, Easing.OutQuint);
            }
        }

        public bool UpdateComponent()
        {
            try
            {
                LoadSBTask = LoadComponentAsync(new ClockContainer(b.Value, 0)
                {
                    Name = "ClockContainer",
                    Alpha = 0,
                    Child = dimmableStoryboard = new DimmableStoryboard(b.Value.Storyboard)
                    {
                        RelativeSizeAxes = Axes.Both,
                        Name = "Storyboard"
                    }
                }, newsbClock =>
                {
                    sbClock = newsbClock;

                    dimmableStoryboard.IgnoreUserSettings.Value = true;

                    sbContainer.Add(sbClock);

                    sbClock.FadeIn(DURATION, Easing.OutQuint);
                    sbClock.Start();
                    sbClock.Seek(b.Value.Track.CurrentTime);

                    IsReady.Value = true;
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
            ChangeSB = new CancellationTokenSource();

            LoadSBTask = null;
            LoadSBAsyncTask = null;
            LogTask = null;
        }

        public void UpdateStoryBoardAsync()
        {
            if ( b == null )
                return;

            Schedule(() =>
            {
                CancelAllTasks();

                IsReady.Value = false;

                var lastdimmableSB = dimmableStoryboard;

                lastdimmableSB?.FadeOut(DURATION, Easing.OutQuint);
                sbClock?.FadeOut(DURATION, Easing.OutQuint);

                lastdimmableSB?.Expire();
                sbClock?.Expire();

                LoadSBAsyncTask = Task.Run( async () =>
                {
                    Logger.Log($"Loading Storyboard for Beatmap \"{b.Value.BeatmapSetInfo}\"...");

                    storyboardReplacesBackground.Value = b.Value.Storyboard.ReplacesBackground && b.Value.Storyboard.HasDrawable;

                    UpdateVisuals();

                    LogTask = Task.Run( () => 
                    {
                        UpdateComponent();
                    });

                    await LogTask;
                });
            });
        }
    }
}