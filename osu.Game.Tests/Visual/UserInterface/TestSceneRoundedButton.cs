// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.UserInterfaceV2;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneRoundedButton : OsuTestScene
    {
        [Test]
        public void TestBasic()
        {
            RoundedButton button = null;

            AddStep("create button", () => Child = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Colour4.DarkGray
                    },
                    button = new RoundedButton
                    {
                        Width = 400,
                        Text = "Test button",
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Action = () => { }
                    }
                }
            });

            AddToggleStep("toggle disabled", disabled => button.Action = disabled ? (Action)null : () => { });
        }
    }
}
