// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Game.Graphics;
using OpenTK.Graphics;
using OpenTK.Input;

namespace osu.Game.Screens.Play
{
    public class FailOverlay : MenuOverlay
    {
        public override string Header => "failed";
        public override string Description => "you're dead, try again?";

        protected override bool IsExitKey(Key key) => key == Key.Escape;
        protected override Action ExitAction => () => Buttons.Children.Last().TriggerOnClick();

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            AddButton("Retry", colours.YellowDark, OnRetry);
            AddButton("Quit", new Color4(170, 27, 39, 255), OnQuit);
        }
    }
}
