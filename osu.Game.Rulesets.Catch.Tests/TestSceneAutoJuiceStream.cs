// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Catch.Tests
{
    public class TestSceneAutoJuiceStream : PlayerTestScene
    {
        public TestSceneAutoJuiceStream()
            : base(new CatchRuleset())
        {
        }

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset)
        {
            var beatmap = new Beatmap
            {
                BeatmapInfo = new BeatmapInfo
                {
                    BaseDifficulty = new BeatmapDifficulty { CircleSize = 6, SliderMultiplier = 3 },
                    Ruleset = ruleset
                }
            };

            for (int i = 0; i < 100; i++)
            {
                float width = (i % 10 + 1) / 20f;

                beatmap.HitObjects.Add(new JuiceStream
                {
                    X = 0.5f - width / 2,
                    Path = new SliderPath(PathType.Linear, new[]
                    {
                        Vector2.Zero,
                        new Vector2(width * CatchPlayfield.BASE_WIDTH, 0)
                    }),
                    StartTime = i * 2000,
                    NewCombo = i % 8 == 0
                });
            }

            return beatmap;
        }

        protected override TestPlayer CreatePlayer(Ruleset ruleset)
        {
            SelectedMods.Value = SelectedMods.Value.Concat(new[] { ruleset.GetAutoplayMod() }).ToArray();
            return base.CreatePlayer(ruleset);
        }
    }
}
