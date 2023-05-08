// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Utility;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Settings
{
    public partial class TestSceneLatencyCertifierScreen : ScreenTestScene
    {
        private LatencyCertifierScreen latencyCertifier = null!;

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("Load screen", () => LoadScreen(latencyCertifier = new LatencyCertifierScreen()));
            AddUntilStep("wait for load", () => latencyCertifier.IsLoaded);
        }

        [Test]
        public void TestSimple()
        {
            AddStep("set visual mode to simple", () => latencyCertifier.VisualMode.Value = LatencyVisualMode.Simple);
        }

        [Test]
        public void TestCircleGameplay()
        {
            AddStep("set visual mode to circles", () => latencyCertifier.VisualMode.Value = LatencyVisualMode.CircleGameplay);
        }

        [Test]
        public void TestScrollingGameplay()
        {
            AddStep("set visual mode to scrolling", () => latencyCertifier.VisualMode.Value = LatencyVisualMode.ScrollingGameplay);
        }

        [Test]
        public void TestCycleVisualModes()
        {
            AddRepeatStep("cycle mode", () => InputManager.Key(Key.Space), 6);
        }

        [Test]
        public void TestCertification()
        {
            checkDifficulty(1);
            clickUntilResults(true);
            continueFromResults();
            checkDifficulty(2);

            clickUntilResults(false);
            continueFromResults();
            checkDifficulty(1);

            clickUntilResults(true);
            AddAssert("check at results", () => !latencyCertifier.ChildrenOfType<LatencyArea>().Any());
            checkDifficulty(1);
        }

        private void continueFromResults()
        {
            AddAssert("check at results", () => !latencyCertifier.ChildrenOfType<LatencyArea>().Any());
            AddStep("hit enter to continue", () => InputManager.Key(Key.Enter));
        }

        private void checkDifficulty(int difficulty)
        {
            AddAssert($"difficulty is {difficulty}", () => latencyCertifier.DifficultyLevel == difficulty);
        }

        private void clickUntilResults(bool clickCorrect)
        {
            AddUntilStep("click correct button until results", () =>
            {
                var latencyArea = latencyCertifier
                                  .ChildrenOfType<LatencyArea>()
                                  .SingleOrDefault(a => clickCorrect ? a.TargetFrameRate == null : a.TargetFrameRate != null);

                // reached results
                if (latencyArea == null)
                    return true;

                latencyArea.ChildrenOfType<OsuButton>().Single().TriggerClick();
                return false;
            });
        }
    }
}
