// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Screens;

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// A <see cref="BreadcrumbControl{IScreen}"/> which follows the active screen (and allows navigation) in a <see cref="Screen"/> stack.
    /// </summary>
    public partial class ScreenBreadcrumbControl : BreadcrumbControl<IScreen>
    {
        public ScreenBreadcrumbControl(ScreenStack stack)
        {
            stack.ScreenPushed += onPushed;
            stack.ScreenExited += onExited;

            if (stack.CurrentScreen != null)
                onPushed(null, stack.CurrentScreen);
        }

        protected override bool UpdateTabSelection(TabItem<IScreen> tab)
        {
            // override base method to prevent current item from being changed on click.
            // depend on screen push/exit to change current item instead.
            var lastTab = SelectedTab;
            tab.Value.MakeCurrent();
            return tab != lastTab;
        }

        private void onPushed(IScreen lastScreen, IScreen newScreen)
        {
            AddItem(newScreen);
            Current.Value = newScreen;
        }

        private void onExited(IScreen lastScreen, IScreen newScreen)
        {
            if (newScreen != null)
                Current.Value = newScreen;

            Items.ToList().SkipWhile(s => s != Current.Value).Skip(1).ForEach(RemoveItem);
        }
    }
}
