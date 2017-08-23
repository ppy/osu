// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Timing;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Mania.Timing;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Timing;
using OpenTK;
using osu.Game.Rulesets;

namespace osu.Desktop.Tests.Visual
{
    internal class TestCaseManiaPlayfield : OsuTestCase
    {
        private const double start_time = 500;
        private const double duration = 500;

        public override string Description => @"Mania playfield";

        protected override double TimePerAction => 200;

        private RulesetInfo maniaRuleset;

        public TestCaseManiaPlayfield()
        {
            AddStep("1 column", () => createPlayfield(1, SpecialColumnPosition.Normal));
            AddStep("4 columns", () => createPlayfield(4, SpecialColumnPosition.Normal));
            AddStep("Left special style", () => createPlayfield(4, SpecialColumnPosition.Left));
            AddStep("Right special style", () => createPlayfield(4, SpecialColumnPosition.Right));
            AddStep("5 columns", () => createPlayfield(5, SpecialColumnPosition.Normal));
            AddStep("8 columns", () => createPlayfield(8, SpecialColumnPosition.Normal));
            AddStep("Left special style", () => createPlayfield(8, SpecialColumnPosition.Left));
            AddStep("Right special style", () => createPlayfield(8, SpecialColumnPosition.Right));

            AddStep("Notes with input", () => createPlayfieldWithNotes(false));
            AddWaitStep((int)Math.Ceiling((start_time + duration) / TimePerAction));

            AddStep("Notes with gravity", () => createPlayfieldWithNotes(true));
            AddWaitStep((int)Math.Ceiling((start_time + duration) / TimePerAction));
        }

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            maniaRuleset = rulesets.GetRuleset(3);
        }

        private SpeedAdjustmentContainer createTimingChange(double time, bool gravity) => new ManiaSpeedAdjustmentContainer(new MultiplierControlPoint(time)
        {
            TimingPoint = { BeatLength = 1000 }
        }, gravity ? ScrollingAlgorithm.Gravity : ScrollingAlgorithm.Basic);

        private void createPlayfield(int cols, SpecialColumnPosition specialPos)
        {
            Clear();

            var inputManager = new ManiaInputManager(maniaRuleset, cols) { RelativeSizeAxes = Axes.Both };
            Add(inputManager);

            inputManager.Add(new ManiaPlayfield(cols)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                SpecialColumnPosition = specialPos,
                Scale = new Vector2(1, -1)
            });
        }

        private void createPlayfieldWithNotes(bool gravity)
        {
            Clear();

            var rateAdjustClock = new StopwatchClock(true) { Rate = 1 };

            var inputManager = new ManiaInputManager(maniaRuleset, 4) { RelativeSizeAxes = Axes.Both };
            Add(inputManager);

            ManiaPlayfield playField;
            inputManager.Add(playField = new ManiaPlayfield(4)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Scale = new Vector2(1, -1),
                Clock = new FramedClock(rateAdjustClock)
            });

            if (!gravity)
                playField.Columns.ForEach(c => c.Add(createTimingChange(0, false)));

            for (double t = start_time; t <= start_time + duration; t += 100)
            {
                if (gravity)
                    playField.Columns.ElementAt(0).Add(createTimingChange(t, true));

                playField.Add(new DrawableNote(new Note
                {
                    StartTime = t,
                    Column = 0
                }, ManiaAction.Key1));

                if (gravity)
                    playField.Columns.ElementAt(3).Add(createTimingChange(t, true));

                playField.Add(new DrawableNote(new Note
                {
                    StartTime = t,
                    Column = 3
                }, ManiaAction.Key4));
            }

            if (gravity)
                playField.Columns.ElementAt(1).Add(createTimingChange(start_time, true));

            playField.Add(new DrawableHoldNote(new HoldNote
            {
                StartTime = start_time,
                Duration = duration,
                Column = 1
            }, ManiaAction.Key2));

            if (gravity)
                playField.Columns.ElementAt(2).Add(createTimingChange(start_time, true));

            playField.Add(new DrawableHoldNote(new HoldNote
            {
                StartTime = start_time,
                Duration = duration,
                Column = 2
            }, ManiaAction.Key3));
        }
    }
}
