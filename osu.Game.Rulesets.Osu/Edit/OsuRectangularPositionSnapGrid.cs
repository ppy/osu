// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Edit
{
    public class OsuRectangularPositionSnapGrid : RectangularPositionSnapGrid
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

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (e.Key == Key.G)
            {
                nextGridSize();
                return true;
            }

            return false;
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
    }
}
