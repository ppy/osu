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

        private DecoupleableInterpolatingFramedClock storyboardClock;

        private BackgroundStoryboard currentStoryboard;

        private WorkingBeatmap targetBeatmap;

        [Resolved]
        private MusicController music { get; set; }

        [Resolved]
        private Bindable<WorkingBeatmap> currentBeatmap { get; set; }

        public BackgroundStoryBoardLoader()
        {
            RelativeSizeAxes = Axes.Both;

            Name = "故事版加载器(内置)";
            Description = "用于呈现故事版; Mfosu自带插件";
            Author = "mf-osu";

            Flags.AddRange(new[]
            {
                PluginFlags.CanDisable,
                PluginFlags.CanUnload,
                PluginFlags.HasConfig
            });
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var config = (SbLoaderConfigManager)DependenciesContainer.Get<MvisPluginManager>().GetConfigManager(this);
            config.BindWith(SbLoaderSettings.EnableStoryboard, Value);

            currentBeatmap.BindValueChanged(v =>
            {
                if (v.NewValue == targetBeatmap)
                {
                    storyboardClock?.Start();
                    storyboardClock?.ChangeSource(music.CurrentTrack);
                }
            });

            if (MvisScreen != null)
            {
                MvisScreen.OnScreenExiting += UnLoad;
                MvisScreen.OnBeatmapChanged += refresh;
            }
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
                if (ContentLoaded)
                    currentStoryboard?.FadeOut(STORYBOARD_FADEOUT_DURATION, Easing.OutQuint);

                StoryboardReplacesBackground.Value = false;
                NeedToHideTriangles.Value = false;
            }
        }

        protected override Drawable CreateContent() => currentStoryboard = new BackgroundStoryboard(targetBeatmap)
        {
            RunningClock = storyboardClock,
            Alpha = 0.1f
        };

        public override IPluginConfigManager CreateConfigManager(Storage storage) => new SbLoaderConfigManager(storage);

        public override PluginSettingsSubSection CreateSettingsSubSection() => new StoryboardSettings(this);

        protected override bool PostInit()
        {
            if (Disabled.Value)
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

        private Drawable prevProxy;

        protected override bool OnContentLoaded(Drawable content)
        {
            var newStoryboard = (BackgroundStoryboard)content;

            //bug: 过早Seek至歌曲时间会导致部分故事版加载过程僵死
            storyboardClock.ChangeSource(music.CurrentTrack);
            Seek(music.CurrentTrack.CurrentTime);

            sbLoaded.Value = true;
            NeedToHideTriangles.Value = targetBeatmap.Storyboard.HasDrawable;

            if (MvisScreen != null)
                MvisScreen.OnSeek += Seek;

            Value.TriggerChange();

            if (prevProxy != null)
            {
                MvisScreen?.ProxyLayer.Remove(prevProxy);
                prevProxy.Expire();
            }

            prevProxy = getProxy(newStoryboard);

            if (prevProxy != null) MvisScreen?.ProxyLayer.Add(prevProxy);
            prevProxy?.Show();

            if (MvisScreen != null)
            {
                MvisScreen.HideTriangles.Value = NeedToHideTriangles.Value;
                MvisScreen.HideScreenBackground.Value = targetBeatmap.Storyboard.ReplacesBackground;
            }

            return true;
        }

        public override void UnLoad()
        {
            ClearInternal();

            if (MvisScreen != null)
            {
                MvisScreen.OnBeatmapChanged -= refresh;
                MvisScreen.OnScreenExiting -= UnLoad;
                MvisScreen.OnSeek -= Seek;
            }

            NeedToHideTriangles.Value = false;
            StoryboardReplacesBackground.Value = false;

            NeedToHideTriangles.UnbindAll();
            StoryboardReplacesBackground.UnbindAll();

            base.UnLoad();
        }

        private void refresh(WorkingBeatmap newBeatmap)
        {
            if (!ContentLoaded)
            {
                Cancel();
                currentStoryboard?.Expire();
                currentStoryboard?.Dispose();
            }
            else
                currentStoryboard?.FadeTo(0.01f, 300, Easing.OutQuint).Expire();

            targetBeatmap = newBeatmap;
            Load();
        }

        private Drawable getProxy(BackgroundStoryboard storyboard)
        {
            if (storyboard != currentStoryboard) return new Container();

            return storyboard.StoryboardProxy();
        }

        public void Seek(double position) =>
            storyboardClock?.Seek(position);
    }
}
