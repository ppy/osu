// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Rulesets.Mania.Objects;
using OpenTK;
using osu.Framework.Graphics.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Configuration;
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
        /// <see cref="ManiaStage"/>s contained by this <see cref="ManiaPlayfield"/>.
        /// </summary>
        private readonly FillFlowContainer<ManiaStage> stages;

        /// <summary>
        /// Whether this playfield should be inverted. This flips everything inside the playfield.
        /// </summary>
        public readonly Bindable<bool> Inverted = new Bindable<bool>(true);

        /// <summary>
        /// The style to use for the special column.
        /// </summary>
        public SpecialColumnPosition SpecialColumnPosition
        {
            get => stages.FirstOrDefault()?.SpecialColumnPosition ?? SpecialColumnPosition.Normal;
            set
            {
                foreach (var singleStage in stages)
                {
                    singleStage.SpecialColumnPosition = value;
                }
            }
        }

        public List<Column> Columns => stages.SelectMany(x => x.Columns).ToList();

        private readonly int columnCount;

        public ManiaPlayfield(List<StageDefinition> stageDefinition)
            : base(ScrollingDirection.Up)
        {
            if (stageDefinition ==null)
                throw new ArgumentNullException();

            if (stageDefinition.Count <= 0)
                throw new ArgumentException("Can't have zero or fewer columns.");

            Inverted.Value = true;

            var stageSpacing = 300 / stageDefinition.Count;

            InternalChildren = new Drawable[]
            {
                this.stages = new FillFlowContainer<ManiaStage>
                {
                    Name = "Stages",
                    Direction = FillDirection.Horizontal,
                    RelativeSizeAxes = Axes.Y,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Spacing = new Vector2(stageSpacing),
                }
            };

            var currentAction = ManiaAction.Key1;

            int stageIndex = 0;
            foreach (var stage in stageDefinition)
            {
                var drawableStage = new ManiaStage();
                drawableStage.VisibleTimeRange.BindTo(VisibleTimeRange);
                drawableStage.Inverted.BindTo(Inverted);
                drawableStage.ColumnStartIndex = stageIndex;

                stages.Add(drawableStage);
                AddNested(drawableStage);

                for (int i = 0; i < stage.Columns; i++)
                {
                    var c = new Column
                    {
                        //c.Action = c.IsSpecial ? ManiaAction.Special : currentAction++;
                        Action = currentAction++
                    };

                    drawableStage.AddColumn(c);
                }

                stageIndex = stageIndex + stage.Columns;
            }
        }

        public override void OnJudgement(DrawableHitObject judgedObject, Judgement judgement)
        {
            var maniaObject = (ManiaHitObject)judgedObject.HitObject;
            int column = maniaObject.Column;
            getStageByColumn(column).AddJudgement(judgedObject,judgement);
        }

        public override void Add(DrawableHitObject h)
        {
            // => Columns.ElementAt(((ManiaHitObject)h.HitObject).Column).Add(h)
            int column = ((ManiaHitObject)h.HitObject).Column;
            var stage = getStageByColumn(column);
            stage.Add(h);
        }

        public void Add(BarLine barline)
        {
            foreach (var single in stages)
            {
                single.HitObjects.Add(new DrawableBarLine(barline));
            }
        }

        private ManiaStage getStageByColumn(int column)
        {
            int sum = 0;
            foreach (var stage in stages)
            {
                sum = sum + stage.Columns.Count();
                if (sum > column)
                {
                    return stage;
                }
            }

            return null;
        }
    }
}
