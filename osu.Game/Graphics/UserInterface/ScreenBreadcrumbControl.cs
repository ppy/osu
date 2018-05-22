// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Screens;

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// A <see cref="BreadcrumbControl"/> which follows the active screen (and allows navigation) in a <see cref="Screen"/> stack.
    /// </summary>
    public class ScreenBreadcrumbControl : BreadcrumbControl<Screen>
    {
        private Screen last;

        public ScreenBreadcrumbControl(Screen initialScreen)
        {
            Current.ValueChanged += newScreen =>
            {
                if (last != newScreen && !newScreen.IsCurrentScreen)
                    newScreen.MakeCurrent();
            };

            onPushed(initialScreen);
        }

        private void screenChanged(Screen newScreen)
        {
            if (newScreen == null) return;

            if (last != null)
            {
                last.Exited -= screenChanged;
                last.ModePushed -= onPushed;
            }

            last = newScreen;

            newScreen.Exited += screenChanged;
            newScreen.ModePushed += onPushed;

            Current.Value = newScreen;
        }

        private void onPushed(Screen screen)
        {
            Items.ToList().SkipWhile(i => i != Current.Value).Skip(1).ForEach(RemoveItem);
            AddItem(screen);

            screenChanged(screen);
        }
    }
}
