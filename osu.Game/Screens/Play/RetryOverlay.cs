// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Input;
using OpenTK.Input;
using osu.Framework.Allocation;
using osu.Framework.Audio.Sample;
using osu.Framework.Audio;
using System;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Screens.Play
{
    public class RetryOverlay : Container
    {
        public Action Action;
        public Action OnKeyPressed;
        public Action OnKeyReleased;

        private SampleChannel retrySample;
        private bool keyIsHeld;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            retrySample = audio.Sample.Get(@"Menu/menuback");
            AlwaysPresent = true;
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (args.Repeat) return false;

            if (args.Key == Key.Tilde)
            {
                keyIsHeld = true;
                OnKeyPressed();

                Delay(500).Schedule(() =>
                {
                    if (keyIsHeld)
                    {
                        retrySample.Play();
                        Action();
                    }
                });
                return true;
            }

            return base.OnKeyDown(state, args);
        }

        protected override bool OnKeyUp(InputState state, KeyUpEventArgs args)
        {
            if (args.Key == Key.Tilde)
            {
                keyIsHeld = false;
                OnKeyReleased();
                return true;
            }

            return base.OnKeyUp(state, args);
        }
    }
}
