// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Replays;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests.Mods
{
    public partial class TestSceneOsuModRelax : OsuModTestScene
    {
        private readonly HitCircle hitObject;
        private readonly HitWindows hitWindows = new OsuHitWindows();

        public TestSceneOsuModRelax()
        {
            hitWindows.SetDifficulty(9);

            hitObject = new HitCircle
            {
                StartTime = 1000,
                Position = new Vector2(100, 100),
                HitWindows = hitWindows
            };
        }

        protected override TestPlayer CreateModPlayer(Ruleset ruleset) => new ModRelaxTestPlayer(CurrentTestData, AllowFail);

        [Test]
        public void TestRelax() => CreateModTest(new ModTestData
        {
            Mod = new OsuModRelax(),
            Autoplay = false,
            CreateBeatmap = () => new Beatmap
            {
                HitObjects = new List<HitObject> { hitObject }
            },
            ReplayFrames = new List<ReplayFrame>
            {
                new OsuReplayFrame(0, new Vector2()),
                new OsuReplayFrame(hitObject.StartTime, hitObject.Position),
            },
            PassCondition = () => Player.ScoreProcessor.Combo.Value == 1
        });

        [Test]
        public void TestRelaxLeniency() => CreateModTest(new ModTestData
        {
            Mod = new OsuModRelax(),
            Autoplay = false,
            CreateBeatmap = () => new Beatmap
            {
                HitObjects = new List<HitObject> { hitObject }
            },
            ReplayFrames = new List<ReplayFrame>
            {
                new OsuReplayFrame(0, new Vector2(hitObject.X - 22, hitObject.Y - 22)), // must be an edge hit for the cursor to not stay on the object for too long
                new OsuReplayFrame(hitObject.StartTime - OsuModRelax.RELAX_LENIENCY, new Vector2(hitObject.X - 22, hitObject.Y - 22)),
                new OsuReplayFrame(hitObject.StartTime, new Vector2(0)),
            },
            PassCondition = () => Player.ScoreProcessor.Combo.Value == 1
        });

        protected partial class ModRelaxTestPlayer : ModTestPlayer
        {
            private readonly ModTestData currentTestData;

            public ModRelaxTestPlayer(ModTestData data, bool allowFail)
                : base(data, allowFail)
            {
                currentTestData = data;
            }

            protected override void PrepareReplay()
            {
                // We need to set IsLegacyScore to true otherwise the mod assumes that presses are already embedded into the replay
                DrawableRuleset?.SetReplayScore(new Score
                {
                    Replay = new Replay { Frames = currentTestData.ReplayFrames! },
                    ScoreInfo = new ScoreInfo { User = new APIUser { Username = @"Test" }, IsLegacyScore = true, Mods = new Mod[] { new OsuModRelax() } },
                });

                DrawableRuleset?.SetRecordTarget(Score);
            }
        }
    }
}
