using System;
using OpenTK.Graphics;
using OpenTK.Input;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Framework.Timing;

namespace osu.Game.Graphics.UserInterface
{
    class SkipButton : Button
    {
        private IAdjustableClock sourceClock;
        private double time;
        public SkipButton(IAdjustableClock clock, double time) 
        {
            Height = 60;
            Width = 100;
            Text = "skip";
            Colour = new Color4(238, 51, 153, 255);
            Action = skip;
            sourceClock = clock;
            this.time = time;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Delay(time - 3000, true);
            Content.FadeOut(250);
        }

        private void skip()
        {
            sourceClock.Seek(time - 3000);
            FadeOut(250);
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            switch (args.Key)
            {
                case Key.Space:
                    if(sourceClock.CurrentTime + 3000 < time)
                        skip();
                    return true;
            }

            return base.OnKeyDown(state, args);
        }
    }
}
