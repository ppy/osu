using System;
using System.Threading;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
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
        private WorkingBeatmap Beatmap;
        public readonly Bindable<bool> IsReady = new Bindable<bool>();
        public readonly Bindable<bool> SBReplacesBg = new Bindable<bool>();
        private readonly Bindable<bool> storyboardReplacesBackground = new Bindable<bool>();
        public BgStoryBoard(WorkingBeatmap beatmap = null)
        {
            this.Beatmap = beatmap;
            RelativeSizeAxes = Axes.Both;
            Children = new Drawable[]
            {
                sbContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both
                },
            };
        }
        protected override void LoadComplete()
        {
            dimmableStoryboard?.StoryboardReplacesBackground.BindTo(storyboardReplacesBackground);
            SBReplacesBg.BindTo(storyboardReplacesBackground);
            UpdateComponent(Beatmap);
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
                }, (ChangeSB = new CancellationTokenSource()).Token);
            }
            catch (Exception e)
            {
                Logger.Error(e, "加载Storyboard时出现错误! 请检查你的谱面!");
                return false;
            }

            return true;
        }
    }
}