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
    public class ScreenBreadcrumbControl : BreadcrumbControl<IScreen>
    {
        public ScreenBreadcrumbControl(ScreenStack stack)
        {
            stack.ScreenPushed += onPushed;
            stack.ScreenExited += onExited;

            onPushed(null, stack.CurrentScreen);

            Current.ValueChanged += newScreen => newScreen.MakeCurrent();
        }

        private void onPushed(IScreen lastScreen, IScreen newScreen)
        {
            AddItem(newScreen);
            Current.Value = newScreen;
        }

        private void onExited(IScreen lastScreen, IScreen newScreen)
        {
            Current.Value = newScreen;
            Items.ToList().SkipWhile(s => s != Current.Value).Skip(1).ForEach(RemoveItem);
        }
    }
}
