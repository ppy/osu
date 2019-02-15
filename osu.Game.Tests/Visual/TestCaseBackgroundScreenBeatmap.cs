// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework.Internal;
using osu.Framework.Allocation;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Rulesets;
using osu.Game.Screens;
using osu.Game.Screens.Backgrounds;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual
{
    public class TestCaseBackgroundScreenBeatmap : TestCasePlayer
    {

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager manager)
        {
            LoadScreen(new DimAccessiblePlayer());
        }

        [Test]
        public void EnableUserDimTest()
        {
            AddStep("Test User Dimming", () => ((DimAccessiblePlayer)Player).EnableScreenDim());
            AddAssert("Check screen dim", () => ((DimAccessiblePlayer)Player).AssertDimState());
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

            private class FadeAccessibleBackground : BackgroundScreenBeatmap
            {
                public bool AssertDimState()
                {
                    return FadeContainer.Colour == OsuColour.Gray(BackgroundOpacity);
                }
            }
        }
    }

    internal class SetupAttribute : Attribute
    {
    }
}
