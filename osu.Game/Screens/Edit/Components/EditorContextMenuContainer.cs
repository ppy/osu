// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Graphics.Cursor;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Edit.Components
{
    public partial class EditorContextMenuContainer : OsuContextMenuContainer
    {
        public override bool ChangeFocusOnClick => true;

        private OsuContextMenu menu = null!;

        protected override Framework.Graphics.UserInterface.Menu CreateMenu() => menu = new OsuContextMenu(true);

        public void ShowMenu()
        {
            menu.Show();
        }

        public void CloseMenu()
        {
            menu.Close();
        }
    }
}
