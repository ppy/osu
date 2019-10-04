// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
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
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Mania.Tests
{
    [TestFixture]
    public class TestSceneStage : ManiaInputTestScene
    {
        private const int columns = 4;

        [Cached(typeof(IReadOnlyList<Mod>))]
        private IReadOnlyList<Mod> mods { get; set; } = Array.Empty<Mod>();

        private readonly List<ManiaStage> stages = new List<ManiaStage>();

        private FillFlowContainer<ScrollingTestContainer> fill;

        public TestSceneStage()
            : base(columns)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = fill = new FillFlowContainer<ScrollingTestContainer>
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

            AddAssert("check note anchors", () => notesInStageAreAnchored(stages[0], Anchor.TopCentre));
            AddAssert("check note anchors", () => notesInStageAreAnchored(stages[1], Anchor.BottomCentre));
            AddAssert("check bar anchors", () => barsInStageAreAnchored(stages[0], Anchor.TopCentre));
            AddAssert("check bar anchors", () => barsInStageAreAnchored(stages[1], Anchor.BottomCentre));

            AddStep("flip direction", () =>
            {
                foreach (var c in fill.Children)
                    c.Flip();
            });

            AddAssert("check note anchors", () => notesInStageAreAnchored(stages[0], Anchor.BottomCentre));
            AddAssert("check note anchors", () => notesInStageAreAnchored(stages[1], Anchor.TopCentre));
            AddAssert("check bar anchors", () => barsInStageAreAnchored(stages[0], Anchor.BottomCentre));
            AddAssert("check bar anchors", () => barsInStageAreAnchored(stages[1], Anchor.TopCentre));
        }

        private bool notesInStageAreAnchored(ManiaStage stage, Anchor anchor) => stage.Columns.SelectMany(c => c.AllHitObjects).All(o => o.Anchor == anchor);

        private bool barsInStageAreAnchored(ManiaStage stage, Anchor anchor) => stage.AllHitObjects.Where(obj => obj is DrawableBarLine).All(o => o.Anchor == anchor);

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
                    Major = major,
                };

                obj.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

                stage.Add(obj);
            }
        }

        private ScrollingTestContainer createStage(ScrollingDirection direction, ManiaAction action)
        {
            var specialAction = ManiaAction.Special1;

            var stage = new ManiaStage(0, new StageDefinition { Columns = 2 }, ref action, ref specialAction);
            stages.Add(stage);

            return new ScrollingTestContainer(direction)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Y,
                AutoSizeAxes = Axes.X,
                TimeRange = 2000,
                Child = stage
            };
        }
    }
}
