// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Graphics.Cursor
{
    public class OsuContextMenuContainer : ContextMenuContainer
    {
        protected override Menu CreateMenu() => new OsuContextMenu();
    }
}
