// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using OpenTK.Input;

namespace osu.Game.Screens.Play
{
    public class SkipButton : TwoLayerButton
    {
        private readonly double skipDestination;

        public SkipButton()
        {
            Text = @"Skip";
            Icon = FontAwesome.fa_osu_right_o;
            Anchor = Anchor.BottomRight;
            Origin = Anchor.BottomRight;
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, OsuColour colours)
        {
            ActivationSound = audio.Sample.Get(@"Menu/menuhit");
            Colour = colours.Yellow;
            HoverColour = colours.YellowDark;
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (args.Repeat) return false;

            switch (args.Key)
            {
                case Key.Space:
                    TriggerClick();
                    return true;
            }

            return base.OnKeyDown(state, args);
        }
    }
}
