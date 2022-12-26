// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osuTK;
using osu.Game.Screens.Play.PlayerSettings;
using osuTK.Input;
using osu.Game.Configuration;
using osu.Framework.Allocation;

namespace osu.Game.Screens.Play.HUD
{
    public class PlayerSettingsOverlay : VisibilityContainer
    {
        private const int fade_duration = 200;

        private Bindable<bool> playbackMenuExpanded;

        private Bindable<bool> visualMenuExpanded;

        public bool ReplayLoaded;

        public readonly PlaybackSettings PlaybackSettings;

        public readonly VisualSettings VisualSettings;

        public PlayerSettingsOverlay()
        {
            AlwaysPresent = true;

            Anchor = Anchor.TopRight;
            Origin = Anchor.TopRight;
            AutoSizeAxes = Axes.Both;

            Child = new FillFlowContainer<PlayerSettingsGroup>
            {
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 20),
                Children = new PlayerSettingsGroup[]
                {
                    //CollectionSettings = new CollectionSettings(),
                    //DiscussionSettings = new DiscussionSettings(),
                    PlaybackSettings = new PlaybackSettings(),
                    VisualSettings = new VisualSettings()
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(SessionStatics statics)
        {
            playbackMenuExpanded = statics.GetBindable<bool>(Static.ReplayPlaybackSettingExpanded);
            visualMenuExpanded = statics.GetBindable<bool>(Static.ReplayVisualSettingsExpanded);

            PlaybackSettings.Expanded.BindTo(playbackMenuExpanded);
            VisualSettings.Expanded.BindTo(visualMenuExpanded);
        }

        protected override void PopIn() => this.FadeIn(fade_duration);
        protected override void PopOut() => this.FadeOut(fade_duration);

        // We want to handle keyboard inputs all the time in order to trigger ToggleVisibility() when not visible
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
