﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Graphics.Cursor
{
    public class OsuContextMenuContainer : ContextMenuContainer
    {
        protected override ContextMenu<ContextMenuItem> CreateContextMenu() => new OsuContextMenu<ContextMenuItem>();

        public OsuContextMenuContainer(CursorContainer cursor) : base(cursor)
        {
        }
    }
}