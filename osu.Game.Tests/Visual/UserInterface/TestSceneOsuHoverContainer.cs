// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Containers;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public partial class TestSceneOsuHoverContainer : OsuManualInputManagerTestScene
    {
        private OsuHoverTestContainer hoverContainer;
        private Box colourContainer;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            Child = hoverContainer = new OsuHoverTestContainer
            {
                Enabled = { Value = true },
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(100),
                Child = colourContainer = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                },
            };

            doMoveOut();
        });

        [Description("Checks IsHovered property value on a container when it is hovered/unhovered.")]
        [TestCase(true, TestName = "Enabled_Check_IsHovered")]
        [TestCase(false, TestName = "Disabled_Check_IsHovered")]
        public void TestIsHoveredHasProperValue(bool isEnabled)
        {
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
        }

        [Test]
        [Description("Checks colour fading on an enabled container when it is hovered/unhovered.")]
        public void TestTransitionWhileEnabled()
        {
            enableContainer();

            checkColour(OsuHoverTestContainer.IDLE_COLOUR);

            moveToText();
            waitUntilColourIs(OsuHoverTestContainer.HOVER_COLOUR);

            moveOut();
            waitUntilColourIs(OsuHoverTestContainer.IDLE_COLOUR);

            moveToText();
            waitUntilColourIs(OsuHoverTestContainer.HOVER_COLOUR);

            moveOut();
            waitUntilColourIs(OsuHoverTestContainer.IDLE_COLOUR);
        }

        [Test]
        [Description("Checks colour fading on a disabled container when it is hovered/unhovered.")]
        public void TestNoTransitionWhileDisabled()
        {
            disableContainer();

            checkColour(OsuHoverTestContainer.IDLE_COLOUR);

            moveToText();
            checkColour(OsuHoverTestContainer.IDLE_COLOUR);

            moveOut();
            checkColour(OsuHoverTestContainer.IDLE_COLOUR);

            moveToText();
            checkColour(OsuHoverTestContainer.IDLE_COLOUR);

            moveOut();
            checkColour(OsuHoverTestContainer.IDLE_COLOUR);
        }

        [Test]
        [Description("Checks that when a disabled & hovered container gets enabled, colour fading happens")]
        public void TestBecomesEnabledTransition()
        {
            disableContainer();
            checkColour(OsuHoverTestContainer.IDLE_COLOUR);

            moveToText();
            checkColour(OsuHoverTestContainer.IDLE_COLOUR);

            enableContainer();
            waitUntilColourIs(OsuHoverTestContainer.HOVER_COLOUR);
        }

        [Test]
        [Description("Checks that when an enabled & hovered container gets disabled, colour fading happens")]
        public void TestBecomesDisabledTransition()
        {
            enableContainer();
            checkColour(OsuHoverTestContainer.IDLE_COLOUR);

            moveToText();
            waitUntilColourIs(OsuHoverTestContainer.HOVER_COLOUR);

            disableContainer();
            waitUntilColourIs(OsuHoverTestContainer.IDLE_COLOUR);
        }

        [Test]
        [Description("Checks that when a hovered container gets enabled and disabled multiple times, colour fading happens")]
        public void TestDisabledChangesMultipleTimes()
        {
            enableContainer();
            checkColour(OsuHoverTestContainer.IDLE_COLOUR);

            moveToText();
            waitUntilColourIs(OsuHoverTestContainer.HOVER_COLOUR);

            disableContainer();
            waitUntilColourIs(OsuHoverTestContainer.IDLE_COLOUR);

            enableContainer();
            waitUntilColourIs(OsuHoverTestContainer.HOVER_COLOUR);

            disableContainer();
            waitUntilColourIs(OsuHoverTestContainer.IDLE_COLOUR);
        }

        [Test]
        public void TestIdleColourChange()
        {
            enableContainer();
            checkColour(OsuHoverTestContainer.IDLE_COLOUR);

            changeIdleColour(Color4.White);
            waitUntilColourIs(Color4.White);

            moveToText();
            waitUntilColourIs(OsuHoverTestContainer.HOVER_COLOUR);

            changeIdleColour(Color4.Black);
            checkColour(OsuHoverTestContainer.HOVER_COLOUR);

            disableContainer();
            waitUntilColourIs(Color4.Black);

            changeIdleColour(Color4.Blue);
            waitUntilColourIs(Color4.Blue);

            moveOut();
            checkColour(Color4.Blue);
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

        private void changeIdleColour(ColourInfo idleColour)
            => AddStep($"Change idle colour to {idleColour}", () => hoverContainer.IdleColour = idleColour);

        private ColourInfo currentColour => colourContainer.DrawColourInfo.Colour;

        /// <summary>
        ///     Moves the cursor to top left corner of the screen
        /// </summary>
        private void doMoveOut()
            => InputManager.MoveMouseTo(new Vector2(InputManager.ScreenSpaceDrawQuad.TopLeft.X, InputManager.ScreenSpaceDrawQuad.TopLeft.Y));

        private sealed partial class OsuHoverTestContainer : OsuHoverContainer
        {
            public static readonly Color4 HOVER_COLOUR = Color4.Red;
            public static readonly Color4 IDLE_COLOUR = Color4.Green;

            public OsuHoverTestContainer()
            {
                HoverColour = HOVER_COLOUR;
                IdleColour = IDLE_COLOUR;
            }
        }
    }
}
