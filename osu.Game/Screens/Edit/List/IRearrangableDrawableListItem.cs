// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Edit.List
{
    public interface IRearrangableDrawableListItem<T> : IDrawableListItem<T>, IStateful<SelectionState>
    {
        event Action Selected;
        event Action Deselected;
    }
}
