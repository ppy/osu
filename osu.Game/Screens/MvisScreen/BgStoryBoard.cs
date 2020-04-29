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
        public readonly Bindable<bool> SBReplacesBg = new Bindable<bool>();
        private readonly Bindable<bool> storyboardReplacesBackground = new Bindable<bool>();
        private Task LogTask;
        private Task LoadSBAsyncTask;

        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; }

        public BgStoryBoard(WorkingBeatmap beatmap = null)
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
            SBReplacesBg.BindTo(storyboardReplacesBackground);
        }

        public void UpdateVisuals()
        {
            if ( EnableSB.Value )
            {
                storyboardReplacesBackground.Value = beatmap.Value.Storyboard.ReplacesBackground && beatmap.Value.Storyboard.HasDrawable;;
                sbClock?.FadeIn(DURATION, Easing.OutQuint);
            }
            else
            {
                storyboardReplacesBackground.Value = false;
                sbClock?.FadeOut(DURATION, Easing.OutQuint);
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

        public bool UpdateComponent(WorkingBeatmap b)
        {
            if ( b == null )
                return false;

            storyboardReplacesBackground.Value = b.Storyboard.ReplacesBackground && b.Storyboard.HasDrawable;

            IsReady.Value = false;

            ChangeSB?.Cancel();

            try
            {
                LoadComponentAsync(new ClockContainer(b, 0)
                {
                    Child = dimmableStoryboard = new DimmableStoryboard(b.Storyboard) { RelativeSizeAxes = Axes.Both }
                }, newsbC =>
                {
                    sbClock?.Stop();
                    sbClock?.Hide();
                    sbClock?.Expire();
                    sbClock = newsbC;

                    dimmableStoryboard.IgnoreUserSettings.Value = true;

                    sbContainer.Add(sbClock);
                    sbClock.Start();
                    sbClock.Seek(b.Track.CurrentTime);

                    IsReady.Value = true;

                    if ( !EnableSB.Value )
                        sbClock.Hide();

                }, (ChangeSB = new CancellationTokenSource()).Token);
            }
            catch (Exception e)
            {
                Logger.Error(e, "加载Storyboard时出现错误! 请检查你的谱面!");
                return false;
            }

            return true;
        }

        public Task UpdateStoryBoardAsync(WorkingBeatmap b) => LoadSBAsyncTask = Task.Run(async () =>
        {
            UpdateComponent(b);
            UpdateVisuals();

            try
            {
                LogTask = Task.Run( () => Logger.Log($"Loading Storyboard for Beatmap \"{b.BeatmapSetInfo}\"..."));
                await LogTask;
            }
            finally
            {
            }
        });
    }
}