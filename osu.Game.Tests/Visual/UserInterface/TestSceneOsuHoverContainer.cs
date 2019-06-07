// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public class TestSceneOsuHoverContainer : ManualInputManagerTestScene
    {
        private OsuHoverTestContainer hoverContainer;
        private OsuSpriteText textContainer;
        private ColourInfo currentColour => textContainer.DrawColourInfo.Colour;
        private ColourInfo idleColour => hoverContainer.IdleColourPublic;
        private ColourInfo hoverColour => hoverContainer.HoverColourPublic;

        public TestSceneOsuHoverContainer()
        {
            setupUI();
        }

        [SetUp]
        public void TestSceneOsuHoverContainer_SetUp() => Schedule(() => setupUI());

        private void setupUI()
        {
            Child = hoverContainer = new OsuHoverTestContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.Both,
                Child = new FillFlowContainer<SpriteText>
                {
                    AutoSizeAxes = Axes.Both,
                    Children = new[]
                    {
                        textContainer = new OsuSpriteText
                        {
                            Text = "Test",
                            Font = OsuFont.GetFont(weight: FontWeight.Medium, size: 20),
                        },
                    }
                }
            };
        }

        [Description("Checks IsHovered property value on a container when it is hovered/unhovered.")]
        [TestCase(true, TestName = "Enabled_Check_IsHovered")]
        [TestCase(false, TestName = "Disabled_Check_IsHovered")]
        public void Check_IsHovered_HasProperValue(bool isEnabled)
        {
            moveOut();
            setContainerEnabledTo(isEnabled);

            checkNotHovered();

            moveToText();
            checkHovered();

            moveOut();
            checkNotHovered();

            moveToText();
            checkHovered();

            moveOut();
            checkNotHovered();

            ReturnUserInput();
        }

        [Test]
        [Description("Checks colour fading on an enabled container when it is hovered/unhovered.")]
        public void WhenEnabled_Fades()
        {
            moveOut();
            enableContainer();

            checkColour(idleColour);

            moveToText();
            waitUntilColourIs(hoverColour);

            moveOut();
            waitUntilColourIs(idleColour);

            moveToText();
            waitUntilColourIs(hoverColour);

            moveOut();
            waitUntilColourIs(idleColour);

            ReturnUserInput();
        }

        [Test]
        [Description("Checks colour fading on a disabled container when it is hovered/unhovered.")]
        public void WhenDisabled_DoesNotFade()
        {
            moveOut();
            disableContainer();

            checkColour(idleColour);

            moveToText();
            checkColour(idleColour);

            moveOut();
            checkColour(idleColour);

            moveToText();
            checkColour(idleColour);

            moveOut();
            checkColour(idleColour);

            ReturnUserInput();
        }

        [Test]
        [Description("Checks that when a disabled & hovered container gets enabled, colour fading happens")]
        public void WhileHovering_WhenGetsEnabled_Fades()
        {
            moveOut();
            disableContainer();
            checkColour(idleColour);

            moveToText();
            checkColour(idleColour);

            enableContainer();
            waitUntilColourIs(hoverColour);
        }

        [Test]
        [Description("Checks that when an enabled & hovered container gets disabled, colour fading happens")]
        public void WhileHovering_WhenGetsDisabled_Fades()
        {
            moveOut();
            enableContainer();
            checkColour(idleColour);

            moveToText();
            waitUntilColourIs(hoverColour);

            disableContainer();
            waitUntilColourIs(idleColour);
        }

        [Test]
        [Description("Checks that when a hovered container gets enabled and disabled multiple times, colour fading happens")]
        public void WhileHovering_WhenEnabledChangesMultipleTimes_Fades()
        {
            moveOut();
            enableContainer();
            checkColour(idleColour);

            moveToText();
            waitUntilColourIs(hoverColour);

            disableContainer();
            waitUntilColourIs(idleColour);

            enableContainer();
            waitUntilColourIs(hoverColour);

            disableContainer();
            waitUntilColourIs(idleColour);
        }

        private void enableContainer() => setContainerEnabledTo(true);

        private void disableContainer() => setContainerEnabledTo(false);

        private void setContainerEnabledTo(bool newValue)
        {
            string word = newValue ? "Enable" : "Disable";
            AddStep($"{word} container", () => hoverContainer.Enabled.Value = newValue);
        }

        private void moveToText() => AddStep("Move mouse to text", () => InputManager.MoveMouseTo(hoverContainer));

        private void moveOut() => AddStep("Move out", doMoveOut);

        private void checkHovered() => AddAssert("Check hovered", () => hoverContainer.IsHovered);

        private void checkNotHovered() => AddAssert("Check not hovered", () => !hoverContainer.IsHovered);

        private void checkColour(ColourInfo expectedColour)
            => AddAssert($"Check colour to be '{expectedColour}'", () => currentColour.Equals(expectedColour));

        private void waitUntilColourIs(ColourInfo expectedColour)
            => AddUntilStep($"Wait until hover colour is {expectedColour}", () => currentColour.Equals(expectedColour));

        /// <summary>
        ///     Moves the cursor to top left corner of the screen
        /// </summary>
        private void doMoveOut()
            => InputManager.MoveMouseTo(new Vector2(InputManager.ScreenSpaceDrawQuad.TopLeft.X, InputManager.ScreenSpaceDrawQuad.TopLeft.Y));

        private sealed class OsuHoverTestContainer : OsuHoverContainer
        {
            public Color4 HoverColourPublic => HoverColour;
            public Color4 IdleColourPublic => IdleColour;
        }
    }
}
