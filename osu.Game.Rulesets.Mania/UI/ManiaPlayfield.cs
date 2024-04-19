// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Primitives;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Mania.UI
{
    [Cached]
    public partial class ManiaPlayfield : ScrollingPlayfield
    {
        public IReadOnlyList<Stage> Stages => stages;
        private readonly List<Stage> stages = new List<Stage>();

        public override Quad SkinnableComponentScreenSpaceDrawQuad
        {
            get
            {
                RectangleF totalArea = RectangleF.Empty;

                for (int i = 0; i < Stages.Count; ++i)
                {
                    var stageArea = Stages[i].ScreenSpaceDrawQuad.AABBFloat;
                    totalArea = i == 0 ? stageArea : RectangleF.Union(totalArea, stageArea);
                }

                return totalArea;
            }
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos)
        {
            foreach (var s in stages)
            {
                if (s.ReceivePositionalInputAt(screenSpacePos))
                    return true;
            }

            return false;
        }

        public ManiaPlayfield(List<StageDefinition> stageDefinitions)
        {
            ArgumentNullException.ThrowIfNull(stageDefinitions);

            if (stageDefinitions.Count <= 0)
                throw new ArgumentException("Can't have zero or fewer stages.");

            PlayfieldGrid playfieldGrid;

            AddInternal(new SkinnableDrawable(new ManiaSkinComponentLookup(ManiaSkinComponents.PlayfieldGrid), playfieldGrid = new PlayfieldGrid
            {
                RelativeSizeAxes = Axes.Y,
                AutoSizeAxes = Axes.X,
                Content = new[] { new Drawable[stageDefinitions.Count] },
                ColumnDimensions = Enumerable.Range(0, stageDefinitions.Count).Select(_ => new Dimension(GridSizeMode.AutoSize)).ToArray()
            })
            {
                RelativeSizeAxes = Axes.Both,
                CentreComponent = false
            });

            var normalColumnAction = ManiaAction.Key1;
            var specialColumnAction = ManiaAction.Special1;
            int firstColumnIndex = 0;

            for (int i = 0; i < stageDefinitions.Count; i++)
            {
                var newStage = new Stage(firstColumnIndex, stageDefinitions[i], ref normalColumnAction, ref specialColumnAction);

                playfieldGrid.Content[0][i] = newStage;

                stages.Add(newStage);
                AddNested(newStage);

                firstColumnIndex += newStage.Columns.Length;
            }
        }

        public override void Add(HitObject hitObject) => getStageByColumn(((ManiaHitObject)hitObject).Column).Add(hitObject);

        public override bool Remove(HitObject hitObject) => getStageByColumn(((ManiaHitObject)hitObject).Column).Remove(hitObject);

        public override void Add(DrawableHitObject h) => getStageByColumn(((ManiaHitObject)h.HitObject).Column).Add(h);

        public override bool Remove(DrawableHitObject h) => getStageByColumn(((ManiaHitObject)h.HitObject).Column).Remove(h);

        public void Add(BarLine barLine) => stages.ForEach(s => s.Add(barLine));

        /// <summary>
        /// Retrieves a column from a screen-space position.
        /// </summary>
        /// <param name="screenSpacePosition">The screen-space position.</param>
        /// <returns>The column which the <paramref name="screenSpacePosition"/> lies in.</returns>
        public Column GetColumnByPosition(Vector2 screenSpacePosition)
        {
            Column found = null;

            foreach (var stage in stages)
            {
                foreach (var column in stage.Columns)
                {
                    if (column.ReceivePositionalInputAt(new Vector2(screenSpacePosition.X, column.ScreenSpaceDrawQuad.Centre.Y)))
                    {
                        found = column;
                        break;
                    }
                }

                if (found != null)
                    break;
            }

            return found;
        }

        /// <summary>
        /// Retrieves a <see cref="Column"/> by index.
        /// </summary>
        /// <param name="index">The index of the column.</param>
        /// <returns>The <see cref="Column"/> corresponding to the given index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="index"/> is less than 0 or greater than <see cref="TotalColumns"/>.</exception>
        public Column GetColumn(int index)
        {
            if (index < 0 || index > TotalColumns - 1)
                throw new ArgumentOutOfRangeException(nameof(index));

            foreach (var stage in stages)
            {
                if (index >= stage.Columns.Length)
                {
                    index -= stage.Columns.Length;
                    continue;
                }

                return stage.Columns[index];
            }

            throw new ArgumentOutOfRangeException(nameof(index));
        }

        /// <summary>
        /// Retrieves the total amount of columns across all stages in this playfield.
        /// </summary>
        public int TotalColumns
        {
            get
            {
                int sum = 0;

                foreach (var stage in stages)
                    sum += stage.Columns.Length;

                return sum;
            }
        }

        private Stage getStageByColumn(int column)
        {
            int sum = 0;

            foreach (var stage in stages)
            {
                sum += stage.Columns.Length;
                if (sum > column)
                    return stage;
            }

            return null;
        }
    }
}
