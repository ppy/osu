// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Tests.Visual;
using osu.Framework.Timing;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Configuration;
using osu.Framework.Bindables;
using osu.Framework.Testing;

namespace osu.Game.Rulesets.Mania.Tests
{
    [TestFixture]
    public class TestSceneTimingBasedNoteColouring : OsuTestScene
    {
        [Resolved]
        private RulesetConfigCache configCache { get; set; }

        private Bindable<bool> configTimingBasedNoteColouring;

        private ManualClock clock;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("setup hierarchy", () => Child = new Container
            {
                Clock = new FramedClock(clock = new ManualClock()),
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children = new[]
                {
                    Ruleset.Value.CreateInstance().CreateDrawableRulesetWith(createTestBeatmap())
                }
            });
            AddStep("retrieve config bindable", () =>
            {
                var config = (ManiaRulesetConfigManager)configCache.GetConfigFor(Ruleset.Value.CreateInstance());
                configTimingBasedNoteColouring = config.GetBindable<bool>(ManiaRulesetSetting.TimingBasedNoteColouring);
            });
        }

        [Test]
        public void TestSimple()
        {
            AddStep("enable", () => configTimingBasedNoteColouring.Value = true);
            AddStep("disable", () => configTimingBasedNoteColouring.Value = false);
        }

        private ManiaBeatmap createTestBeatmap()
        {
            const double beat_length = 500;

            var beatmap = new ManiaBeatmap(new StageDefinition { Columns = 1 })
            {
                HitObjects =
                {
                    new Note { StartTime = 0 },
                    new Note { StartTime = beat_length / 16 },
                    new Note { StartTime = beat_length / 12 },
                    new Note { StartTime = beat_length / 8 },
                    new Note { StartTime = beat_length / 6 },
                    new Note { StartTime = beat_length / 4 },
                    new Note { StartTime = beat_length / 3 },
                    new Note { StartTime = beat_length / 2 },
                    new Note { StartTime = beat_length }
                },
                ControlPointInfo = new ControlPointInfo(),
                BeatmapInfo = { Ruleset = Ruleset.Value },
            };

            foreach (var note in beatmap.HitObjects)
            {
                note.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());
            }

            beatmap.ControlPointInfo.Add(0, new TimingControlPoint
            {
                BeatLength = beat_length
            });
            return beatmap;
        }
    }
}
