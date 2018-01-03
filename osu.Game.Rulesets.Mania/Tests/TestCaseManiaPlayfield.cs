// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Timing;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Judgements;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Mania.Timing;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Timing;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Mania.Tests
{
    [TestFixture]
    [Ignore("getting CI working")]
    public class TestCaseManiaPlayfield : OsuTestCase
    {
        private const double start_time = 500;
        private const double duration = 500;

        protected override double TimePerAction => 200;

        private RulesetInfo maniaRuleset;

        public TestCaseManiaPlayfield()
        {
            var rng = new Random(1337);

            AddStep("1 column", () => createPlayfield(1, SpecialColumnPosition.Normal));
            AddStep("4 columns", () => createPlayfield(4, SpecialColumnPosition.Normal));
            AddStep("Left special style", () => createPlayfield(4, SpecialColumnPosition.Left));
            AddStep("Right special style", () => createPlayfield(4, SpecialColumnPosition.Right));
            AddStep("5 columns", () => createPlayfield(5, SpecialColumnPosition.Normal));
            AddStep("8 columns", () => createPlayfield(8, SpecialColumnPosition.Normal));
            AddStep("4 + 4 columns", () => 
            {
                var stages = new List<StageDefinition>()
                {
                    new StageDefinition() { Columns = 4 },
                    new StageDefinition() { Columns = 4 },
                };
                createPlayfield(stages, SpecialColumnPosition.Normal);
            });
            AddStep("2 + 4 + 2 columns", () =>
            {
                var stages = new List<StageDefinition>()
                {
                    new StageDefinition() { Columns = 2 },
                    new StageDefinition() { Columns = 4 },
                    new StageDefinition() { Columns = 2 },
                };
                createPlayfield(stages, SpecialColumnPosition.Normal);
            });
            AddStep("1 + 1 + 8 columns", () =>
            {
                var stages = new List<StageDefinition>()
                {
                    new StageDefinition() { Columns = 1 },
                    new StageDefinition() { Columns = 8 },
                    new StageDefinition() { Columns = 1 },
                };
                createPlayfield(stages, SpecialColumnPosition.Normal);
            });

            AddStep("Left special style", () => createPlayfield(8, SpecialColumnPosition.Left));
            AddStep("Right special style", () => createPlayfield(8, SpecialColumnPosition.Right));
            AddStep("Reversed", () => createPlayfield(4, SpecialColumnPosition.Normal, true));

            AddStep("Notes with input", () => createPlayfieldWithNotes(false));
            AddStep("Notes with input (reversed)", () => createPlayfieldWithNotes(false, true));
            AddStep("Notes with gravity", () => createPlayfieldWithNotes(true));
            AddStep("Notes with gravity (reversed)", () => createPlayfieldWithNotes(true, true));

            AddStep("Hit explosion", () =>
            {
                var playfield = createPlayfield(4, SpecialColumnPosition.Normal);

                int col = rng.Next(0, 4);

                var note = new DrawableNote(new Note { Column = col }, ManiaAction.Key1)
                {
                    AccentColour = playfield.Columns.ElementAt(col).AccentColour
                };

                playfield.OnJudgement(note, new ManiaJudgement { Result = HitResult.Perfect });
            });
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

        private ManiaPlayfield createPlayfield(int cols, SpecialColumnPosition specialPos, bool inverted = false)
        {
            var stages = new List<StageDefinition>()
            {
                new StageDefinition() { Columns = cols },
            };
            return createPlayfield(stages, specialPos, inverted);
        }

        private ManiaPlayfield createPlayfield(List<StageDefinition> stages, SpecialColumnPosition specialPos, bool inverted = false)
        {
            Clear();

            var inputManager = new ManiaInputManager(maniaRuleset, stages.Sum(g => g.Columns)) { RelativeSizeAxes = Axes.Both };
            Add(inputManager);

            ManiaPlayfield playfield;

            inputManager.Add(playfield = new ManiaPlayfield(stages)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                SpecialColumnPosition = specialPos
            });

            playfield.Inverted.Value = inverted;

            return playfield;
        }

        private void createPlayfieldWithNotes(bool gravity, bool inverted = false)
        {
            Clear();

            var rateAdjustClock = new StopwatchClock(true) { Rate = 1 };

            var inputManager = new ManiaInputManager(maniaRuleset, 4) { RelativeSizeAxes = Axes.Both };
            Add(inputManager);

            ManiaPlayfield playfield;
            var stages = new List<StageDefinition>()
            {
                new StageDefinition() { Columns = 4 },
            };
            inputManager.Add(playfield = new ManiaPlayfield(stages)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Clock = new FramedClock(rateAdjustClock)
            });

            playfield.Inverted.Value = inverted;

            if (!gravity)
                playfield.Columns.ForEach(c => c.Add(createTimingChange(0, false)));

            for (double t = start_time; t <= start_time + duration; t += 100)
            {
                if (gravity)
                    playfield.Columns.ElementAt(0).Add(createTimingChange(t, true));

                playfield.Add(new DrawableNote(new Note
                {
                    StartTime = t,
                    Column = 0
                }, ManiaAction.Key1));

                if (gravity)
                    playfield.Columns.ElementAt(3).Add(createTimingChange(t, true));

                playfield.Add(new DrawableNote(new Note
                {
                    StartTime = t,
                    Column = 3
                }, ManiaAction.Key4));
            }

            if (gravity)
                playfield.Columns.ElementAt(1).Add(createTimingChange(start_time, true));

            playfield.Add(new DrawableHoldNote(new HoldNote
            {
                StartTime = start_time,
                Duration = duration,
                Column = 1
            }, ManiaAction.Key2));

            if (gravity)
                playfield.Columns.ElementAt(2).Add(createTimingChange(start_time, true));

            playfield.Add(new DrawableHoldNote(new HoldNote
            {
                StartTime = start_time,
                Duration = duration,
                Column = 2
            }, ManiaAction.Key3));
        }
    }
}
