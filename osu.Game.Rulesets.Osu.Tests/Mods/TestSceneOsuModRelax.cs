// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Scoring;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Osu.Tests.Mods
{
    public class TestSceneOsuModRelax : OsuModTestScene
    {
        [Test]
        public void Test()
        {
            var lastObject = new HitCircle()
            {
                Position = OsuPlayfield.BASE_SIZE / 2,
                StartTime = 2550
            };
            CreateModTest(new ModTestData
            {
                Autoplay = false,
                Mods =
                    new Mod[]
                    {
                        new OsuModRelax(),
                        new OsuModAutopilot()
                    },
                Beatmap = new Beatmap
                {
                    HitObjects =
                    {
                        new HitCircle()
                        {
                            Position = OsuPlayfield.BASE_SIZE / 2,
                            StartTime = 2500
                        },
                        lastObject
                    }
                },
                PassCondition = () => Player.ScoreProcessor.JudgedHits == 2
            });
            Score score = null;

            AddUntilStep("wait for beatmap end", () => Player.GameplayClockContainer.GameplayClock.CurrentTime >= lastObject.GetEndTime());
            AddStep("get score", () => score = ((ScoreAccessiblePlayer)Player).Score);
            AddAssert("no key was held", () => score.Replay.Frames.Cast<OsuReplayFrame>().TakeLast(2).FirstOrDefault().Actions.Count == 0);
        }

        protected override TestPlayer CreateModPlayer(Ruleset ruleset) => new ScoreAccessiblePlayer(AllowFail);

        protected class ScoreAccessiblePlayer : ModTestPlayer
        {
            public ScoreAccessiblePlayer(bool allowFail)
                : base(allowFail)
            {
            }

            public new Score Score => base.Score;
        }
    }
}
