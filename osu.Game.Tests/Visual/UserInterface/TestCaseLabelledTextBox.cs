// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Screens.Edit.Setup.Components.LabelledComponents;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public class TestCaseLabelledTextBox : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(LabelledTextBox),
        };

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.X,
                Padding = new MarginPadding { Left = 150, Right = 150 },
                Child = new LabelledTextBox
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    LabelText = "Testing text",
                    PlaceholderText = "This is definitely working as intended",
                }
            };
        }
    }
}
