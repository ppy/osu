// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Replays;
using osu.Game.Rulesets.Osu.Configuration;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.Replays;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    public partial class TestSceneOsuAnalysisContainer : OsuTestScene
    {
        private TestReplayAnalysisOverlay analysisContainer = null!;
        private ReplayAnalysisSettings settings = null!;

        [Cached]
        private OsuRulesetConfigManager config = new OsuRulesetConfigManager(null, new OsuRuleset().RulesetInfo);

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create analysis container", () =>
            {
                Children = new Drawable[]
                {
                    analysisContainer = new TestReplayAnalysisOverlay(fabricateReplay()),
                    settings = new ReplayAnalysisSettings(config),
                };
            });
        }

        [Test]
        public void TestHitMarkers()
        {
            AddStep("enable hit markers", () => settings.HitMarkersEnabled.Value = true);
            AddAssert("hit markers visible", () => analysisContainer.HitMarkersVisible);
            AddStep("disable hit markers", () => settings.HitMarkersEnabled.Value = false);
            AddAssert("hit markers not visible", () => !analysisContainer.HitMarkersVisible);
        }

        [Test]
        public void TestAimMarker()
        {
            AddStep("enable aim markers", () => settings.AimMarkersEnabled.Value = true);
            AddAssert("aim markers visible", () => analysisContainer.AimMarkersVisible);
            AddStep("disable aim markers", () => settings.AimMarkersEnabled.Value = false);
            AddAssert("aim markers not visible", () => !analysisContainer.AimMarkersVisible);
        }

        [Test]
        public void TestAimLines()
        {
            AddStep("enable aim lines", () => settings.AimLinesEnabled.Value = true);
            AddAssert("aim lines visible", () => analysisContainer.AimLinesVisible);
            AddStep("disable aim lines", () => settings.AimLinesEnabled.Value = false);
            AddAssert("aim lines not visible", () => !analysisContainer.AimLinesVisible);
        }

        private Replay fabricateReplay()
        {
            var frames = new List<ReplayFrame>();
            var random = new Random();
            int posX = 250;
            int posY = 250;
            bool leftOrRight = false;

            for (int i = 0; i < 1000; i++)
            {
                posX = Math.Clamp(posX + random.Next(-10, 11), 0, 500);
                posY = Math.Clamp(posY + random.Next(-10, 11), 0, 500);

                var actions = new List<OsuAction>();

                if (i % 20 == 0)
                {
                    actions.Add(leftOrRight ? OsuAction.LeftButton : OsuAction.RightButton);
                    leftOrRight = !leftOrRight;
                }

                frames.Add(new OsuReplayFrame
                {
                    Time = Time.Current + i * 15,
                    Position = new Vector2(posX, posY),
                    Actions = actions
                });
            }

            return new Replay { Frames = frames };
        }

        private partial class TestReplayAnalysisOverlay : ReplayAnalysisOverlay
        {
            public TestReplayAnalysisOverlay(Replay replay)
                : base(replay)
            {
            }

            public bool HitMarkersVisible => HitMarkers.Alpha > 0 && HitMarkers.Entries.Any();
            public bool AimMarkersVisible => AimMarkers.Alpha > 0 && AimMarkers.Entries.Any();
            public bool AimLinesVisible => AimLines.Alpha > 0 && AimLines.Vertices.Count > 1;
        }
    }
}
