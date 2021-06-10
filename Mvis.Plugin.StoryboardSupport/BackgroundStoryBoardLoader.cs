using System;
using Mvis.Plugin.StoryboardSupport.Config;
using Mvis.Plugin.StoryboardSupport.Storyboard;
using Mvis.Plugin.StoryboardSupport.UI;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Platform;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Overlays;
using osu.Game.Screens.Mvis.Plugins;
using osu.Game.Screens.Mvis.Plugins.Config;
using osu.Game.Screens.Mvis.Plugins.Types;
using osu.Game.Screens.Play;

namespace Mvis.Plugin.StoryboardSupport
{
    ///<summary>
    /// 负责故事版的异步加载功能
    ///</summary>
    public class BackgroundStoryBoardLoader : BindableControlledPlugin
    {
        public const float STORYBOARD_FADEIN_DURATION = 750;
        public const float STORYBOARD_FADEOUT_DURATION = STORYBOARD_FADEIN_DURATION / 2;

        ///<summary>
        ///用于内部确定故事版是否已加载
        ///</summary>
        private readonly BindableBool sbLoaded = new BindableBool();

        public readonly BindableBool NeedToHideTriangles = new BindableBool();
        public readonly BindableBool StoryboardReplacesBackground = new BindableBool();

        private BackgroundStoryboard currentStoryboard;

        private WorkingBeatmap targetBeatmap;

        [Resolved]
        private MusicController music { get; set; }

        [Resolved]
        private Bindable<WorkingBeatmap> currentBeatmap { get; set; }

        public BackgroundStoryBoardLoader()
        {
            RelativeSizeAxes = Axes.Both;

            Name = "故事版加载器";
            Description = "在播放器的背景显示谱面故事版";
            Author = "mf-osu";

            Flags.AddRange(new[]
            {
                PluginFlags.CanDisable,
                PluginFlags.CanUnload
            });
        }

        private readonly EpilepsyWarning epilepsyWarning = new EpilepsyWarning
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            Margin = new MarginPadding(20),
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Depth = -1
        };

        [BackgroundDependencyLoader]
        private void load()
        {
            var config = (SbLoaderConfigManager)DependenciesContainer.Get<MvisPluginManager>().GetConfigManager(this);
            config.BindWith(SbLoaderSettings.EnableStoryboard, Value);

            if (MvisScreen != null)
            {
                MvisScreen.OnScreenExiting += UnLoad;
                MvisScreen.OnScreenSuspending += onScreenSuspending;
                MvisScreen.OnScreenResuming += onScreenResuming;
                MvisScreen.OnBeatmapChanged(refresh, this);
            }

            if (MvisScreen != null)
                MvisScreen.AddDrawableToProxy(epilepsyWarning);
            else
                AddInternal(epilepsyWarning);
        }

        private void onScreenResuming()
        {
            if (!Disabled.Value && ContentLoaded)
                MvisScreen.HideScreenBackground.Value = targetBeatmap.Storyboard.ReplacesBackground;
        }

        private void onScreenSuspending()
        {
            MvisScreen.HideScreenBackground.Value = false;
        }

        protected override void OnValueChanged(ValueChangedEvent<bool> v)
        {
            base.OnValueChanged(v);

            if (v.NewValue)
            {
                if (ContentLoaded)
                {
                    StoryboardReplacesBackground.Value = targetBeatmap.Storyboard.ReplacesBackground && targetBeatmap.Storyboard.HasDrawable;
                    NeedToHideTriangles.Value = targetBeatmap.Storyboard.HasDrawable;
                    currentStoryboard?.FadeIn(STORYBOARD_FADEIN_DURATION, Easing.OutQuint);
                }
            }
            else
            {
                StoryboardReplacesBackground.Value = false;
                NeedToHideTriangles.Value = false;
            }
        }

        protected override Drawable CreateContent() => currentStoryboard = new BackgroundStoryboard(targetBeatmap)
        {
            RunningClock = new InterpolatingFramedClock(targetBeatmap.Track),
            Alpha = 0.1f
        };

        public override IPluginConfigManager CreateConfigManager(Storage storage) => new SbLoaderConfigManager(storage);

        public override PluginSettingsSubSection CreateSettingsSubSection() => new StoryboardSettings(this);

