// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Input;
using osu.Framework.Testing;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Mania.UI;
using System;
using System.Collections.Generic;
using osu.Game.Beatmaps.Timing;
using OpenTK;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Mania.Objects;

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
                Add(new ManiaPlayfield(cols, new List<ControlPoint>())
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
                Add(playField = new ManiaPlayfield(cols, new List<ControlPoint> { new ControlPoint { BeatLength = 200 } })
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
        }

        private void triggerKeyDown(Column column)
        {
            column.TriggerKeyDown(new InputState(), new KeyDownEventArgs
            {
                Key = column.Key,
                Repeat = false
            });
        }

        private void triggerKeyUp(Column column)
        {
            column.TriggerKeyUp(new InputState(), new KeyUpEventArgs
            {
                Key = column.Key
            });
        }
    }
}
