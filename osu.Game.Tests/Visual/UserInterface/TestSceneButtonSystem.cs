// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Shapes;
using osu.Game.Screens.Menu;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public partial class TestSceneButtonSystem : OsuManualInputManagerTestScene
    {
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

        [TestCase(Key.P, true)]
        [TestCase(Key.M, true)]
        [TestCase(Key.L, true)]
        [TestCase(Key.E, false)]
        [TestCase(Key.D, false)]
        [TestCase(Key.Q, false)]
        [TestCase(Key.O, false)]
        public void TestShortcutKeys(Key key, bool entersPlay)
        {
            int activationCount = -1;
            AddStep("set up action", () =>
            {
                activationCount = 0;
                void action() => activationCount++;

                switch (key)
                {
                    case Key.P:
                        buttons.OnSolo = action;
                        break;

                    case Key.M:
                        buttons.OnMultiplayer = action;
                        break;

                    case Key.L:
                        buttons.OnPlaylists = action;
                        break;

                    case Key.E:
                        buttons.OnEditBeatmap = action;
                        break;

                    case Key.D:
                        buttons.OnBeatmapListing = action;
                        break;

                    case Key.Q:
                        buttons.OnExit = action;
                        break;

                    case Key.O:
                        buttons.OnSettings = action;
                        break;
                }
            });

            AddStep($"press {key}", () => InputManager.Key(key));
            AddAssert("state is top level", () => buttons.State == ButtonSystemState.TopLevel);

            if (entersPlay)
            {
                AddStep("press P", () => InputManager.Key(Key.P));
                AddAssert("state is play", () => buttons.State == ButtonSystemState.Play);
            }

            AddStep($"press {key}", () => InputManager.Key(key));
            AddAssert("action triggered", () => activationCount == 1);
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
