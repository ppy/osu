// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Utility;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Settings
{
    public class TestSceneLatencyCertifierScreen : ScreenTestScene
    {
        private LatencyCertifierScreen latencyCertifier = null!;

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("Load screen", () => LoadScreen(latencyCertifier = new LatencyCertifierScreen()));
            AddUntilStep("wait for load", () => latencyCertifier.IsLoaded);
        }

        [Test]
        public void TestCertification()
        {
            for (int i = 0; i < 4; i++)
            {
                int difficulty = i + 1;

                checkDifficulty(difficulty);
                clickUntilResults(true);
                continueFromResults();
            }

            checkDifficulty(5);
            clickUntilResults(false);
            continueFromResults();
            checkDifficulty(4);

            clickUntilResults(false);
            continueFromResults();
            checkDifficulty(3);

            clickUntilResults(true);
            AddAssert("check at results", () => !latencyCertifier.ChildrenOfType<LatencyArea>().Any());
            AddAssert("check no buttons", () => !latencyCertifier.ChildrenOfType<OsuButton>().Any());
            checkDifficulty(3);
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
