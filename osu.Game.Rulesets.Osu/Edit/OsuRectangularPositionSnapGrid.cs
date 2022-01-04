// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Input.Bindings;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit
{
    public class OsuRectangularPositionSnapGrid : RectangularPositionSnapGrid, IKeyBindingHandler<GlobalAction>
    {
        private static readonly int[] grid_sizes = { 4, 8, 16, 32 };

        private int currentGridSizeIndex = grid_sizes.Length - 1;

        [Resolved]
        private EditorBeatmap editorBeatmap { get; set; }

        public OsuRectangularPositionSnapGrid()
            : base(OsuPlayfield.BASE_SIZE / 2)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            int gridSizeIndex = Array.IndexOf(grid_sizes, editorBeatmap.BeatmapInfo.GridSize);
            if (gridSizeIndex >= 0)
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
            int gridSize = grid_sizes[currentGridSizeIndex];

            editorBeatmap.BeatmapInfo.GridSize = gridSize;
            Spacing = new Vector2(gridSize);
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
