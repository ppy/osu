// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Configuration;
using osu.Game.Input.Bindings;

namespace osu.Game.Screens.Play.HUD
{
    public partial class ReplayOverlay : CompositeDrawable, IKeyBindingHandler<GlobalAction>
    {
        public ReplaySettingsOverlay Settings { get; private set; } = null!;

        private const int fade_duration = 200;

        private Bindable<bool> configSettingsOverlay = null!;
        private Container messageContainer = null!;
        private Container content = null!;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            RelativeSizeAxes = Axes.Both;

            configSettingsOverlay = config.GetBindable<bool>(OsuSetting.ReplaySettingsOverlay);

            InternalChild = content = new Container
            {
                Alpha = 0,
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    messageContainer = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Depth = float.MaxValue,
                    },
                    Settings = new ReplaySettingsOverlay(),
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            configSettingsOverlay.BindValueChanged(_ => updateVisibility(), true);
        }

        private void updateVisibility()
        {
            if (configSettingsOverlay.Value)
                content.FadeIn(fade_duration, Easing.OutQuint);
            else
                content.FadeOut(fade_duration, Easing.OutQuint);
        }

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (e.Repeat)
                return false;

            switch (e.Action)
            {
                case GlobalAction.ToggleReplaySettings:
                    configSettingsOverlay.Value = !configSettingsOverlay.Value;
                    return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }

        public override void Show() => this.FadeIn(fade_duration, Easing.OutQuint);
        public override void Hide() => this.FadeOut(fade_duration, Easing.OutQuint);

        public void SetMessage(ScrollingMessage scrollingMessage) => messageContainer.Child = scrollingMessage;
    }
}
