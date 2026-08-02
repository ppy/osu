// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Game.Replays;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Scoring;
using osuTK;

namespace osu.Game.Tests.Visual
{
    public partial class TestSceneReplayStability : ReplayStabilityTestScene
    {
        [Test]
        public void TestOutrageouslyLargeLeadInTime()
        {
            // "graciously borrowed" from https://osu.ppy.sh/beatmapsets/948643#osu/1981090
            const double lead_in_time = 2147272727;
            const double hit_circle_time = 100;

            var beatmap = new OsuBeatmap
            {
                HitObjects =
                {
                    new HitCircle
                    {
                        StartTime = hit_circle_time,
                        Position = OsuPlayfield.BASE_SIZE / 2
                    }
                },
                AudioLeadIn = lead_in_time,
                BeatmapInfo =
                {
                    Ruleset = new OsuRuleset().RulesetInfo,
                },
            };

            var replay = new Replay
            {
                Frames = Enumerable.Range(0, 300).Select(t => new OsuReplayFrame(-lead_in_time + 40 * t, new Vector2(t), t % 2 == 0 ? [] : [OsuAction.LeftButton]))
                                   .Concat([
                                       new OsuReplayFrame(0, OsuPlayfield.BASE_SIZE / 2),
                                       new OsuReplayFrame(hit_circle_time, OsuPlayfield.BASE_SIZE / 2, OsuAction.LeftButton),
                                       new OsuReplayFrame(hit_circle_time + 20, OsuPlayfield.BASE_SIZE / 2),
                                   ])
                                   .Cast<ReplayFrame>()
                                   .ToList(),
            };

            RunTest(beatmap, replay, [HitResult.Great]);
        }
    }
}
