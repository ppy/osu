// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneOsuAnimatedButton : OsuTestScene
    {
        [Test]
        public void TestRelativeSized()
        {
            AddStep("add button", () => Child = new BaseContainer("relative sized")
            {
                RelativeSizeAxes = Axes.Both,
                Action = () => { }
            });
        }

        [Test]
        public void TestAutoSized()
        {
            AddStep("add button", () => Child = new BaseContainer("auto sized")
            {
                AutoSizeAxes = Axes.Both,
                Action = () => { }
            });
        }

        [Test]
        public void TestRelativeYAutoX()
        {
            AddStep("add button", () => Child = new BaseContainer("relative Y auto X")
            {
                RelativeSizeAxes = Axes.Y,
                AutoSizeAxes = Axes.X,
                Action = () => { }
            });
        }

        [Test]
        public void TestRelativeXAutoY()
        {
            AddStep("add button", () => Child = new BaseContainer("relative X auto Y")
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Action = () => { }
            });
        }

        [Test]
        public void TestFixed1()
        {
            AddStep("add button", () => Child = new BaseContainer("fixed")
            {
                Size = new Vector2(100),
                Action = () => { }
            });
        }

        [Test]
        public void TestFixed2()
        {
            AddStep("add button", () => Child = new BaseContainer("fixed")
            {
                Size = new Vector2(100, 50),
                Action = () => { }
            });
        }

        [Test]
        public void TestToggleEnabled()
        {
            BaseContainer button = null;

            AddStep("add button", () => Child = button = new BaseContainer("fixed")
            {
                Size = new Vector2(200),
            });

            AddToggleStep("toggle enabled", toggle =>
            {
                for (int i = 0; i < 6; i++)
                    button.Action = toggle ? () => { } : (Action)null;
            });
        }

        [Test]
        public void TestInitiallyDisabled()
        {
            AddStep("add disabled button", () =>
            {
                Child = new BaseContainer("disabled")
                {
                    Size = new Vector2(100)
                };
            });
        }

        public class BaseContainer : OsuAnimatedButton
        {
            public BaseContainer(string text)
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                Add(new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Text = text
                });
            }
        }
    }
}
