// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Tests.Visual
{
    /// <summary>
    /// An abstract test case which exposes small cells arranged in a grid.
    /// Useful for displaying multiple configurations of a tested component at a glance.
    /// </summary>
    public abstract class OsuGridTestScene : OsuTestScene
    {
        private readonly Drawable[,] cells;

        /// <summary>
        /// The amount of rows in the grid.
        /// </summary>
        protected readonly int Rows;

        /// <summary>
        /// The amount of columns in the grid.
        /// </summary>
        protected readonly int Cols;

        /// <summary>
        /// Constructs a grid test case with the given dimensions.
        /// </summary>
        protected OsuGridTestScene(int rows, int cols)
        {
            Rows = rows;
            Cols = cols;

            GridContainer testContainer;
            Add(testContainer = new GridContainer { RelativeSizeAxes = Axes.Both });

            cells = new Drawable[rows, cols];
            for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                cells[r, c] = new Container { RelativeSizeAxes = Axes.Both };

            testContainer.Content = cells.ToJagged();
        }

        protected Container Cell(int index) => (Container)cells[index / Cols, index % Cols];
        protected Container Cell(int row, int col) => (Container)cells[row, col];
    }
}
