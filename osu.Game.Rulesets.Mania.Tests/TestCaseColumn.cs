// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Mania.UI.Components;
using osu.Game.Rulesets.UI.Scrolling;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Rulesets.Mania.Tests
{
    [TestFixture]
    public class TestCaseColumn : ManiaInputTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(Column),
            typeof(ColumnBackground),
            typeof(ColumnKeyArea),
            typeof(ColumnHitObjectArea)
        };

        private readonly List<Column> columns = new List<Column>();

        public TestCaseColumn()
            : base(2)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Spacing = new Vector2(20, 0),
                Children = new[]
                {
                    createColumn(ScrollingDirection.Up, ManiaAction.Key1),
                    createColumn(ScrollingDirection.Down, ManiaAction.Key2)
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AddStep("note", createNote);
            AddStep("hold note", createHoldNote);
        }

        private void createNote()
        {
            for (int i = 0; i < columns.Count; i++)
            {
                var obj = new Note { Column = i, StartTime = Time.Current + 2000 };
                obj.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

                columns[i].Add(new DrawableNote(obj));
            }
        }

        private void createHoldNote()
        {
            for (int i = 0; i < columns.Count; i++)
            {
                var obj = new HoldNote { Column = i, StartTime = Time.Current + 2000, Duration = 500 };
                obj.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

                columns[i].Add(new DrawableHoldNote(obj));
            }
        }

        private Drawable createColumn(ScrollingDirection direction, ManiaAction action)
        {
            var column = new Column(direction)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Height = 0.85f,
                AccentColour = Color4.OrangeRed,
                Action = { Value = action },
                VisibleTimeRange = { Value = 2000 }
            };

            columns.Add(column);

            return new ScrollingTestContainer(direction)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.X,
                RelativeSizeAxes = Axes.Y,
                Child = column
            };
        }
    }
}
