// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Beatmaps.Timing;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Edit.Timing;

namespace osu.Game.Tests.Visual.Editing
{
    public class TestSceneLabelledTimeSignature : OsuManualInputManagerTestScene
    {
        private LabelledTimeSignature timeSignature;

        private void createLabelledTimeSignature(TimeSignature initial) => AddStep("create labelled time signature", () =>
        {
            Child = timeSignature = new LabelledTimeSignature
            {
                Label = "Time Signature",
                RelativeSizeAxes = Axes.None,
                Width = 400,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Current = { Value = initial }
            };
        });

        private OsuTextBox numeratorTextBox => timeSignature.ChildrenOfType<OsuTextBox>().Single();

        [Test]
        public void TestInitialValue()
        {
            createLabelledTimeSignature(TimeSignature.SimpleTriple);
            AddAssert("current is 3/4", () => timeSignature.Current.Value.Equals(TimeSignature.SimpleTriple));
        }

        [Test]
        public void TestChangeViaCurrent()
        {
            createLabelledTimeSignature(TimeSignature.SimpleQuadruple);
            AddAssert("current is 4/4", () => timeSignature.Current.Value.Equals(TimeSignature.SimpleQuadruple));

            AddStep("set current to 5/4", () => timeSignature.Current.Value = new TimeSignature(5));

            AddAssert("current is 5/4", () => timeSignature.Current.Value.Equals(new TimeSignature(5)));
            AddAssert("numerator is 5", () => numeratorTextBox.Current.Value == "5");

            AddStep("set current to 3/4", () => timeSignature.Current.Value = TimeSignature.SimpleTriple);

            AddAssert("current is 3/4", () => timeSignature.Current.Value.Equals(TimeSignature.SimpleTriple));
            AddAssert("numerator is 3", () => numeratorTextBox.Current.Value == "3");
        }

        [Test]
        public void TestChangeNumerator()
        {
            createLabelledTimeSignature(TimeSignature.SimpleQuadruple);
            AddAssert("current is 4/4", () => timeSignature.Current.Value.Equals(TimeSignature.SimpleQuadruple));

            AddStep("focus text box", () => InputManager.ChangeFocus(numeratorTextBox));

            AddStep("set numerator to 7", () => numeratorTextBox.Current.Value = "7");
            AddAssert("current is 4/4", () => timeSignature.Current.Value.Equals(TimeSignature.SimpleQuadruple));

            AddStep("drop focus", () => InputManager.ChangeFocus(null));
            AddAssert("current is 7/4", () => timeSignature.Current.Value.Equals(new TimeSignature(7)));
        }

        [Test]
        public void TestInvalidChangeRollbackOnCommit()
        {
            createLabelledTimeSignature(TimeSignature.SimpleQuadruple);
            AddAssert("current is 4/4", () => timeSignature.Current.Value.Equals(TimeSignature.SimpleQuadruple));

            AddStep("focus text box", () => InputManager.ChangeFocus(numeratorTextBox));

            AddStep("set numerator to 0", () => numeratorTextBox.Current.Value = "0");
            AddAssert("current is 4/4", () => timeSignature.Current.Value.Equals(TimeSignature.SimpleQuadruple));

            AddStep("drop focus", () => InputManager.ChangeFocus(null));
            AddAssert("current is 4/4", () => timeSignature.Current.Value.Equals(TimeSignature.SimpleQuadruple));
            AddAssert("numerator is 4", () => numeratorTextBox.Current.Value == "4");
        }
    }
}
