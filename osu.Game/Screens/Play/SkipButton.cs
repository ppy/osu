// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Timing;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using OpenTK.Input;

namespace osu.Game.Screens.Play
{
    public class SkipButton : TwoLayerButton
    {
        private readonly double startTime;
        public IAdjustableClock AudioClock;

        public SkipButton(double startTime)
        {
            this.startTime = startTime;
            Text = @"Skip";
            Icon = FontAwesome.fa_osu_right_o;
            Anchor = Anchor.BottomRight;
            Origin = Anchor.BottomRight;
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, OsuColour colours)
        {
            ActivationSound = audio.Sample.Get(@"Menu/menuhit");
            BackgroundColour = colours.Yellow;
            HoverColour = colours.YellowDark;

            const double skip_required_cutoff = 3000;
            const double fade_time = 300;

            if (startTime < skip_required_cutoff)
            {
                Alpha = 0;
                Expire();
                return;
            }

            FadeInFromZero(fade_time);

            Action = () => AudioClock.Seek(startTime - skip_required_cutoff - fade_time);

            Delay(startTime - skip_required_cutoff - fade_time);
            FadeOut(fade_time);
            Expire();
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
