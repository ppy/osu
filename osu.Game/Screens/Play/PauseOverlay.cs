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
    public class PauseOverlay : InGameOverlay
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

        protected override void AddButtons(OsuColour colours)
        {
            AddButton(@"Continue", colours.Green, OnResume);
            AddButton(@"Retry", colours.YellowDark, OnRetry);
            AddButton(@"Quit to Main Menu", new Color4(170, 27, 39, 255), OnQuit);
        }

        public PauseOverlay()
        {
            Title = @"paused";
            Description = @"you're not going to do what i think you're going to do, are ya?";
        }
    }
}
        