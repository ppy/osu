// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Input;
using osu.Framework.Testing;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Mania.UI;
using System;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Mania.Objects;
using osu.Framework.Configuration;
using OpenTK.Input;
using osu.Framework.Timing;
using osu.Framework.Extensions.IEnumerableExtensions;
using System.Linq;
using osu.Game.Rulesets.Mania.Timing;
using osu.Game.Rulesets.Timing;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Mania.Judgements;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCaseManiaPlayfield : TestCase
    {
        public override string Description => @"Mania playfield";

        protected override double TimePerAction => 200;

        public override void Reset()
        {
            base.Reset();

            testBasicPlayfields();
            testNotes();
            testJudgements();

        }

        private void testBasicPlayfields()
        {
            Action<int, SpecialColumnPosition, bool> createPlayfield = (cols, pos, flipped) =>
            {
                Clear();

                ManiaPlayfield playfield;
                Add(playfield = new ManiaPlayfield(cols)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    SpecialColumnPosition = pos,
                    Clock = new FramedClock(new StopwatchClock(true))
                });

                playfield.Flipped.Value = flipped;
            };

            AddStep("1 column", () => createPlayfield(1, SpecialColumnPosition.Normal, false));
            AddStep("4 columns", () => createPlayfield(4, SpecialColumnPosition.Normal, false));
            AddStep("Left special style", () => createPlayfield(4, SpecialColumnPosition.Left, false));
            AddStep("Right special style", () => createPlayfield(4, SpecialColumnPosition.Right, false));
            AddStep("5 columns", () => createPlayfield(5, SpecialColumnPosition.Normal, false));
            AddStep("8 columns", () => createPlayfield(8, SpecialColumnPosition.Normal, false));
            AddStep("Left special style", () => createPlayfield(8, SpecialColumnPosition.Left, false));
            AddStep("Right special style", () => createPlayfield(8, SpecialColumnPosition.Right, false));
            AddStep("8 columns, right special, flipped", () => createPlayfield(8, SpecialColumnPosition.Right, true));
        }

        private void testNotes()
        {
            const double start_time = 500;
            const double duration = 1000;

            Func<double, bool, SpeedAdjustmentContainer> createTimingChange = (time, gravity) => new ManiaSpeedAdjustmentContainer(new MultiplierControlPoint(time)
            {
                TimingPoint = { BeatLength = 1000 }
            }, gravity ? ScrollingAlgorithm.Gravity : ScrollingAlgorithm.Basic);

            Action<bool> createPlayfieldWithNotes = gravity =>
            {
                Clear();

                ManiaPlayfield playfield;
                Add(playfield = new ManiaPlayfield(4)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Clock = new FramedClock(new StopwatchClock(true))
                });

                playfield.Flipped.Value = true;
                playfield.VisibleTimeRange.Value = 500;

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
                    }, new Bindable<Key>(Key.D)));

                    if (gravity)
                        playfield.Columns.ElementAt(3).Add(createTimingChange(t, true));

                    playfield.Add(new DrawableNote(new Note
                    {
                        StartTime = t,
                        Column = 3
                    }, new Bindable<Key>(Key.K)));
                }

                if (gravity)
                    playfield.Columns.ElementAt(1).Add(createTimingChange(start_time, true));

                playfield.Add(new DrawableHoldNote(new HoldNote
                {
                    StartTime = start_time,
                    Duration = duration,
                    Column = 1
                }, new Bindable<Key>(Key.F)));

                if (gravity)
                    playfield.Columns.ElementAt(2).Add(createTimingChange(start_time, true));

                playfield.Add(new DrawableHoldNote(new HoldNote
                {
                    StartTime = start_time,
                    Duration = duration,
                    Column = 2
                }, new Bindable<Key>(Key.J)));
            };

            AddStep("Basic scrolling notes", () => createPlayfieldWithNotes(false));
            AddWaitStep((int)Math.Ceiling((start_time + duration) / TimePerAction));

            AddStep("Gravity scrolling notes", () => createPlayfieldWithNotes(true));
            AddWaitStep((int)Math.Ceiling((start_time + duration) / TimePerAction));
        }

        private void testJudgements()
        {
            ManiaPlayfield playfield = null;
            AddStep("Create playfield", () =>
            {
                Add(playfield = new ManiaPlayfield(4)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Clock = new FramedClock(new StopwatchClock(true))
                });
            });

            Action<HitResult> addHitJudgement = h =>
            {
                playfield?.OnJudgement(new DrawableTestHit(new Note())
                {
                    Judgement = new ManiaJudgement
                    {
                        Result = h,
                        ManiaResult = ManiaHitResult.Perfect
                    }
                });
            };

            AddStep("Hit!", () => addHitJudgement(HitResult.Hit));
            AddStep("Miss :(", () => addHitJudgement(HitResult.Miss));
        }

        private void triggerKeyDown(Column column)
        {
            column.TriggerOnKeyDown(new InputState(), new KeyDownEventArgs
            {
                Key = column.Key,
                Repeat = false
            });
        }

        private void triggerKeyUp(Column column)
        {
            column.TriggerOnKeyUp(new InputState(), new KeyUpEventArgs
            {
                Key = column.Key
            });
        }

        private class DrawableTestHit : DrawableHitObject<ManiaHitObject, ManiaJudgement>
        {
            public DrawableTestHit(ManiaHitObject hitObject)
                : base(hitObject)
            {
            }

            protected override ManiaJudgement CreateJudgement() => new ManiaJudgement();

            protected override void UpdateState(ArmedState state)
            {
            }
        }
    }
}
