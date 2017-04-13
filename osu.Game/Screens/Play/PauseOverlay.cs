// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using osu.Framework.Input;
using osu.Game.Graphics;
using OpenTK.Input;
using OpenTK.Graphics;
using osu.Framework.Allocation;

namespace osu.Game.Screens.Play
{
    public class PauseOverlay : MenuOverlay
    {
        public Action OnResume;

        public override string Header => "paused";
        public override string Description => "you're not going to do what i think you're going to do, are ya?";

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (!args.Repeat && args.Key == Key.Escape)
            {
                Buttons.Children.First().TriggerClick();
                return true;
            }

            return base.OnKeyDown(state, args);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            AddButton("Continue", colours.Green, OnResume);
            AddButton("Retry", colours.YellowDark, OnRetry);
            AddButton("Quit", new Color4(170, 27, 39, 255), OnQuit);
        }
    }
}
