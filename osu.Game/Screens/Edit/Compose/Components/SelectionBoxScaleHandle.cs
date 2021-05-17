// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components
{
    public class SelectionBoxScaleHandle : SelectionBoxDragHandle
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            Size = new Vector2(10);
        }
    }
}
