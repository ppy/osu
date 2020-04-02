// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osuTK;
using osu.Game.Screens.Play.PlayerSettings;
using osuTK.Input;
using osu.Framework.Bindables;
using osu.Framework.Allocation;
using osu.Game.Configuration;

namespace osu.Game.Screens.Play.HUD
{
    public class BreakSettingsOverlay : VisibilityContainer
    {
        private readonly Bindable<bool> Optui = new Bindable<bool>();
        private const int fade_duration = 200;

        public bool ReplayLoaded;

        public readonly VisualSettings VisualSettings;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            config.BindWith(OsuSetting.OptUI, Optui);

            Optui.ValueChanged += _ => UpdateVisibilities();
            UpdateVisibilities();
        }

        public BreakSettingsOverlay()
        {
            AlwaysPresent = true;
            RelativeSizeAxes = Axes.Both;

            Child = new FillFlowContainer<PlayerSettingsGroup>
            {
                Origin = Anchor.TopRight,
                Anchor = Anchor.TopRight,
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 20),
                Margin = new MarginPadding { Top = 100, Right = 10 },
                Children = new PlayerSettingsGroup[]
                {
                    VisualSettings = new VisualSettings { Expanded = false , OptUIEnabled = Optui.Value }
                }
            };
        }

        private void UpdateVisibilities()
        {
            switch (Optui.Value)
            {
                case true:
                    VisualSettings.FadeTo(0.5f, 250);
                    break;

                case false:
                    VisualSettings.FadeOut(250);
                    break;
            }

        }

        protected override void PopIn() => this.FadeIn(fade_duration);
        protected override void PopOut() => this.FadeOut(fade_duration);

        //We want to handle keyboard inputs all the time in order to trigger ToggleVisibility() when not visible
        public override bool PropagateNonPositionalInputSubTree => true;

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (e.Repeat) return false;

            if (e.ControlPressed)
            {
                if (e.Key == Key.H && ReplayLoaded)
                {
                    ToggleVisibility();
                    return true;
                }
            }

            return base.OnKeyDown(e);
        }
    }
}
