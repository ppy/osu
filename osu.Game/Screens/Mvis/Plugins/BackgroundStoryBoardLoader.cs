using System;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Overlays;
using osu.Game.Screens.Mvis.Plugins.Storyboard;

namespace osu.Game.Screens.Mvis.Plugins
{
    ///<summary>
    /// 负责故事版的异步加载功能
    ///</summary>
    public class BackgroundStoryBoardLoader : MvisPlugin
    {
        public const float STORYBOARD_FADEIN_DURATION = 750;
        public const float STORYBOARD_FADEOUT_DURATION = STORYBOARD_FADEIN_DURATION / 2;
        private Bindable<bool> disableSb;

        ///<summary>
        ///用于内部确定故事版是否已加载
        ///</summary>
        private readonly BindableBool sbLoaded = new BindableBool();

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

        [Resolved]
        private MvisPluginManager manager { get; set; }

        public BackgroundStoryBoardLoader(WorkingBeatmap working)
        {
            RelativeSizeAxes = Axes.Both;
            targetBeatmap = working;

            Name = "故事版加载器(Mf-osu自带插件)";
            Description = "用于呈现故事版; Mfosu自带插件";

            Flags.Add(PluginFlags.CanDisable);
        }

        [BackgroundDependencyLoader]
        private void load(MConfigManager config)
        {
            disableSb = config.GetBindable<bool>(MSetting.MvisDisableStoryboard);
            Disabled.BindTo(disableSb);

            currentBeatmap.BindValueChanged(v =>
            {
                if (v.NewValue == targetBeatmap)
                {
                    storyboardClock.Start();
                    storyboardClock.ChangeSource(music.CurrentTrack);
                }
            });
        }

        protected override void LoadComplete()
        {
            disableSb.BindValueChanged(OnDisableSBChanged, true);
            base.LoadComplete();
        }

        protected override Drawable CreateContent() => currentStoryboard = new BackgroundStoryboard(targetBeatmap)
        {
            RunningClock = storyboardClock,
            Alpha = 0.1f
        };

        protected override bool PostInit()
        {
            if (disableSb.Value)
                return false;

            if (targetBeatmap == null)
                throw new InvalidOperationException("currentBeatmap 不能为 null");

            sbLoaded.Value = false;
            NeedToHideTriangles.Value = false;
            StoryboardReplacesBackground.Value = false;

            storyboardClock = new DecoupleableInterpolatingFramedClock
            {
                IsCoupled = true,
                DisableSourceAdjustment = true
            };

            return true;
        }

        protected override bool OnContentLoaded(Drawable content)
        {
            //Logger.Log("故事版OnContentLoaded");
            var newStoryboard = (BackgroundStoryboard)content;

            //bug: 过早Seek至歌曲时间会导致部分故事版加载过程僵死
            storyboardClock.ChangeSource(music.CurrentTrack);
            Seek(music.CurrentTrack.CurrentTime);

            sbLoaded.Value = true;
            NeedToHideTriangles.Value = targetBeatmap.Storyboard.HasDrawable;

            setProxy(newStoryboard);

            disableSb.TriggerChange();
            OnNewStoryboardLoaded?.Invoke();

            return true;
        }

        [CanBeNull]
        public Drawable StoryboardProxy;

        private void setProxy(BackgroundStoryboard storyboard)
        {
            if (storyboard != currentStoryboard) return;

            StoryboardProxy = storyboard.StoryboardProxy();
        }

        public void OnDisableSBChanged(ValueChangedEvent<bool> v)
        {
            if (v.NewValue)
            {
                manager.DisablePlugin(this);

                if (ContentLoaded)
                    currentStoryboard?.FadeOut(STORYBOARD_FADEOUT_DURATION, Easing.OutQuint);

                StoryboardReplacesBackground.Value = false;
                NeedToHideTriangles.Value = false;
            }
            else
            {
                manager.ActivePlugin(this);

                if (ContentLoaded)
                {
                    StoryboardReplacesBackground.Value = targetBeatmap.Storyboard.ReplacesBackground && targetBeatmap.Storyboard.HasDrawable;
                    NeedToHideTriangles.Value = targetBeatmap.Storyboard.HasDrawable;
                    currentStoryboard?.FadeIn(STORYBOARD_FADEIN_DURATION, Easing.OutQuint);
                }
            }
        }

        public void Seek(double position) =>
            storyboardClock?.Seek(position);

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (isDisposing)
                Cancel();
        }

        public override bool Disable()
        {
            disableSb.Value = true;
            return base.Disable();
        }

        public override bool Enable()
        {
            disableSb.Value = false;
            return base.Enable();
        }

        public override void UnLoad()
        {
            disableSb.UnbindAll();
            base.UnLoad();
        }
    }
}
