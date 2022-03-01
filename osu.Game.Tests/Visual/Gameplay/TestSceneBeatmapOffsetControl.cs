// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Overlays.Settings;
using osu.Game.Scoring;
using osu.Game.Screens.Play.PlayerSettings;
using osu.Game.Tests.Visual.Ranking;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneBeatmapOffsetControl : OsuTestScene
    {
        private BeatmapOffsetControl offsetControl;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("Create control", () =>
            {
                Child = new PlayerSettingsGroup("Some settings")
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        offsetControl = new BeatmapOffsetControl()
                    }
                };
            });
        }

        [Test]
        public void TestDisplay()
        {
            const double average_error = -4.5;

            AddAssert("Offset is neutral", () => offsetControl.Current.Value == 0);
            AddAssert("No calibration button", () => !offsetControl.ChildrenOfType<SettingsButton>().Any());
            AddStep("Set reference score", () =>
            {
                offsetControl.ReferenceScore.Value = new ScoreInfo
                {
                    HitEvents = TestSceneHitEventTimingDistributionGraph.CreateDistributedHitEvents(average_error)
                };
            });

            AddAssert("Has calibration button", () => offsetControl.ChildrenOfType<SettingsButton>().Any());
            AddStep("Press button", () => offsetControl.ChildrenOfType<SettingsButton>().Single().TriggerClick());
            AddAssert("Offset is adjusted", () => offsetControl.Current.Value == average_error);

            AddStep("Remove reference score", () => offsetControl.ReferenceScore.Value = null);
            AddAssert("No calibration button", () => !offsetControl.ChildrenOfType<SettingsButton>().Any());
        }
    }
}
