// Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using OpenTK;
using osu.Framework.GameModes.Testing;

namespace osu.Framework.VisualTests.Tests
{
    class TestCaseTextBox : TestCase
    {
        private TextBox tb;

        public override string Name => @"TextBox";

        public override string Description => @"Text entry evolved";

        public override int DisplayOrder => -1;

        public override void Reset()
        {
            base.Reset();

            FlowContainer textBoxes = new FlowContainer
            {
                Direction = FlowDirection.VerticalOnly,
                Padding = new Vector2(0, 50),
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                SizeMode = InheritMode.XY,
                Size = new Vector2(0.8f, 1)
            };

            Add(textBoxes);

            textBoxes.Add(tb = new TextBox
            {
                Size = new Vector2(100, 16),
            });

            textBoxes.Add(tb = new TextBox
            {
                Text = @"Limited length",
                Size = new Vector2(200, 20),
                LengthLimit = 20
            });

            textBoxes.Add(tb = new TextBox
            {
                Text = @"Box with some more text",
                Size = new Vector2(500, 30),
            });

            //textBoxes.Add(tb = new PasswordTextBox(@"", 14, Vector2.Zero, 300));
        }
    }
}
