// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Cursor;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Edit.Components
{
    public partial class EditorContextMenuContainer : OsuContextMenuContainer, IKeyBindingHandler<PlatformAction>
    {
        public override bool ChangeFocusOnClick => true;

        private OsuContextMenu menu = null!;

        protected override Framework.Graphics.UserInterface.Menu CreateMenu() => menu = new OsuContextMenu(true);

        public bool OnPressed(KeyBindingPressEvent<PlatformAction> e)
        {
            switch (e.Action)
            {
                case PlatformAction.Delete:
                    menu.Close();
                    break;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<PlatformAction> e)
        {
        }
    }
}
