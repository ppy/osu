using Mvis.Plugin.Sandbox.Components;
using Mvis.Plugin.Sandbox.Config;
using Mvis.Plugin.Sandbox.UI;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Screens.Mvis.Plugins;
using osu.Game.Screens.Mvis.Plugins.Config;
using osu.Game.Screens.Mvis.Plugins.Types;
using osuTK;

namespace Mvis.Plugin.Sandbox
{
    [Cached]
    public class SandboxPanel : BindableControlledPlugin
    {
        public override TargetLayer Target => TargetLayer.Foreground;
        public override int Version => 5;
        public Bindable<WorkingBeatmap> CurrentBeatmap = new Bindable<WorkingBeatmap>();

        public SandboxPanel()
        {
            Name = "Sandbox";
            Description = "可能是最好的osu!音乐可视化";
            Author = "EVAST9919; mf-osu";

            Flags.AddRange(new[]
            {
                PluginFlags.CanDisable,
                PluginFlags.CanUnload
            });

            RelativeSizeAxes = Axes.Both;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Scale = new Vector2(0.8f);
        }

        private readonly BindableFloat idleAlpha = new BindableFloat();

        [BackgroundDependencyLoader]
        private void load()
        {
            idleAlpha.BindValueChanged(onIdleAlphaChanged);

            var config = (SandboxConfigManager)Dependencies.Get<MvisPluginManager>().GetConfigManager(this);

            config.BindWith(SandboxSetting.EnableRulesetPanel, Value);
            config.BindWith(SandboxSetting.IdleAlpha, idleAlpha);

            if (MvisScreen != null)
            {
                MvisScreen.OnIdle += () => idleAlpha.TriggerChange();
                MvisScreen.OnResumeFromIdle += () =>
                {
                    if (Value.Value)
                        this.FadeTo(1, 750, Easing.OutQuint);

                    CurrentBeatmap.Disabled = false;
                    MvisScreen?.OnBeatmapChanged(onBeatmapChanged, this, true);
                };
            }
        }

        private void onIdleAlphaChanged(ValueChangedEvent<float> v)
        {
            if ((MvisScreen?.OverlaysHidden ?? true) && Value.Value)
            {
                this.FadeTo(v.NewValue, 750, Easing.OutQuint);
                if (v.NewValue == 0) CurrentBeatmap.Disabled = true;
            }
        }

        public override IPluginConfigManager CreateConfigManager(Storage storage)
            => new SandboxConfigManager(storage);

        public override PluginSettingsSubSection CreateSettingsSubSection()
            => new SandboxSettings(this);

        public override PluginSidebarSettingsSection CreateSidebarSettingsSection()
            => new RulesetPanelSidebarSection(this);

        protected override Drawable CreateContent() => new LayoutController();

        protected override bool OnContentLoaded(Drawable content) => true;

        protected override bool PostInit() => true;

        public override bool Disable()
        {
            this.FadeOut(300, Easing.OutQuint).ScaleTo(0.8f, 400, Easing.OutQuint);

            return base.Disable();
        }

        public override bool Enable()
        {
            bool result = base.Enable();

            this.FadeTo(MvisScreen?.OverlaysHidden ?? false ? idleAlpha.Value : 1, 300).ScaleTo(1, 400, Easing.OutQuint);
            MvisScreen?.OnBeatmapChanged(onBeatmapChanged, this, true);

            return result;
        }

        private void onBeatmapChanged(WorkingBeatmap working)
        {
            if (Disabled.Value || CurrentBeatmap.Disabled) return;

            CurrentBeatmap.Value = working;
        }

        public override void UnLoad()
        {
            if (ContentLoaded)
            {
                //MvisScreen.OnScreenExiting -= beatmapLogo.StopResponseOnBeatmapChanges;
                //MvisScreen.OnScreenSuspending -= beatmapLogo.StopResponseOnBeatmapChanges;
            }

            Value.UnbindAll();
            Disable();

            //bug: 直接调用Expire会导致面板直接消失
            this.Delay(400).Expire();
        }
    }
}
