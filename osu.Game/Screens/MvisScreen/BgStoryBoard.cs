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
    public class BgStoryBoard : Container
    {
        private const float DURATION = 750;
        private Container sbContainer;
        public ClockContainer sbClock;
        private CancellationTokenSource ChangeSB;
        private DimmableStoryboard dimmableStoryboard;
        private Bindable<bool> EnableSB = new Bindable<bool>();
        public readonly Bindable<bool> IsReady = new Bindable<bool>();
        public readonly Bindable<bool> storyboardReplacesBackground = new Bindable<bool>();
        private Task LogTask;
        private Task LoadSBAsyncTask;

        [Resolved]
        private IBindable<WorkingBeatmap> b { get; set; }

        public BgStoryBoard()
        {
            RelativeSizeAxes = Axes.Both;
            Children = new Drawable[]
            {
                sbContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            config.BindWith(OsuSetting.MvisEnableStoryboard, EnableSB);

            EnableSB.ValueChanged += _ => UpdateVisuals();
        }

        protected override void LoadComplete()
        {
            dimmableStoryboard?.StoryboardReplacesBackground.BindTo(storyboardReplacesBackground);
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

        public void CancelUpdateComponent()
        {
            ChangeSB?.Cancel();
            ChangeSB = new CancellationTokenSource();

            if ( LoadSBAsyncTask?.IsCompleted != true || LogTask?.IsCompleted != true )
            {
                LoadSBAsyncTask = null;
                LogTask = null;
            }
        }

        public bool UpdateComponent()
        {
            if ( b == null )
                return false;

            IsReady.Value = false;

            sbClock?.FadeOut(DURATION, Easing.OutQuint);
            sbClock?.Expire();

            storyboardReplacesBackground.Value = b.Value.Storyboard.ReplacesBackground && b.Value.Storyboard.HasDrawable;

            ChangeSB?.Cancel();

            try
            {
                LoadComponentAsync(new ClockContainer(b.Value, 0)
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

        public Task UpdateStoryBoardAsync() => LoadSBAsyncTask = Task.Run(async () =>
        {
            UpdateComponent();
            UpdateVisuals();

            LogTask = Task.Run( () => Logger.Log($"Loading Storyboard for Beatmap \"{b.Value.BeatmapSetInfo}\"..."));
            await LogTask;
        });
    }
}