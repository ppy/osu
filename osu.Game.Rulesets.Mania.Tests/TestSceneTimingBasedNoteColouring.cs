// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Tests.Visual;
using osu.Framework.Timing;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Configuration;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Mania.UI;

namespace osu.Game.Rulesets.Mania.Tests
{
    [TestFixture]
    public class TestSceneTimingBasedNoteColouring : OsuTestScene
    {
        private Bindable<bool> configTimingBasedNoteColouring;

        private ManualClock clock;
        private DrawableManiaRuleset drawableRuleset;

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
                    drawableRuleset = (DrawableManiaRuleset)Ruleset.Value.CreateInstance().CreateDrawableRulesetWith(createTestBeatmap())
                }
            });
            AddStep("retrieve config bindable", () =>
            {
                var config = (ManiaRulesetConfigManager)RulesetConfigs.GetConfigFor(Ruleset.Value.CreateInstance()).AsNonNull();
                configTimingBasedNoteColouring = config.GetBindable<bool>(ManiaRulesetSetting.TimingBasedNoteColouring);
            });
        }

        [Test]
        public void TestSimple()
        {
            AddStep("enable", () => configTimingBasedNoteColouring.Value = true);
            AddStep("disable", () => configTimingBasedNoteColouring.Value = false);
        }

        [Test]
        public void TestToggleOffScreen()
        {
            AddStep("enable", () => configTimingBasedNoteColouring.Value = true);

            seekTo(10000);
            AddStep("disable", () => configTimingBasedNoteColouring.Value = false);
            seekTo(0);
            AddAssert("all notes not coloured", () => this.ChildrenOfType<DrawableNote>().All(note => note.Colour == Colour4.White));

            seekTo(10000);
            AddStep("enable again", () => configTimingBasedNoteColouring.Value = true);
            seekTo(0);
            AddAssert("some notes coloured", () => this.ChildrenOfType<DrawableNote>().Any(note => note.Colour != Colour4.White));
        }

        private void seekTo(double time)
        {
            AddStep($"seek to {time}", () => clock.CurrentTime = time);
            AddUntilStep("wait for seek", () => Precision.AlmostEquals(drawableRuleset.FrameStableClock.CurrentTime, time, 1));
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
