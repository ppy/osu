// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Input;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;

namespace osu.Game.Screens.Play.ReplaySettings
{
    public class ReplaySettingsOverlay : FillFlowContainer
    {
        private const int fade_duration = 100;

        private bool isVisible;

        public ReplaySettingsOverlay()
        {
            AlwaysPresent = true;
            Direction = FillDirection.Vertical;
            AutoSizeAxes = Axes.Both;
            Spacing = new Vector2(0, 20);

            Add(new CollectionSettings());
            Add(new DiscussionSettings());
            Add(new PlaybackSettings());
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (args.Repeat) return false;

            switch (args.Key)
            {
                case Key.H:
                    FadeTo(isVisible ? 1 : 0, fade_duration);
                    isVisible = !isVisible;
                    return true;
            }

            return base.OnKeyDown(state, args);
        }
    }
}
