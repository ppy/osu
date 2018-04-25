// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Input;
using OpenTK.Input;
using osu.Game.Graphics;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using System.Linq;

namespace osu.Game.Screens.Play
{
    public class FailOverlay : GameplayMenuOverlay
    {
        public override string Header => "failed";
        public override string Description => "you're dead, try again?";

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            AddButton("Retry", colours.YellowDark, () => OnRetry?.Invoke());
            AddButton("Quit", new Color4(170, 27, 39, 255), () => OnQuit?.Invoke());
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (!args.Repeat && args.Key == Key.Escape)
            {
                InternalButtons.Children.Last().TriggerOnClick();
                return true;
            }

            return base.OnKeyDown(state, args);
        }
    }
}
