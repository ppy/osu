// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Graphics.Cursor
{
    public partial class OsuContextMenuContainer : ContextMenuContainer
    {
        [Cached]
        private OsuContextMenuSamples samples = new OsuContextMenuSamples();

        private OsuContextMenu menu = null!;

        protected override Menu CreateMenu() => menu = new OsuContextMenu(true);

        public OsuContextMenuContainer()
        {
            AddInternal(samples);
        }

        public void CloseMenu()
        {
            menu.Close();
        }
    }
}
