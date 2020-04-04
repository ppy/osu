// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public class TestSceneNumberBox : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(OsuNumberBox),
        };

        private OsuNumberBox numberBox;

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.X,
                Padding = new MarginPadding { Horizontal = 250 },
                Child = numberBox = new OsuNumberBox
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.X,
                    PlaceholderText = "Insert numbers here"
                }
            };

            clearInput();
            AddStep("enter numbers", () => numberBox.Text = "987654321");
            expectedValue("987654321");
            clearInput();
            AddStep("enter text + single number", () => numberBox.Text = "1 hello 2 world 3");
            expectedValue("123");
            clearInput();
        }

        private void clearInput() => AddStep("clear input", () => numberBox.Text = null);

        private void expectedValue(string value) => AddAssert("expect number", () => numberBox.Text == value);
    }
}
