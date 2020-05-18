// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Framework.Timing;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Edit;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Screens.Edit;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Mania.Tests
{
    [Cached(typeof(IManiaHitObjectComposer))]
    public class TestSceneManiaBeatSnapGrid : EditorClockTestScene, IManiaHitObjectComposer
    {
        [Cached(typeof(IScrollingInfo))]
        private ScrollingTestContainer.TestScrollingInfo scrollingInfo = new ScrollingTestContainer.TestScrollingInfo();

        [Cached(typeof(EditorBeatmap))]
        private EditorBeatmap editorBeatmap = new EditorBeatmap(new ManiaBeatmap(new StageDefinition()));

        private readonly ManiaBeatSnapGrid beatSnapGrid;

        public TestSceneManiaBeatSnapGrid()
        {
            editorBeatmap.ControlPointInfo.Add(0, new TimingControlPoint { BeatLength = 200 });
            editorBeatmap.ControlPointInfo.Add(10000, new TimingControlPoint { BeatLength = 200 });

            BeatDivisor.Value = 3;

            // Some sane defaults
            scrollingInfo.Algorithm.Algorithm = ScrollVisualisationMethod.Constant;
            scrollingInfo.Direction.Value = ScrollingDirection.Up;
            scrollingInfo.TimeRange.Value = 1000;

            Children = new Drawable[]
            {
                Playfield = new ManiaPlayfield(new List<StageDefinition>
                {
                    new StageDefinition { Columns = 4 },
                    new StageDefinition { Columns = 3 }
                })
                {
                    Clock = new FramedClock(new StopwatchClock())
                },
                beatSnapGrid = new ManiaBeatSnapGrid()
            };
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            // We're providing a constant scroll algorithm.
            float relativePosition = Playfield.Stages[0].HitObjectContainer.ToLocalSpace(e.ScreenSpaceMousePosition).Y / Playfield.Stages[0].HitObjectContainer.DrawHeight;
            double timeValue = scrollingInfo.TimeRange.Value * relativePosition;

            beatSnapGrid.SelectionTimeRange = (timeValue, timeValue);

            return true;
        }

        public Column ColumnAt(Vector2 screenSpacePosition) => null;

        public ManiaPlayfield Playfield { get; }
    }
}
