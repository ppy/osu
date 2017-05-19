// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Input;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;

namespace osu.Game.Screens.Play.Settings
{
    public class ReplaySettingsOverlay : FillFlowContainer
    {
        private const int fade_duration = 100;

        private bool isVisible;
        public bool IsVisible
        {
            set
            {
                isVisible = value;

                if (isVisible)
                    FadeIn(fade_duration);
                else
                    FadeOut(fade_duration);
            }
            get { return isVisible; }
        }

        public ReplaySettingsOverlay()
        {
            AlwaysPresent = true;
            Direction = FillDirection.Vertical;
            Spacing = new Vector2(0, 20);

            Add(new CollectionSettings());
            Add(new DiscussionSettings());
            Add(new PlaybackSettings());
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (args.Repeat) return false;

            if (state.Keyboard.ControlPressed)
            {
                switch (args.Key)
                {
                    case Key.H:
                        IsVisible = !isVisible;
                        return true;
                }
            }

            return base.OnKeyDown(state, args);
        }
    }
}
