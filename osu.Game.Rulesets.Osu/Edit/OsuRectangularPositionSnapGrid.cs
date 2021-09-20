// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Input.Bindings;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit
{
    public class OsuRectangularPositionSnapGrid : RectangularPositionSnapGrid, IKeyBindingHandler<GlobalAction>
    {
        private static readonly int[] grid_sizes = { 4, 8, 16, 32 };

        private int currentGridSizeIndex;

        public OsuRectangularPositionSnapGrid(int gridSize)
            : base(OsuPlayfield.BASE_SIZE / 2)
        {
            var gridSizeIndex = Array.IndexOf(grid_sizes, gridSize);
            if (gridSizeIndex > 0)
                currentGridSizeIndex = gridSizeIndex;
            updateSpacing();
        }

        private void nextGridSize()
        {
            currentGridSizeIndex = (currentGridSizeIndex + 1) % grid_sizes.Length;
            updateSpacing();
        }

        private void updateSpacing()
        {
            Spacing = new Vector2(grid_sizes[currentGridSizeIndex]);
        }

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            switch (e.Action)
            {
                case GlobalAction.EditorCycleGridDisplayMode:
                    nextGridSize();
                    return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }
    }
}
