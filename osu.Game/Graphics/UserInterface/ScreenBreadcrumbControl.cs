// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Screens;

namespace osu.Game.Graphics.UserInterface
{
    public class ScreenBreadcrumbControl : ScreenBreadcrumbControl<Screen>
    {
    }

    public class ScreenBreadcrumbControl<T> : BreadcrumbControl<T> where T : Screen
    {
        private T currentScreen;
        public T CurrentScreen
        {
            get { return currentScreen; }
            set
            {
                if (value == currentScreen) return;

                if (CurrentScreen != null)
                {
                    CurrentScreen.Exited -= onExited;
                    CurrentScreen.ModePushed -= onPushed;
                }
                else
                {
                    // this is the first screen in the stack, so call the initial onPushed
                    currentScreen = value;
                    onPushed(CurrentScreen);
                }

                currentScreen = value;

                if (CurrentScreen != null)
                {
                    CurrentScreen.Exited += onExited;
                    CurrentScreen.ModePushed += onPushed;
                    Current.Value = CurrentScreen;
                    OnScreenChanged?.Invoke(CurrentScreen);
                }
            }
        }

        public event Action<T> OnScreenChanged;

        public ScreenBreadcrumbControl()
        {
            Current.ValueChanged += s =>
            {
                if (s != CurrentScreen)
                {
                    CurrentScreen = s;
                    s.MakeCurrent();
                }
            };
        }

        private void onExited(Screen screen)
        {
            CurrentScreen = screen as T;
        }

        private void onPushed(Screen screen)
        {
            var newScreen = screen as T;

            Items.ToList().SkipWhile(i => i != Current.Value).Skip(1).ForEach(RemoveItem);
            AddItem(newScreen);

            CurrentScreen = newScreen;
        }
    }
}
