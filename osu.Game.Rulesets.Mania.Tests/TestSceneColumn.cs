// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Tests.Visual;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.Tests
{
    [TestFixture]
    public partial class TestSceneColumn : ManiaInputTestScene
    {
        [Cached(typeof(IReadOnlyList<Mod>))]
        private IReadOnlyList<Mod> mods { get; set; } = Array.Empty<Mod>();

        [Cached]
        private readonly StageDefinition stage = new StageDefinition(1);

        private readonly List<Column> columns = new List<Column>();

        public TestSceneColumn()
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
                    createColumn(ScrollingDirection.Up, ManiaAction.Key1, 0),
                    createColumn(ScrollingDirection.Down, ManiaAction.Key2, 1)
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

        private Drawable createColumn(ScrollingDirection direction, ManiaAction action, int index)
        {
            var column = new Column(index, false)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Height = 0.85f,
                AccentColour = { Value = Color4.OrangeRed },
                Action = { Value = action },
            };

            columns.Add(column);

            return new ScrollingTestContainer(direction)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.X,
                RelativeSizeAxes = Axes.Y,
                TimeRange = 2000,
                Child = column
            };
        }
    }
}
