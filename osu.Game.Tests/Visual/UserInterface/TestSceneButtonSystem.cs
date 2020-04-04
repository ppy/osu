// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Shapes;
using osu.Game.Screens.Menu;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public class TestSceneButtonSystem : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(ButtonSystem),
            typeof(ButtonArea),
            typeof(Button)
        };

        private OsuLogo logo;
        private ButtonSystem buttons;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            Children = new Drawable[]
            {
                new Box
                {
                    Colour = ColourInfo.GradientVertical(Color4.Gray, Color4.WhiteSmoke),
                    RelativeSizeAxes = Axes.Both,
                },
                buttons = new ButtonSystem(),
                logo = new OsuLogo
                {
                    RelativePositionAxes = Axes.Both,
                    Position = new Vector2(0.5f)
                }
            };

            buttons.SetOsuLogo(logo);
        });

        [Test]
        public void TestAllStates()
        {
            foreach (var s in Enum.GetValues(typeof(ButtonSystemState)).OfType<ButtonSystemState>().Skip(1))
                AddStep($"State to {s}", () => buttons.State = s);

            AddStep("Enter mode", performEnterMode);

            AddStep("Return to menu", () =>
            {
                buttons.State = ButtonSystemState.Play;
                buttons.FadeIn(MainMenu.FADE_IN_DURATION, Easing.OutQuint);
                buttons.MoveTo(new Vector2(0), MainMenu.FADE_IN_DURATION, Easing.OutQuint);
                logo.FadeColour(Color4.White, 100, Easing.OutQuint);
                logo.FadeIn(100, Easing.OutQuint);
            });
        }

        [Test]
        public void TestSmoothExit()
        {
            AddStep("Enter mode", performEnterMode);
        }

        private void performEnterMode()
        {
            buttons.State = ButtonSystemState.EnteringMode;
            buttons.FadeOut(MainMenu.FADE_OUT_DURATION, Easing.InSine);
            buttons.MoveTo(new Vector2(-800, 0), MainMenu.FADE_OUT_DURATION, Easing.InSine);
            logo.FadeOut(300, Easing.InSine)
                .ScaleTo(0.2f, 300, Easing.InSine);
        }
    }
}
