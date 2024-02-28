// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Spectate
{
    /// <summary>
    /// A grid of players playing the multiplayer match.
    /// </summary>
    public partial class PlayerGrid : CompositeDrawable
    {
        public const float ANIMATION_DELAY = 400;

        /// <summary>
        /// A temporary limitation on the number of players, because only layouts up to 16 players are supported for a single screen.
        /// Todo: Can be removed in the future with scrolling support + performance improvements.
        /// </summary>
        public const int MAX_PLAYERS = 16;

        private const float player_spacing = 6;

        /// <summary>
        /// The currently-maximised facade.
        /// </summary>
        public Facade MaximisedFacade { get; }

        private readonly Container paddingContainer;
        private readonly FillFlowContainer<Facade> facadeContainer;
        private readonly Container<Cell> cellContainer;

        public PlayerGrid()
        {
            InternalChildren = new Drawable[]
            {
                paddingContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(player_spacing),
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Child = facadeContainer = new FillFlowContainer<Facade>
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Spacing = new Vector2(player_spacing),
                            }
                        },
                        MaximisedFacade = new Facade
                        {
                            RelativeSizeAxes = Axes.Both,
                            Size = new Vector2(0.8f),
                        }
                    }
                },
                cellContainer = new Container<Cell> { RelativeSizeAxes = Axes.Both }
            };
        }

        /// <summary>
        /// Adds a new cell with content to this grid.
        /// </summary>
        /// <param name="content">The content the cell should contain.</param>
        /// <exception cref="InvalidOperationException">If more than <see cref="MAX_PLAYERS"/> cells are added.</exception>
        public void Add(Drawable content)
        {
            if (cellContainer.Count == MAX_PLAYERS)
                throw new InvalidOperationException($"Only {MAX_PLAYERS} cells are supported.");

            int index = cellContainer.Count;

            var facade = new Facade();
            facadeContainer.Add(facade);

            var cell = new Cell(index, content, facade) { ToggleMaximisationState = toggleMaximisationState };

            cellContainer.Add(cell);
        }

        /// <summary>
        /// The content added to this grid.
        /// </summary>
        public IEnumerable<Drawable> Content => cellContainer.OrderBy(c => c.FacadeIndex).Select(c => c.Content);

        // A depth value that gets decremented every time a new instance is maximised in order to reduce underlaps.
        private float maximisedInstanceDepth;

        private void toggleMaximisationState(Cell target)
        {
            // in the case the target is the already maximised cell (or there is only one cell), no cell should be maximised.
            bool hasMaximised = !target.IsMaximised && cellContainer.Count > 1;

            // Iterate through all cells to ensure only one is maximised at any time.
            foreach (var cell in cellContainer.ToList())
            {
                if (hasMaximised && cell == target)
                {
                    // Transfer cell to the maximised facade.
                    cell.SetFacade(MaximisedFacade, true);
                    cellContainer.ChangeChildDepth(cell, maximisedInstanceDepth -= 0.001f);
                }
                else
                {
                    // Transfer cell back to its original facade.
                    cell.SetFacade(facadeContainer[cell.FacadeIndex], false);
                }

                cell.FadeColour(hasMaximised && cell != target ? Color4.Gray : Color4.White, ANIMATION_DELAY, Easing.OutQuint);
            }

            facadeContainer.ScaleTo(hasMaximised ? 0.95f : 1, ANIMATION_DELAY, Easing.OutQuint);
        }

        protected override void Update()
        {
            base.Update();

            // Different layouts are used for varying cell counts in order to maximise dimensions.
            Vector2 cellsPerDimension;

            switch (facadeContainer.Count)
            {
                case 1:
                    cellsPerDimension = Vector2.One;
                    break;

                case 2:
                    cellsPerDimension = new Vector2(2, 1);
                    break;

                case 3:
                case 4:
                    cellsPerDimension = new Vector2(2);
                    break;

                case 5:
                case 6:
                    cellsPerDimension = new Vector2(3, 2);
                    break;

                case 7:
                case 8:
                case 9:
                    // 3 rows / 3 cols.
                    cellsPerDimension = new Vector2(3);
                    break;

                case 10:
                case 11:
                case 12:
                    // 3 rows / 4 cols.
                    cellsPerDimension = new Vector2(4, 3);
                    break;

                default:
                    // 4 rows / 4 cols.
                    cellsPerDimension = new Vector2(4);
                    break;
            }

            // Total inter-cell spacing.
            Vector2 totalCellSpacing = player_spacing * (cellsPerDimension - Vector2.One);

            Vector2 fullSize = paddingContainer.ChildSize - totalCellSpacing;
            Vector2 cellSize = Vector2.Divide(fullSize, new Vector2(cellsPerDimension.X, cellsPerDimension.Y));

            foreach (var cell in facadeContainer)
                cell.Size = cellSize;
        }

        /// <summary>
        /// A facade of the grid which is used as a dummy object to store the required position/size of cells.
        /// </summary>
        public partial class Facade : Drawable
        {
            public Facade()
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
            }
        }
    }
}
