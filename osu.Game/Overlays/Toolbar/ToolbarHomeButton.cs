// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Bindings;
using osu.Game.Input.Bindings;

namespace osu.Game.Overlays.Toolbar
{
    public class ToolbarHomeButton : ToolbarButton, IKeyBindingHandler<GlobalAction>
    {
        public ToolbarHomeButton()
        {
            Icon = FontAwesome.Solid.Home;
            TooltipMain = "Home";
            TooltipSub = "Return to the main menu";
        }

        public bool OnPressed(GlobalAction action)
        {
            if (action == GlobalAction.Home)
            {
                Click();
                return true;
            }

            return false;
        }

        public void OnReleased(GlobalAction action)
        {
        }
    }
}
