// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Framework.Graphics;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Mania.Configuration;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Allocation;
using osu.Framework.Timing;
using osu.Framework.Testing;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using System.Linq;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.Tests.Skinning
{
    [TestFixture]
    public partial class TestSceneAdjustableTimingColouring : ManiaSkinnableTestScene
    {
        private Bindable<bool> configTimingBasedNoteColouring = null!;

        private ManiaBeatmap testBeatmap = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            testBeatmap = createTestBeatmap();
            SetContents(skin =>
            {
                var drawableRuleset = (DrawableManiaRuleset)Ruleset.Value.CreateInstance().CreateDrawableRulesetWith(testBeatmap);
                drawableRuleset.Clock = new FramedClock(new ManualClock());

                return drawableRuleset;
            });
            var config = (ManiaRulesetConfigManager)RulesetConfigs.GetConfigFor(Ruleset.Value.CreateInstance()).AsNonNull();
            configTimingBasedNoteColouring = config.GetBindable<bool>(ManiaRulesetSetting.TimingBasedNoteColouring);
        }

        [Test]
        public void TestColouring()
        {
            AddStep("disable colouring", () => configTimingBasedNoteColouring.Value = false);
            AddStep("enable colouring", () => configTimingBasedNoteColouring.Value = true);
        }

        [Test]
        public void TestCustomColouring()
        {
            AddStep("disable colouring", () => configTimingBasedNoteColouring.Value = false);
            AddAssert("all notes not coloured", () => this.ChildrenOfType<DrawableNote>().All(note => note.Colour == Colour4.White));
            AddStep("enable colouring", () => configTimingBasedNoteColouring.Value = true);
            AddAssert("any notes coloured", () => this.ChildrenOfType<DrawableNote>().Any(note => note.Colour != Colour4.White));
            AddAssert("special-skin colours correct",
                () => Cell(4).ChildrenOfType<DrawableNote>().All(note =>
                    snapColourIsAccurate(
                        note.Colour,
                        testBeatmap.ControlPointInfo.GetClosestBeatDivisor(note.HitObject.StartTime)
                    )
                )
            );
        }

        private bool snapColourIsAccurate(Color4 color, int divisor)
        {
            switch (divisor)
            {
                case 1:
                    return color == new Color4(255, 0, 0, 255);

                case 2:
                    return color == new Color4(0, 0, 255, 255);

                case 3:
                    return color == new Color4(0, 255, 0, 255);

                default:
                    return color == new Color4(255, 255, 255, 255);
            }
        }

        private ManiaBeatmap createTestBeatmap()
        {
            const double beat_length = 1000;

            var beatmap = new ManiaBeatmap(new StageDefinition(1))
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
