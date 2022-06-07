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
    public class TestSceneLatencyComparer : ScreenTestScene
    {
        private LatencyComparerScreen latencyComparer = null!;

        public override void SetUpSteps()
        {
            base.SetUpSteps();
            AddStep("Load screen", () => LoadScreen(latencyComparer = new LatencyComparerScreen()));
        }

        [Test]
        public void TestCertification()
        {
            for (int i = 0; i < 4; i++)
            {
                clickCorrectUntilResults();
                AddAssert("check at results", () => !latencyComparer.ChildrenOfType<LatencyArea>().Any());
                AddStep("hit c to continue", () => InputManager.Key(Key.C));
            }

            AddAssert("check at results", () => !latencyComparer.ChildrenOfType<LatencyArea>().Any());

            AddAssert("check no buttons", () => !latencyComparer.ChildrenOfType<OsuButton>().Any());
        }

        private void clickCorrectUntilResults()
        {
            AddUntilStep("click correct button until results", () =>
            {
                var latencyArea = latencyComparer
                                  .ChildrenOfType<LatencyArea>()
                                  .SingleOrDefault(a => a.TargetFrameRate == 0);

                // reached results
                if (latencyArea == null)
                    return true;

                latencyArea.ChildrenOfType<OsuButton>().Single().TriggerClick();
                return false;
            });
        }
    }
}
