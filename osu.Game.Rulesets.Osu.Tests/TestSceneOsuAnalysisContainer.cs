// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Framework.Timing;
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

        private readonly StopwatchClock clock = new StopwatchClock();

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create analysis container", () =>
            {
                Children = new Drawable[]
                {
                    new OsuPlayfieldAdjustmentContainer
                    {
                        Child = analysisContainer = new TestReplayAnalysisOverlay(fabricateReplay())
                        {
                            Clock = new FramedClock(clock)
                        },
                    },
                    settings = new ReplayAnalysisSettings(config),
                };

                settings.ShowClickMarkers.Value = false;
                settings.ShowAimMarkers.Value = false;
                settings.ShowCursorPath.Value = false;
            });
        }

        [Test]
        public void TestEverythingOn()
        {
            AddStep("enable everything", () =>
            {
                settings.ShowClickMarkers.Value = true;
                settings.ShowAimMarkers.Value = true;
                settings.ShowCursorPath.Value = true;
            });
            AddToggleStep("toggle pause", running =>
            {
                if (running)
                    clock.Stop();
                else
                    clock.Start();
            });
        }

        [Test]
        public void TestHitMarkers()
        {
            AddStep("stop at 2000", () =>
            {
                clock.Stop();
                clock.Seek(2000);
            });
            AddStep("enable hit markers", () => settings.ShowClickMarkers.Value = true);
            AddUntilStep("hit markers visible", () => analysisContainer.HitMarkersVisible);
            AddStep("disable hit markers", () => settings.ShowClickMarkers.Value = false);
            AddUntilStep("hit markers not visible", () => !analysisContainer.HitMarkersVisible);
        }

        [Test]
        public void TestAimMarker()
        {
            AddStep("stop at 2000", () =>
            {
                clock.Stop();
                clock.Seek(2000);
            });
            AddStep("enable aim markers", () => settings.ShowAimMarkers.Value = true);
            AddUntilStep("aim markers visible", () => analysisContainer.AimMarkersVisible);
            AddStep("disable aim markers", () => settings.ShowAimMarkers.Value = false);
            AddUntilStep("aim markers not visible", () => !analysisContainer.AimMarkersVisible);
        }

        [Test]
        public void TestAimLines()
        {
            AddStep("stop at 2000", () =>
            {
                clock.Stop();
                clock.Seek(2000);
            });
            AddStep("enable aim lines", () => settings.ShowCursorPath.Value = true);
            AddUntilStep("aim lines visible", () => analysisContainer.AimLinesVisible);
            AddStep("disable aim lines", () => settings.ShowCursorPath.Value = false);
            AddUntilStep("aim lines not visible", () => !analysisContainer.AimLinesVisible);
        }

        private Replay fabricateReplay()
        {
            var frames = new List<ReplayFrame>();
            var random = new Random(20250522);
            int posX = 250;
            int posY = 250;

            var actions = new HashSet<OsuAction>();

            for (int i = 0; i < 1000; i++)
            {
                posX = Math.Clamp(posX + random.Next(-20, 21), -100, 600);
                posY = Math.Clamp(posY + random.Next(-20, 21), -100, 600);

                if (random.NextDouble() > (actions.Count == 0 ? 0.9 : 0.95))
                {
                    actions.Add(random.NextDouble() > 0.5 ? OsuAction.LeftButton : OsuAction.RightButton);
                }
                else if (random.NextDouble() > 0.7)
                {
                    actions.Remove(random.NextDouble() > 0.5 ? OsuAction.LeftButton : OsuAction.RightButton);
                }

                frames.Add(new OsuReplayFrame
                {
                    Time = i * 15,
                    Position = new Vector2(posX, posY),
                    Actions = actions.ToList(),
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

            public bool HitMarkersVisible => ClickMarkers?.Alpha > 0 && ClickMarkers.Entries.Any();
            public bool AimMarkersVisible => FrameMarkers?.Alpha > 0 && FrameMarkers.Entries.Any();
            public bool AimLinesVisible => CursorPath?.Alpha > 0 && CursorPath.Vertices.Count > 1;
        }
    }
}
