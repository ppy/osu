// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Game.Rulesets.Taiko.Beatmaps;
using osu.Game.Rulesets.Taiko.Mods;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Replays;

namespace osu.Game.Rulesets.Taiko.Tests.Mods
{
    public partial class TestSceneTaikoModRelax : TaikoModTestScene
    {
        [Test]
        public void TestRelax()
        {
            var beatmap = new TaikoBeatmap
            {
                HitObjects =
                {
                    new Hit { StartTime = 0, Type = HitType.Centre, },
                    new Hit { StartTime = 250, Type = HitType.Rim, },
                    new DrumRoll { StartTime = 500, Duration = 500, },
                    new Swell { StartTime = 1250, Duration = 500 },
                }
            };
            foreach (var ho in beatmap.HitObjects)
                ho.ApplyDefaults(beatmap.ControlPointInfo, beatmap.Difficulty);

            var replay = new TaikoAutoGenerator(beatmap).Generate();

            foreach (var frame in replay.Frames.OfType<TaikoReplayFrame>().Where(r => r.Actions.Any()))
                frame.Actions = [TaikoAction.LeftCentre];

            CreateModTest(new ModTestData
            {
                Mod = new TaikoModRelax(),
                Beatmap = beatmap,
                ReplayFrames = replay.Frames,
                Autoplay = false,
                PassCondition = () => Player.ScoreProcessor.HasCompleted.Value && Player.ScoreProcessor.Accuracy.Value == 1,
            });
        }
    }
}
