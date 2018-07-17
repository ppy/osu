// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Configuration;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI.Scrolling;
using System;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Rulesets.Mania.UI
{
    public class ManiaPlayfield : ManiaScrollingPlayfield
    {
        public List<Column> Columns => stages.SelectMany(x => x.Columns).ToList();

        private readonly List<ManiaStage> stages = new List<ManiaStage>();
        public IReadOnlyList<ManiaStage> Stages => stages;

        protected virtual bool DisplayJudgements => true;

        public ManiaPlayfield(ScrollingDirection direction, List<StageDefinition> stageDefinitions)
            : base(direction)
        {
            if (stageDefinitions == null)
                throw new ArgumentNullException(nameof(stageDefinitions));

            if (stageDefinitions.Count <= 0)
                throw new ArgumentException("Can't have zero or fewer stages.");

            GridContainer playfieldGrid;
            InternalChild = playfieldGrid = new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                Content = new[] { new Drawable[stageDefinitions.Count] }
            };

            var normalColumnAction = ManiaAction.Key1;
            var specialColumnAction = ManiaAction.Special1;
            int firstColumnIndex = 0;
            for (int i = 0; i < stageDefinitions.Count; i++)
            {
                var newStage = CreateStage(direction, firstColumnIndex, stageDefinitions[i], ref normalColumnAction, ref specialColumnAction);
                newStage.DisplayJudgements = DisplayJudgements;
                newStage.VisibleTimeRange.BindTo(VisibleTimeRange);

                playfieldGrid.Content[0][i] = newStage;

                stages.Add(newStage);
                AddNested(newStage);

                firstColumnIndex += newStage.Columns.Count;
            }
        }

        public override void Add(DrawableHitObject h)
        {
            h.OnJudgement += OnJudgement;
            getStageByColumn(((ManiaHitObject)h.HitObject).Column).Add(h);
        }

        public void Add(BarLine barline) => stages.ForEach(s => s.Add(barline));

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

        [BackgroundDependencyLoader]
        private void load(ManiaConfigManager maniaConfig)
        {
            maniaConfig.BindWith(ManiaSetting.ScrollTime, VisibleTimeRange);
        }

        internal void OnJudgement(DrawableHitObject judgedObject, Judgement judgement)
        {
            if (!judgedObject.DisplayJudgement || !DisplayJudgements)
                return;

            getStageByColumn(((ManiaHitObject)judgedObject.HitObject).Column).OnJudgement(judgedObject, judgement);
        }

        protected virtual ManiaStage CreateStage(ScrollingDirection direction, int firstColumnIndex, StageDefinition definition, ref ManiaAction normalColumnStartAction, ref ManiaAction specialColumnStartAction)
            => new ManiaStage(direction, firstColumnIndex, definition, ref normalColumnStartAction, ref specialColumnStartAction);
    }
}
