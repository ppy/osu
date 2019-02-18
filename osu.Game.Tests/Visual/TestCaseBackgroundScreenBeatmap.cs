// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Graphics;
using osu.Game.Rulesets;
using osu.Game.Screens;
using osu.Game.Screens.Backgrounds;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseBackgroundScreenBeatmap : TestCasePlayer
    {
        /// <summary>
        /// Check if the fade container is properly being faded when screen dim is enabled.
        /// </summary>
        [Test]
        public void EnableUserDimTest()
        {
            AddStep("Test User Dimming", () => ((DimAccessiblePlayer)Player).EnableScreenDim());
            AddWaitStep(5, "Wait for dim");
            AddAssert("Check screen dim", () => ((DimAccessiblePlayer)Player).AssertDimState());
        }

        /// <summary>
        /// Check if the fade container is properly being reset when screen dim is disabled.
        /// </summary>
        [Test]
        public void DisableUserDimTest()
        {
            AddStep("Test User Dimming", () => ((DimAccessiblePlayer)Player).DisableScreenDim());
            AddWaitStep(5, "Wait for dim");
            AddAssert("Check screen dim", () => ((DimAccessiblePlayer)Player).AssertUndimmed());
        }

        protected override Player CreatePlayer(Ruleset ruleset) => new DimAccessiblePlayer();

        private class DimAccessiblePlayer : Player
        {
            protected override BackgroundScreen CreateBackground() => new FadeAccessibleBackground();

            public void EnableScreenDim()
            {
                Background.UpdateDim.Value = true;
            }

            public void DisableScreenDim()
            {
                Background.UpdateDim.Value = false;
            }

            public bool AssertDimState()
            {
                return ((FadeAccessibleBackground)Background).AssertDimState();
            }

            public bool AssertUndimmed()
            {
                return ((FadeAccessibleBackground)Background).AssertUndimmed();
            }

            private class FadeAccessibleBackground : BackgroundScreenBeatmap
            {
                public bool AssertDimState()
                {
                    return FadeContainer.Colour == OsuColour.Gray(BackgroundOpacity);
                }

                public bool AssertUndimmed()
                {
                    return FadeContainer.Colour == OsuColour.Gray(1.0f);
                }
            }
        }
    }
}
