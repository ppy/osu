// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Input;
using osu.Framework.Testing;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Mania.UI;
using System;
using System.Collections.Generic;
using OpenTK;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Timing;
using osu.Framework.Configuration;
using OpenTK.Input;
using osu.Framework.Timing;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCaseManiaPlayfield : TestCase
    {
        public override string Description => @"Mania playfield";

        protected override double TimePerAction => 200;

        public override void Reset()
        {
            base.Reset();

            Action<int, SpecialColumnPosition> createPlayfield = (cols, pos) =>
            {
                Clear();
                Add(new ManiaPlayfield(cols, new List<TimingChange>())
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    SpecialColumnPosition = pos,
                    Scale = new Vector2(1, -1)
                });
            };

            Action<int, SpecialColumnPosition> createPlayfieldWithNotes = (cols, pos) =>
            {
                Clear();

                ManiaPlayfield playField;
                Add(playField = new ManiaPlayfield(cols, new List<TimingChange> { new TimingChange { BeatLength = 200 } })
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    SpecialColumnPosition = pos,
                    Scale = new Vector2(1, -1)
                });

                for (int i = 0; i < cols; i++)
                {
                    playField.Add(new DrawableNote(new Note
                    {
                        StartTime = Time.Current + 1000,
                        Column = i
                    }));
                }
            };

            Action createPlayfieldWithNotesAcceptingInput = () =>
            {
                Clear();

                var rateAdjustClock = new StopwatchClock(true) { Rate = 0.5 };

                ManiaPlayfield playField;
                Add(playField = new ManiaPlayfield(4, new List<TimingChange> { new TimingChange { BeatLength = 200 } })
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Scale = new Vector2(1, -1),
                    Clock = new FramedClock(rateAdjustClock)
                });

                for (int t = 1000; t <= 2000; t += 100)
                {
                    playField.Add(new DrawableNote(new Note
                    {
                        StartTime = t,
                        Column = 0
                    }, new Bindable<Key>(Key.D)));

                    playField.Add(new DrawableNote(new Note
                    {
                        StartTime = t,
                        Column = 3
                    }, new Bindable<Key>(Key.K)));
                }

                playField.Add(new DrawableHoldNote(new HoldNote
                {
                    StartTime = 1000,
                    Duration = 1000,
                    Column = 1
                }, new Bindable<Key>(Key.F)));

                playField.Add(new DrawableHoldNote(new HoldNote
                {
                    StartTime = 1000,
                    Duration = 1000,
                    Column = 2
                }, new Bindable<Key>(Key.J)));
            };

            AddStep("1 column", () => createPlayfield(1, SpecialColumnPosition.Normal));
            AddStep("4 columns", () => createPlayfield(4, SpecialColumnPosition.Normal));
            AddStep("Left special style", () => createPlayfield(4, SpecialColumnPosition.Left));
            AddStep("Right special style", () => createPlayfield(4, SpecialColumnPosition.Right));
            AddStep("5 columns", () => createPlayfield(5, SpecialColumnPosition.Normal));
            AddStep("8 columns", () => createPlayfield(8, SpecialColumnPosition.Normal));
            AddStep("Left special style", () => createPlayfield(8, SpecialColumnPosition.Left));
            AddStep("Right special style", () => createPlayfield(8, SpecialColumnPosition.Right));

            AddStep("Normal special style", () => createPlayfield(4, SpecialColumnPosition.Normal));

            AddStep("Notes", () => createPlayfieldWithNotes(4, SpecialColumnPosition.Normal));
            AddWaitStep(10);
            AddStep("Left special style", () => createPlayfieldWithNotes(4, SpecialColumnPosition.Left));
            AddWaitStep(10);
            AddStep("Right special style", () => createPlayfieldWithNotes(4, SpecialColumnPosition.Right));
            AddWaitStep(10);

            AddStep("Notes with input", () => createPlayfieldWithNotesAcceptingInput());
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
    }
}
