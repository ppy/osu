// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.UI.Scrolling;
using OpenTK;

namespace osu.Game.Rulesets.Mania.Tests
{
    [TestFixture]
    public class TestCaseStage : ManiaInputTestCase
    {
        private const int columns = 4;

        private readonly List<ManiaStage> stages = new List<ManiaStage>();

        public TestCaseStage()
            : base(columns)
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
                    createStage(ScrollingDirection.Up, ManiaAction.Key1),
                    createStage(ScrollingDirection.Down, ManiaAction.Key3)
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AddStep("note", createNote);
            AddStep("hold note", createHoldNote);
            AddStep("minor bar line", () => createBarLine(false));
            AddStep("major bar line", () => createBarLine(true));
        }

        private void createNote()
        {
            foreach (var stage in stages)
            {
                for (int i = 0; i < stage.Columns.Count; i++)
                {
                    var obj = new Note { Column = i, StartTime = Time.Current + 2000 };
                    obj.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

                    stage.Add(new DrawableNote(obj));
                }
            }
        }

        private void createHoldNote()
        {
            foreach (var stage in stages)
            {
                for (int i = 0; i < stage.Columns.Count; i++)
                {
                    var obj = new HoldNote { Column = i, StartTime = Time.Current + 2000, Duration = 500 };
                    obj.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

                    stage.Add(new DrawableHoldNote(obj));
                }
            }
        }

        private void createBarLine(bool major)
        {
            foreach (var stage in stages)
            {
                var obj = new BarLine
                {
                    StartTime = Time.Current + 2000,
                    ControlPoint = new TimingControlPoint(),
                    BeatIndex = major ? 0 : 1
                };

                obj.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

                stage.Add(obj);
            }
        }

        private Drawable createStage(ScrollingDirection direction, ManiaAction action)
        {
            var specialAction = ManiaAction.Special1;

            var stage = new ManiaStage(direction, 0, new StageDefinition { Columns = 2 }, ref action, ref specialAction) { VisibleTimeRange = { Value = 2000 } };
            stages.Add(stage);

            return new ScrollingTestContainer(direction)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Y,
                AutoSizeAxes = Axes.X,
                Child = stage
            };
        }
    }
}
