// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Graphics.Cursor
{
    [Cached(typeof(OsuContextMenuContainer))]
    public partial class OsuContextMenuContainer : ContextMenuContainer
    {
        private OsuContextMenu menu = null!;

        protected override Menu CreateMenu() => menu = new OsuContextMenu(true);

        public void CloseMenu()
        {
            menu.Close();
        }
    }
}
