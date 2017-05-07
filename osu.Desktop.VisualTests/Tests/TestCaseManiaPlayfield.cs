// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Input;
using osu.Framework.Testing;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Mania.UI;
using System.Linq;
using System;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCaseManiaPlayfield : TestCase
    {
        public override string Description => @"Mania playfield";

        protected override double TimePerAction => 200;

        public override void Reset()
        {
            base.Reset();

            const int max_columns = 10;

            Action<int, SpecialColumnPosition> createPlayfield = (cols, pos) =>
            {
                Clear();
                Add(new ManiaPlayfield(cols)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    SpecialColumnPosition = pos
                });
            };

            for (int i = 1; i <= max_columns; i++)
            {
                int tempI = i;

                AddStep($"{i} column" + (i > 1 ? "s" : ""), () => createPlayfield(tempI, SpecialColumnPosition.Normal));

                AddStep("Trigger keys down", () => ((ManiaPlayfield)Children.First()).Columns.Children.ForEach(triggerKeyDown));
                AddStep("Trigger keys up", () => ((ManiaPlayfield)Children.First()).Columns.Children.ForEach(triggerKeyUp));

                AddStep("Left special style", () => createPlayfield(tempI, SpecialColumnPosition.Left));
                AddStep("Right special style", () => createPlayfield(tempI, SpecialColumnPosition.Right));
            }

            AddStep("Normal special style", () => createPlayfield(max_columns, SpecialColumnPosition.Normal));
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
