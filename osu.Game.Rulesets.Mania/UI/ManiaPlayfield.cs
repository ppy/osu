// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;
using OpenTK;

namespace osu.Game.Rulesets.Mania.UI
{
    public class ManiaPlayfield : ScrollingPlayfield
    {
        /// <summary>
        /// list mania column group
        /// </summary>
        private readonly FillFlowContainer<ManiaColumnGroup> listColumnGroup;

        /// <summary>
        /// Whether this playfield should be inverted. This flips everything inside the playfield.
        /// </summary>
        public readonly Bindable<bool> Inverted = new Bindable<bool>(true);

        /// <summary>
        /// The style to use for the special column.
        /// </summary>
        public SpecialColumnPosition SpecialColumnPosition
        {
            get => listColumnGroup.FirstOrDefault()?.SpecialColumnPosition ?? SpecialColumnPosition.Normal;
            set
            {
                foreach (var singleGroup in listColumnGroup)
                {
                    singleGroup.SpecialColumnPosition = value;
                }
            }
        }

        public List<Column> Columns
        {
            get
            {
                var list = new List<Column>();
                foreach (var single in listColumnGroup)
                {
                    list.AddRange(single.Columns);
                }
                return list;
            }
        }

        public ManiaPlayfield(int columnCount, bool coop)
            : base(Axes.Y)
        {
            if (columnCount <= 0)
                throw new ArgumentException("Can't have zero or fewer columns.");

            Inverted.Value = true;

            InternalChildren = new Drawable[]
            {
                listColumnGroup = new FillFlowContainer<ManiaColumnGroup>
                {
                    Direction = FillDirection.Horizontal,
                    RelativeSizeAxes = Axes.Y,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Spacing = new Vector2(400),
                }
            };

            int numberOfGroup = 1;
            if (coop)
                numberOfGroup = 2;

            for (int i = 0; i < numberOfGroup; i++)
            {
                var group = new ManiaColumnGroup(columnCount / numberOfGroup);
                listColumnGroup.Add(group);
            }


            foreach (var single in listColumnGroup)
            {
                single.VisibleTimeRange.BindTo(VisibleTimeRange);
                AddNested(single);
            }

            var currentAction = ManiaAction.Key1;
            for (int i = 0; i < columnCount; i++)
            {
                var c = new Column
                {
                    //c.Action = c.IsSpecial ? ManiaAction.Special : currentAction++;
                    Action = currentAction++
                };

                /*
                c.IsSpecial = isSpecialColumn(i);
                topLevelContainer.Add(c.TopLevelContainer.CreateProxy());
                columns.Add(c);
                */
                getFallDownControlContainerByActualColumn(i).AddColumn(c);
                AddNested(c);
            }

            Inverted.ValueChanged += invertedChanged;
            Inverted.TriggerChange();
        }

        private void invertedChanged(bool newValue)
        {
            Scale = new Vector2(1, newValue ? -1 : 1);

            //judgements.Scale = Scale;
            foreach (var single in listColumnGroup)
            {
                single.Judgements.Scale = Scale;
            }
        }

        public override void OnJudgement(DrawableHitObject judgedObject, Judgement judgement)
        {
            var maniaObject = (ManiaHitObject)judgedObject.HitObject;
            int column = maniaObject.Column;
            Columns[maniaObject.Column].OnJudgement(judgedObject, judgement);

            getFallDownControlContainerByActualColumn(column).AddJudgement(judgement);
        }

        public override void Add(DrawableHitObject h) => Columns.ElementAt(((ManiaHitObject)h.HitObject).Column).Add(h);

        public void Add(BarLine barline)
        {
            //HitObjects.Add(new DrawableBarLine(barline));
            foreach (var single in listColumnGroup)
            {
                single.HitObjects.Add(new DrawableBarLine(barline));
            }
        }

        private ManiaColumnGroup getFallDownControlContainerByActualColumn(int actualColumn)
        {
            int sum = 0;
            foreach (var single in listColumnGroup)
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
