// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Input;
using OpenTK.Input;
using osu.Game.Graphics;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using System.Linq;

namespace osu.Game.Screens.Play
{
    public class FailOverlay : MenuOverlay
    {
        public override string Header => "failed";
        public override string Description => "you're dead, try again?";

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            AddButton("Retry", colours.YellowDark, OnRetry);
            AddButton("Quit", new Color4(170, 27, 39, 255), OnQuit);
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (!args.Repeat && args.Key == Key.Escape)
            {
                Buttons.Children.Last().TriggerClick();
                return true;
            }

            return base.OnKeyDown(state, args);
        }
    }
}