        protected override bool PostInit()
        {
            if (targetBeatmap == null)
                throw new InvalidOperationException("targetBeatmap 不能为 null");

            epilepsyWarning.ScaleTo(1);

            if (targetBeatmap.BeatmapInfo.EpilepsyWarning)
                epilepsyWarning.Show();
            else
                epilepsyWarning.Hide();

            sbLoaded.Value = false;
            NeedToHideTriangles.Value = false;
            StoryboardReplacesBackground.Value = false;

            return true;
        }

        public override int Version => 4;

        private Drawable prevProxy;

        protected override bool OnContentLoaded(Drawable content)
        {
            var newStoryboard = (BackgroundStoryboard)content;

            Seek(music.CurrentTrack.CurrentTime);

            sbLoaded.Value = true;
            NeedToHideTriangles.Value = targetBeatmap.Storyboard.HasDrawable;

            if (MvisScreen != null)
                MvisScreen.OnSeek += Seek;

            Value.TriggerChange();

            if (prevProxy != null)
            {
                MvisScreen?.RemoveDrawableFromProxy(prevProxy);
                prevProxy.Expire();
            }

            prevProxy = getProxy(newStoryboard);

            if (prevProxy != null) MvisScreen?.AddDrawableToProxy(prevProxy);
            prevProxy?.Show();

            if (MvisScreen != null)
            {
                MvisScreen.HideTriangles.Value = NeedToHideTriangles.Value;
                MvisScreen.HideScreenBackground.Value = targetBeatmap.Storyboard.ReplacesBackground;
            }

            if (targetBeatmap.BeatmapInfo.EpilepsyWarning)
                epilepsyWarning.ScaleTo(1.001f, 5000).OnComplete(_ => epilepsyWarning.Hide());

            return true;
        }

        public override void UnLoad()
        {
            if (ContentLoaded)
                currentStoryboard?.FadeTo(0.01f, 250, Easing.OutQuint).Expire();

            currentStoryboard = null;

            if (MvisScreen != null)
            {
                MvisScreen.OnScreenSuspending -= onScreenSuspending;
                MvisScreen.OnScreenResuming -= onScreenResuming;
                MvisScreen.OnScreenExiting -= UnLoad;
                MvisScreen.OnSeek -= Seek;
            }

            NeedToHideTriangles.Value = false;
            StoryboardReplacesBackground.Value = false;

            NeedToHideTriangles.UnbindAll();
            StoryboardReplacesBackground.UnbindAll();

            this.FadeTo(0.01f, 300, Easing.OutQuint);

            base.UnLoad();
        }

        public override bool Disable()
        {
            if (MvisScreen != null)
            {
                MvisScreen.HideTriangles.Value = false;
                MvisScreen.HideScreenBackground.Value = false;
            }

            hideOrCancelLoadStoryboard(false);

            return base.Disable();
        }

        public override bool Enable()
        {
            if (MvisScreen != null && ContentLoaded)
            {
                MvisScreen.HideTriangles.Value = targetBeatmap.Storyboard.HasDrawable;
                MvisScreen.HideScreenBackground.Value = targetBeatmap.Storyboard.ReplacesBackground;
            }

            return base.Enable();
        }

        private void hideOrCancelLoadStoryboard(bool expireIfLoaded)
        {
            if (!ContentLoaded)
            {
                Cancel();

                currentStoryboard?.Expire();
                currentStoryboard?.Dispose();
                currentStoryboard = null;
            }
            else if (expireIfLoaded)
                currentStoryboard?.FadeTo(0.01f, 300, Easing.OutQuint).Expire();
            else
                currentStoryboard?.FadeTo(0, 300, Easing.OutQuint);
        }

        private void refresh(WorkingBeatmap newBeatmap)
        {
            hideOrCancelLoadStoryboard(true);
            if (Disabled.Value && newBeatmap != targetBeatmap) ContentLoaded = false;

            targetBeatmap = newBeatmap;

            if (!Disabled.Value)
                Load();
        }

        private Drawable getProxy(BackgroundStoryboard storyboard)
        {
            if (storyboard != currentStoryboard) return new Container();

            return storyboard.StoryboardProxy();
        }

        public void Seek(double position)
        {
        }
    }
}
