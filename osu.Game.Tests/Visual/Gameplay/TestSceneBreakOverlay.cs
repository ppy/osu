// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Screens;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneBreakOverlay : OsuPlayerTestScene
    {
        [Test]
        public void TestGradeNotUpdatedOnExit()
        {
            bool gradeDisplayUpdated = false;

            AddStep("exit player", () =>
            {
                gradeDisplayUpdated = false;
                Player.BreakOverlay.Info.GradeDisplay.Current.ValueChanged += _ => { gradeDisplayUpdated = true; };
                Player.Exit();
            });
            AddAssert("grade display didn't update", () => !gradeDisplayUpdated);
        }
    }
}
