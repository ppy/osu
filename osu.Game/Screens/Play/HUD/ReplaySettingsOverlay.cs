// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Screens.Play.ReplaySettings;
using OpenTK;
using osu.Framework.Input;
using OpenTK.Input;

namespace osu.Game.Screens.Play.HUD
{
    public class ReplaySettingsOverlay : VisibilityContainer
    {
        private const int fade_duration = 200;

        public bool ReplayLoaded;

        public readonly PlaybackSettings PlaybackSettings;
        //public readonly CollectionSettings CollectionSettings;
        //public readonly DiscussionSettings DiscussionSettings;

        public ReplaySettingsOverlay()
        {
            AlwaysPresent = true;
            RelativeSizeAxes = Axes.Both;

            Child = new FillFlowContainer<ReplayGroup>
            {
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 20),
                Margin = new MarginPadding { Top = 100, Right = 10 },
                Children = new[]
                {
                    //CollectionSettings = new CollectionSettings(),
                    //DiscussionSettings = new DiscussionSettings(),
                    PlaybackSettings = new PlaybackSettings(),
                }
            };

            State = Visibility.Visible;
        }

        protected override void PopIn() => this.FadeIn(fade_duration);
        protected override void PopOut() => this.FadeOut(fade_duration);

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
