// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Replays;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Tests.Visual;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    public class TestSceneMissHitWindowJudgements : ModTestScene
    {
        public TestSceneMissHitWindowJudgements()
            : base(new OsuRuleset())
        {
        }

        [Test]
        public void TestMissViaEarlyHit()
        {
            var beatmap = new Beatmap
            {
                HitObjects = { new HitCircle { Position = new Vector2(256, 192) } }
            };

            var hitWindows = new OsuHitWindows();
            hitWindows.SetDifficulty(beatmap.BeatmapInfo.BaseDifficulty.OverallDifficulty);

            CreateModTest(new ModTestData
            {
                Autoplay = false,
                Mod = new TestAutoMod(),
                Beatmap = new Beatmap
                {
                    HitObjects = { new HitCircle { Position = new Vector2(256, 192) } }
                },
                PassCondition = () => Player.Results.Count > 0 && Player.Results[0].TimeOffset < -hitWindows.WindowFor(HitResult.Meh) && Player.Results[0].Type == HitResult.Miss
            });
        }

        [Test]
        public void TestMissViaNotHitting()
        {
            var beatmap = new Beatmap
            {
                HitObjects = { new HitCircle { Position = new Vector2(256, 192) } }
            };

            var hitWindows = new OsuHitWindows();
            hitWindows.SetDifficulty(beatmap.BeatmapInfo.BaseDifficulty.OverallDifficulty);

            CreateModTest(new ModTestData
            {
                Autoplay = false,
                Beatmap = beatmap,
                PassCondition = () => Player.Results.Count > 0 && Player.Results[0].TimeOffset >= hitWindows.WindowFor(HitResult.Meh) && Player.Results[0].Type == HitResult.Miss
            });
        }

        private class TestAutoMod : OsuModAutoplay
        {
            public override Score CreateReplayScore(IBeatmap beatmap) => new Score
            {
                ScoreInfo = new ScoreInfo { User = new User { Username = "Autoplay" } },
                Replay = new MissingAutoGenerator(beatmap).Generate()
            };
        }

        private class MissingAutoGenerator : OsuAutoGeneratorBase
        {
            public new OsuBeatmap Beatmap => (OsuBeatmap)base.Beatmap;

            public MissingAutoGenerator(IBeatmap beatmap)
                : base(beatmap)
            {
            }

            public override Replay Generate()
            {
                AddFrameToReplay(new OsuReplayFrame(-100000, new Vector2(256, 500)));
                AddFrameToReplay(new OsuReplayFrame(Beatmap.HitObjects[0].StartTime - 1500, new Vector2(256, 500)));
                AddFrameToReplay(new OsuReplayFrame(Beatmap.HitObjects[0].StartTime - 1500, new Vector2(256, 500)));

                AddFrameToReplay(new OsuReplayFrame(Beatmap.HitObjects[0].StartTime - 450, Beatmap.HitObjects[0].StackedPosition));
                AddFrameToReplay(new OsuReplayFrame(Beatmap.HitObjects[0].StartTime - 350, Beatmap.HitObjects[0].StackedPosition, OsuAction.LeftButton));
                AddFrameToReplay(new OsuReplayFrame(Beatmap.HitObjects[0].StartTime - 325, Beatmap.HitObjects[0].StackedPosition));

                return Replay;
            }
        }
    }
}
