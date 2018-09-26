// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.EventArgs;
using osu.Framework.Input.States;
using OpenTK;
using osu.Game.Screens.Play.PlayerSettings;
using OpenTK.Input;

namespace osu.Game.Screens.Play.HUD
{
    public class PlayerSettingsOverlay : VisibilityContainer
    {
        private const int fade_duration = 200;

        public bool ReplayLoaded;

        public readonly PlaybackSettings PlaybackSettings;
        public readonly VisualSettings VisualSettings;
        //public readonly CollectionSettings CollectionSettings;
        //public readonly DiscussionSettings DiscussionSettings;

        public PlayerSettingsOverlay()
        {
            AlwaysPresent = true;
            RelativeSizeAxes = Axes.Both;

            Child = new FillFlowContainer<PlayerSettingsGroup>
            {
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 20),
                Margin = new MarginPadding { Top = 100, Right = 10 },
                Children = new PlayerSettingsGroup[]
                {
                    //CollectionSettings = new CollectionSettings(),
                    //DiscussionSettings = new DiscussionSettings(),
                    PlaybackSettings = new PlaybackSettings(),
                    VisualSettings = new VisualSettings { Expanded = false }
                }
            };

            State = Visibility.Visible;
        }

        protected override void PopIn() => this.FadeIn(fade_duration);
        protected override void PopOut() => this.FadeOut(fade_duration);

        //We want to handle keyboard inputs all the time in order to trigger ToggleVisibility() when not visible
        public override bool HandleNonPositionalInput => true;

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (args.Repeat) return false;

            if (state.Keyboard.ControlPressed)
            {
                if (args.Key == Key.H && ReplayLoaded)
                {
                    ToggleVisibility();
                    return true;
                }
            }

            return base.OnKeyDown(state, args);
        }
    }
}
