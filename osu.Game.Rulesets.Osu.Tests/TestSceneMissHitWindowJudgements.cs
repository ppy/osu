// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Replays;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Rulesets.Scoring;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    public partial class TestSceneMissHitWindowJudgements : ModTestScene
    {
        protected override Ruleset CreatePlayerRuleset() => new OsuRuleset();

        [Test]
        public void TestMissViaEarlyHit()
        {
            var beatmap = new Beatmap
            {
                HitObjects = { new HitCircle { Position = new Vector2(256, 192) } }
            };

            var hitWindows = new OsuHitWindows();
            hitWindows.SetDifficulty(beatmap.Difficulty.OverallDifficulty);

            CreateModTest(new ModTestData
            {
                Autoplay = false,
                Mod = new TestAutoMod(),
                Beatmap = new Beatmap
                {
                    HitObjects = { new HitCircle { Position = new Vector2(256, 192) } }
                },
                PassCondition = () => Player.Results.Count > 0 && Player.Results[0].TimeOffset < -hitWindows.WindowFor(HitResult.Meh) && !Player.Results[0].IsHit
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
            hitWindows.SetDifficulty(beatmap.Difficulty.OverallDifficulty);

            CreateModTest(new ModTestData
            {
                Autoplay = false,
                Beatmap = beatmap,
                PassCondition = () => Player.Results.Count > 0 && Player.Results[0].TimeOffset >= hitWindows.WindowFor(HitResult.Meh) && !Player.Results[0].IsHit
            });
        }

        private class TestAutoMod : OsuModAutoplay
        {
            public override ModReplayData CreateReplayData(IBeatmap beatmap, IReadOnlyList<Mod> mods)
                => new ModReplayData(new MissingAutoGenerator(beatmap, mods).Generate(), new ModCreatedUser { Username = "Autoplay" });
        }

        private class MissingAutoGenerator : OsuAutoGeneratorBase
        {
            public new OsuBeatmap Beatmap => (OsuBeatmap)base.Beatmap;

            public MissingAutoGenerator(IBeatmap beatmap, IReadOnlyList<Mod> mods)
                : base(beatmap, mods)
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
