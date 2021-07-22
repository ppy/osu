// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
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
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests.Mods
{
    public class TestSceneOsuModRelax : OsuModTestScene
    {
        protected new ScoreAccessiblePlayer Player => (ScoreAccessiblePlayer)base.Player;

        [Test]
        public void TestKeyUp()
        {
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
                    HitObjects = CreateHitObjects()
                },
                PassCondition = () => Player.ScoreProcessor.Combo.Value == 15
            });

            AddUntilStep("wait for beatmap end", () => Player.ScoreProcessor.HasCompleted.Value);
            AddAssert("no key was held", () => Player.Score.Replay.Frames
                                                     .Cast<OsuReplayFrame>().Last().Actions.Count == 0);
        }

        protected override TestPlayer CreateModPlayer(Ruleset ruleset) => new ScoreAccessiblePlayer(AllowFail);

        protected List<HitObject> CreateHitObjects()
        {
            const int start = 500;
            var list = new List<HitObject>();

            for (int i = 0; i < 10; i++)
            {
                list.Add(new HitCircle
                {
                    StartTime = start + i * 50,
                    Position = new Vector2((i + 1) * 32, OsuPlayfield.BASE_SIZE.Y / 2)
                });
            }

            var lastPos = ((HitCircle)list.Last()).Position + new Vector2(32, 0);

            for (int i = 0; i < 5; i++)
            {
                list.Add(new HitCircle
                {
                    StartTime = list.Last().StartTime,
                    Position = lastPos
                });
            }

            return list;
        }

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
