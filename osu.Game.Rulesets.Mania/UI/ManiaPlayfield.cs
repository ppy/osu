// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Rulesets.Mania.Objects;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI.Scrolling;

namespace osu.Game.Rulesets.Mania.UI
{
    public class ManiaPlayfield : ScrollingPlayfield
    {
        /// <summary>
        /// list mania column stages
        /// </summary>
        private readonly FillFlowContainer<ManiaColumnStage> listColumnStages;

        /// <summary>
        /// Whether this playfield should be inverted. This flips everything inside the playfield.
        /// </summary>
        public readonly Bindable<bool> Inverted = new Bindable<bool>(true);

        /// <summary>
        /// The style to use for the special column.
        /// </summary>
        public SpecialColumnPosition SpecialColumnPosition
        {
            get => listColumnStages.FirstOrDefault()?.SpecialColumnPosition ?? SpecialColumnPosition.Normal;
            set
            {
                foreach (var singleStage in listColumnStages)
                {
                    singleStage.SpecialColumnPosition = value;
                }
            }
        }

        public List<Column> Columns => listColumnStages.SelectMany(x => x.Columns).ToList();

        private readonly int columnCount;

        public ManiaPlayfield(List<StageDefinition> stages)
            : base(ScrollingDirection.Up)
        {
            if (stages.Count <= 0)
                throw new ArgumentException("Can't have zero or fewer columns.");

            Inverted.Value = true;

            var stageSpacing = 300 / stages.Count;

            InternalChildren = new Drawable[]
            {
                listColumnStages = new FillFlowContainer<ManiaColumnStage>
                {
                    Name="Stages",
                    Direction = FillDirection.Horizontal,
                    RelativeSizeAxes = Axes.Y,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Spacing = new Vector2(stageSpacing),
                }
            };

            var currentAction = ManiaAction.Key1;

            foreach (var stage in stages)
            {
                var drawableStage = new ManiaColumnStage(stage.Columns);
                drawableStage.VisibleTimeRange.BindTo(VisibleTimeRange);

                listColumnStages.Add(drawableStage);
                AddNested(drawableStage);

                for (int i = 0; i < stage.Columns; i++)
                {
                    var c = new Column
                    {
                        //c.Action = c.IsSpecial ? ManiaAction.Special : currentAction++;
                        Action = currentAction++
                    };

                    drawableStage.AddColumn(c);
                    AddNested(c);
                }
            }

            Inverted.ValueChanged += invertedChanged;
            Inverted.TriggerChange();
        }

        private void invertedChanged(bool newValue)
        {
            Scale = new Vector2(1, newValue ? -1 : 1);

            foreach (var single in listColumnStages)
            {
                single.Judgements.Scale = Scale;
            }
        }

        public override void OnJudgement(DrawableHitObject judgedObject, Judgement judgement)
        {
            var maniaObject = (ManiaHitObject)judgedObject.HitObject;
            int column = maniaObject.Column;
            Columns[column].OnJudgement(judgedObject, judgement);

            getFallDownControlContainerByActualColumn(column).AddJudgement(judgement);
        }

        public override void Add(DrawableHitObject h) => Columns.ElementAt(((ManiaHitObject)h.HitObject).Column).Add(h);

        public void Add(BarLine barline)
        {
            foreach (var single in listColumnStages)
            {
                single.HitObjects.Add(new DrawableBarLine(barline));
            }
        }

        private ManiaColumnStage getFallDownControlContainerByActualColumn(int actualColumn)
        {
            int sum = 0;
            foreach (var single in listColumnStages)
            {
                sum = sum + single.ColumnCount;
                if (sum > actualColumn)
                {
                    return single;
                }
            }

            return null;
        }
    }
}
