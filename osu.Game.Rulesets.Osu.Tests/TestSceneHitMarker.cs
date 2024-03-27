// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Replays;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.Replays;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    public partial class TestSceneHitMarker : OsuTestScene
    {
        private TestOsuAnalysisContainer analysisContainer;

        [Test]
        public void TestHitMarkers()
        {
            createAnalysisContainer();
            AddStep("enable hit markers", () => analysisContainer.HitMarkerEnabled.Value = true);
            AddAssert("hit markers visible", () => analysisContainer.HitMarkersVisible);
            AddStep("disable hit markers", () => analysisContainer.HitMarkerEnabled.Value = false);
            AddAssert("hit markers not visible", () => !analysisContainer.HitMarkersVisible);
        }

        [Test]
        public void TestAimMarker()
        {
            createAnalysisContainer();
            AddStep("enable aim markers", () => analysisContainer.AimMarkersEnabled.Value = true);
            AddAssert("aim markers visible", () => analysisContainer.AimMarkersVisible);
            AddStep("disable aim markers", () => analysisContainer.AimMarkersEnabled.Value = false);
            AddAssert("aim markers not visible", () => !analysisContainer.AimMarkersVisible);
        }

        [Test]
        public void TestAimLines()
        {
            createAnalysisContainer();
            AddStep("enable aim lines", () => analysisContainer.AimLinesEnabled.Value = true);
            AddAssert("aim lines visible", () => analysisContainer.AimLinesVisible);
            AddStep("disable aim lines", () => analysisContainer.AimLinesEnabled.Value = false);
            AddAssert("aim lines not visible", () => !analysisContainer.AimLinesVisible);
        }

        private void createAnalysisContainer()
        {
            AddStep("create new analysis container", () => Child = analysisContainer = new TestOsuAnalysisContainer(fabricateReplay()));
        }

        private Replay fabricateReplay()
        {
            var frames = new List<ReplayFrame>();

            for (int i = 0; i < 50; i++)
            {
                frames.Add(new OsuReplayFrame
                {
                    Time = Time.Current + i * 15,
                    Position = new Vector2(20 + i * 10, 20),
                    Actions =
                    {
                        i % 2 == 0 ? OsuAction.LeftButton : OsuAction.RightButton
                    }
                });
            }

            return new Replay { Frames = frames };
        }

        private partial class TestOsuAnalysisContainer : OsuAnalysisContainer
        {
            public TestOsuAnalysisContainer(Replay replay)
                : base(replay)
            {
            }

            public bool HitMarkersVisible => HitMarkers.Alpha > 0 && HitMarkers.Entries.Any();

            public bool AimMarkersVisible => AimMarkers.Alpha > 0 && AimMarkers.Entries.Any();

            public bool AimLinesVisible => AimLines.Alpha > 0 && AimLines.Vertices.Count > 1;
        }
    }
}
