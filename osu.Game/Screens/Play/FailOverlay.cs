// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using OpenTK.Input;

namespace osu.Game.Screens.Play
{
    public class FailOverlay : PauseOverlay
    {
        public Action OnQuit;

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (args.Key == Key.Escape)
            {
                if (State == Visibility.Hidden) return false;
                quit();
                return true;
            }

            return base.OnKeyDown(state, args);
        }

        private void quit()
        {
            OnQuit?.Invoke();
            Hide();
        }

        public FailOverlay()
        {
            title.Text = @"failed";
            description.Text = @"you're dead, try again?";
        }
    }
}
