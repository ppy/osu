using System;
using Mvis.Plugin.RulesetPanel.Config;
using Mvis.Plugin.RulesetPanel.Objects;
using Mvis.Plugin.RulesetPanel.UI;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Screens.Mvis.Plugins;
using osu.Game.Screens.Mvis.Plugins.Config;
using osu.Game.Screens.Mvis.Plugins.Types;
using osuTK;

namespace Mvis.Plugin.RulesetPanel
{
    public class RulesetPanel : BindableControlledPlugin
    {
        public override TargetLayer Target => TargetLayer.Foreground;
        public override int Version => 3;

        public RulesetPanel()
        {
            Name = "Mvis2Player";
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

        private readonly Bindable<bool> showParticles = new BindableBool();
        private readonly BindableFloat idleAlpha = new BindableFloat();

        private readonly Bindable<float> xPos = new Bindable<float>(0.5f);
        private readonly Bindable<float> yPos = new Bindable<float>(0.5f);

        [BackgroundDependencyLoader]
        private void load()
        {
            var config = (RulesetPanelConfigManager)Dependencies.Get<MvisPluginManager>().GetConfigManager(this);

            config.BindWith(RulesetPanelSetting.ShowParticles, showParticles);
            config.BindWith(RulesetPanelSetting.EnableRulesetPanel, Value);
            config.BindWith(RulesetPanelSetting.IdleAlpha, idleAlpha);

            config.BindWith(RulesetPanelSetting.LogoPositionX, xPos);
            config.BindWith(RulesetPanelSetting.LogoPositionY, yPos);

            idleAlpha.BindValueChanged(onIdleAlphaChanged);

            if (MvisScreen != null)
            {
                MvisScreen.OnIdle += () => idleAlpha.TriggerChange();
                MvisScreen.OnResumeFromIdle += () =>
                {
                    if (Value.Value)
                        this.FadeTo(1, 750, Easing.OutQuint);
                };
                MvisScreen.OnBeatmapChanged += onMvisBeatmapChanged;
            }
        }

        public Action<WorkingBeatmap> OnMvisBeatmapChanged;

        private Container particlesPlaceholder;
        private BeatmapLogo logo;

        private void onMvisBeatmapChanged(WorkingBeatmap b) => OnMvisBeatmapChanged?.Invoke(b);

        private void onIdleAlphaChanged(ValueChangedEvent<float> v)
        {
            if ((MvisScreen?.OverlaysHidden ?? true) && Value.Value)
                this.FadeTo(v.NewValue, 750, Easing.OutQuint);
        }

        public override IPluginConfigManager CreateConfigManager(Storage storage)
            => new RulesetPanelConfigManager(storage);

        public override PluginSettingsSubSection CreateSettingsSubSection()
            => new RulesetPanelSettings(this);

        protected override Drawable CreateContent() => new Container
        {
            RelativeSizeAxes = Axes.Both,
            Children = new Drawable[]
            {
                particlesPlaceholder = new Container
                {
                    RelativeSizeAxes = Axes.Both
                },
                logo = new BeatmapLogo
                {
                    RelativePositionAxes = Axes.Both
                }
            }
        };

        protected override bool OnContentLoaded(Drawable content)
        {
            showParticles.BindValueChanged(v =>
            {
                if (v.NewValue)
                {
                    particlesPlaceholder.Child = new Particles();
                    return;
                }

                particlesPlaceholder.Clear();
            }, true);

            logo.X = xPos.Value;
            logo.Y = yPos.Value;

            xPos.BindValueChanged(x =>
            {
                logo.MoveToX(x.NewValue, 200, Easing.OutQuint);
            }, true);

            yPos.BindValueChanged(y =>
            {
                logo.MoveToY(y.NewValue, 200, Easing.OutQuint);
            }, true);

            //MvisScreen.OnScreenExiting += beatmapLogo.StopResponseOnBeatmapChanges;
            //MvisScreen.OnScreenSuspending += beatmapLogo.StopResponseOnBeatmapChanges;
            //MvisScreen.OnScreenResuming += () =>
            //{
            //    if (!Disabled.Value) beatmapLogo.ResponseOnBeatmapChanges();
            //};
            //
            //beatmapLogo.ResponseOnBeatmapChanges();

            return true;
        }

        protected override bool PostInit() => true;

        public override bool Disable()
        {
            this.FadeOut(300, Easing.OutQuint).ScaleTo(0.8f, 400, Easing.OutQuint);
            //beatmapLogo?.StopResponseOnBeatmapChanges();

            return base.Disable();
        }

        public override bool Enable()
        {
            this.FadeTo(MvisScreen?.OverlaysHidden ?? false ? idleAlpha.Value : 1, 300).ScaleTo(1, 400, Easing.OutQuint);

           // beatmapLogo?.ResponseOnBeatmapChanges();

            return base.Enable();
        }

        public override void UnLoad()
        {
            MvisScreen.OnBeatmapChanged -= onMvisBeatmapChanged;

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
