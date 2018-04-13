// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Screens;
using osu.Game.Screens;

namespace osu.Game.Tests.Visual
{
    /// <summary>
    /// A test case which can be used to test a screen (that relies on OnEntering being called to execute startup instructions).
    /// </summary>
    public abstract class ScreenTestCase : OsuTestCase
    {
        private readonly TestOsuScreen baseScreen;

        protected ScreenTestCase()
        {
            Add(baseScreen = new TestOsuScreen());
        }

        protected void LoadScreen(OsuScreen screen) => baseScreen.LoadScreen(screen);

        public class TestOsuScreen : OsuScreen
        {
            private OsuScreen nextScreen;

            public void LoadScreen(OsuScreen screen) => Schedule(() =>
            {
                nextScreen = screen;

                if (IsCurrentScreen)
                {
                    Push(screen);
                    nextScreen = null;
                }
                else
                    MakeCurrent();
            });

            protected override void OnResuming(Screen last)
            {
                base.OnResuming(last);
                if (nextScreen != null)
                    LoadScreen(nextScreen);
            }
        }
    }
}
