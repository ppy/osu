using Mvis.Plugin.SandboxToPanel.RulesetComponents.Configuration;
using Mvis.Plugin.SandboxToPanel.RulesetComponents.Screens.Visualizer.Components;
using Mvis.Plugin.SandboxToPanel.RulesetComponents.Screens.Visualizer.Components.Settings;
using Mvis.Plugin.SandboxToPanel.RulesetComponents.UI;
using Mvis.Plugin.SandboxToPanel.RulesetComponents.UI.Settings;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Screens;
using osu.Game.Graphics.Cursor;
using osu.Game.Input;
using osu.Game.Input.Bindings;
using osuTK;

#nullable disable

namespace Mvis.Plugin.SandboxToPanel.RulesetComponents.Screens.Visualizer
{
    public partial class VisualizerScreen : SandboxScreenWithSettings, IKeyBindingHandler<GlobalAction>
    {
        public override bool AllowBackButton => false;

        public override bool HideOverlaysOnEnter => true;

        private readonly IBindable<bool> isIdle = new BindableBool();
        private readonly BindableBool showTip = new BindableBool();
        private CursorHider cursorHider;

        [BackgroundDependencyLoader]
        private void load(SandboxRulesetConfigManager config, IdleTracker idleTracker)
        {
            config.BindWith(SandboxRulesetSetting.ShowSettingsTip, showTip);
            isIdle.BindTo(idleTracker.IsIdle);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (showTip.Value)
            {
                var tip = new VisualizerSettingsTip();
                AddInternal(tip);
                tip.Show();
            }

            AddInternal(cursorHider = new CursorHider());

            isIdle.BindValueChanged(idle =>
            {
                if (idle.NewValue)
                {
                    SettingsVisible.Value = false;
                    cursorHider.Size = Vector2.One;
                }
                else
                {
                    cursorHider.Size = Vector2.Zero;
                }
            });
        }

        protected override Drawable CreateBackground() => new Container
        {
            RelativeSizeAxes = Axes.Both,
            Children = new Drawable[]
            {
                new StoryboardContainer(),
                new Particles()
            }
        };

        protected override Drawable CreateContent() => new LayoutController();

        protected override SandboxSettingsSection[] CreateSettingsSections() => new SandboxSettingsSection[]
        {
            new TrackSection(),
            new BackgroundSection(),
            new VisualizerSection()
        };

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            switch (e.Action)
            {
                case GlobalAction.Back:
                    this.Exit();
                    return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }

        private partial class CursorHider : CompositeDrawable, IProvideCursor
        {
            public CursorHider()
            {
                RelativeSizeAxes = Axes.Both;
                Size = Vector2.Zero;
            }

            public bool ProvidingUserCursor => true;

            public CursorContainer Cursor => new EmptyCursor();

            protected override bool OnHover(HoverEvent e)
            {
                base.OnHover(e);
                return true;
            }

            private partial class EmptyCursor : CursorContainer
            {
                protected override Drawable CreateCursor() => Empty();
            }
        }
    }
}
