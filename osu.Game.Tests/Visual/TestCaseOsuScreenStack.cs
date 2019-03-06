// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Screens;
using osu.Game.Screens;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual
{
    public class TestCaseOsuScreenStack : ScreenTestCase
    {
        private TestOsuScreen baseScreen;
        private TestScreenWithBeatmapBackground newScreen;

        [SetUp]
        public void SetUp()
        {
            baseScreen?.MakeCurrent();
            baseScreen?.Exit();
        }

        /// <summary>
        /// Make sure that OsuScreen dependencies are returned immediately after Exit is called.
        /// </summary>
        [Test]
        public void DependencyReturnTest()
        {
            AddStep("Add base screen", () => LoadScreen(baseScreen = new TestOsuScreen()));
            AddUntilStep(() => baseScreen.IsLoaded, "Wait until screen loaded");
            AddStep("Add new screen", () => baseScreen.Push(newScreen = new TestScreenWithBeatmapBackground()));
            AddUntilStep(() => newScreen.IsLoaded, "Wait until screen loaded");
            AddStep("Exit new screen", () => newScreen.Exit());
            AddUntilStep(() => baseScreen.IsCurrentScreen(), "Wait until base is current");
            AddAssert("Bindables have been returned by new screen", () => baseScreen.IsBindablesReturned);
        }

        private class TestOsuScreen : OsuScreen
        {
            public bool IsBindablesReturned;

            public override bool DisallowExternalBeatmapRulesetChanges => false;

            public override void OnResuming(IScreen last)
            {
                // Check if the last screen's bindables have since been returned
                if (!((TestScreenWithBeatmapBackground)last).Beatmap.Disabled && !((TestScreenWithBeatmapBackground)last).Ruleset.Disabled)
                    IsBindablesReturned = true;

                base.OnResuming(last);
            }
        }

        private class TestScreenWithBeatmapBackground : ScreenWithBeatmapBackground
        {
            public override bool DisallowExternalBeatmapRulesetChanges => true;
        }
    }
}
