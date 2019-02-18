// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Configuration;
using osu.Game.Graphics;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Scoring;
using osu.Game.Screens;
using osu.Game.Screens.Backgrounds;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseBackgroundScreenBeatmap : TestCasePlayer
    {
        public TestCaseBackgroundScreenBeatmap() : base(new OsuRuleset())
        {
        }

        [SetUp]
        public void Setup()
        {
            ((DimAccessiblePlayer)Player).UpdateBindables();
        }

        /// <summary>
        /// Check if the fade container is properly being faded when screen dim is enabled.
        /// </summary>
        [Test]
        public void EnableUserDimTest()
        {
            AddStep("Test User Dimming", () => ((DimAccessiblePlayer)Player).DimEnabled.Value = true);
            AddWaitStep(5, "Wait for dim");
            AddAssert("Check screen dim", () => ((DimAccessiblePlayer)Player).AssertDimState());
        }

        /// <summary>
        /// Check if the fade container is properly being reset when screen dim is disabled.
        /// </summary>
        [Test]
        public void DisableUserDimTest()
        {
            AddStep("Test User Undimming", () => ((DimAccessiblePlayer)Player).DimEnabled.Value = false);
            AddWaitStep(5, "Wait for dim");
            AddAssert("Check screen dim", () => ((DimAccessiblePlayer)Player).AssertUndimmed());
        }

        protected override Player CreatePlayer(Ruleset ruleset) => new DimAccessiblePlayer();

        private class DimAccessiblePlayer : Player
        {
            public Bindable<bool> DimEnabled;

            protected override BackgroundScreen CreateBackground() => new FadeAccessibleBackground();

            public void UpdateBindables()
            {
                DimEnabled = Background.UpdateDim;
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
                    return FadeContainer.Colour == OsuColour.Gray(1 - (float)DimLevel);
                }

                public bool AssertUndimmed()
                {
                    return FadeContainer.Colour == OsuColour.Gray(1.0f);
                }
            }
        }
    }
}
