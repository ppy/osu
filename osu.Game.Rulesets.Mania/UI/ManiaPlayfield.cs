// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI.Scrolling;
using osuTK;

namespace osu.Game.Rulesets.Mania.UI
{
    public class ManiaPlayfield : ScrollingPlayfield
    {
        private readonly List<ManiaStage> stages = new List<ManiaStage>();

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => stages.Any(s => s.ReceivePositionalInputAt(screenSpacePos));

        public ManiaPlayfield(List<StageDefinition> stageDefinitions)
        {
            if (stageDefinitions == null)
                throw new ArgumentNullException(nameof(stageDefinitions));

            if (stageDefinitions.Count <= 0)
                throw new ArgumentException("Can't have zero or fewer stages.");

            GridContainer playfieldGrid;
            AddInternal(playfieldGrid = new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                Content = new[] { new Drawable[stageDefinitions.Count] }
            });

            var normalColumnAction = ManiaAction.Key1;
            var specialColumnAction = ManiaAction.Special1;
            int firstColumnIndex = 0;

            for (int i = 0; i < stageDefinitions.Count; i++)
            {
                var newStage = new ManiaStage(firstColumnIndex, stageDefinitions[i], ref normalColumnAction, ref specialColumnAction);

                playfieldGrid.Content[0][i] = newStage;

                stages.Add(newStage);
                AddNested(newStage);

                firstColumnIndex += newStage.Columns.Count;
            }
        }

        public override void Add(DrawableHitObject h) => getStageByColumn(((ManiaHitObject)h.HitObject).Column).Add(h);

        public override bool Remove(DrawableHitObject h) => getStageByColumn(((ManiaHitObject)h.HitObject).Column).Remove(h);

        public void Add(BarLine barline) => stages.ForEach(s => s.Add(barline));

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
                    if (column.ReceivePositionalInputAt(screenSpacePosition))
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
        /// Retrieves the total amount of columns across all stages in this playfield.
        /// </summary>
        public int TotalColumns => stages.Sum(s => s.Columns.Count);

        private ManiaStage getStageByColumn(int column)
        {
            int sum = 0;

            foreach (var stage in stages)
            {
                sum = sum + stage.Columns.Count;
                if (sum > column)
                    return stage;
            }

            return null;
        }
    }
}
