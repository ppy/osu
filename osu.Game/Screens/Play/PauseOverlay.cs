// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Input;
using osu.Game.Graphics;
using OpenTK.Input;
using osu.Framework.Graphics.Containers;
using OpenTK.Graphics;

namespace osu.Game.Screens.Play
{
    public class PauseOverlay : FailOverlay
    {
        public Action OnResume;

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (args.Key == Key.Escape)
            {
                if (State == Visibility.Hidden) return false;
                OnResume();
                return true;
            }

            return base.OnKeyDown(state, args);
        }

        public PauseOverlay()
        {
            AddButton(@"Continue", Color4.Green, OnResume);
        }
    }
}
        