// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Screens.Play;
using osu.Game.Tests.Visual;
using OpenTK;

namespace osu.Game.Rulesets.Catch.Tests
{
    public class TestCaseAutoJuiceStream : TestCasePlayer
    {
        public TestCaseAutoJuiceStream()
            : base(new CatchRuleset())
        {
        }

        protected override IBeatmap CreateBeatmap(Ruleset ruleset)
        {
            var beatmap = new Beatmap
            {
                BeatmapInfo = new BeatmapInfo
                {
                    BaseDifficulty = new BeatmapDifficulty { CircleSize = 6, SliderMultiplier = 3 },
                    Ruleset = ruleset.RulesetInfo
                }
            };

            for (int i = 0; i < 100; i++)
            {
                float width = (i % 10 + 1) / 20f;

                beatmap.HitObjects.Add(new JuiceStream
                {
                    X = 0.5f - width / 2,
                    ControlPoints = new List<Vector2>
                    {
                        Vector2.Zero,
                        new Vector2(width * CatchPlayfield.BASE_WIDTH, 0)
                    },
                    CurveType = CurveType.Linear,
                    Distance = width * CatchPlayfield.BASE_WIDTH,
                    StartTime = i * 2000,
                    NewCombo = i % 8 == 0
                });
            }

            return beatmap;
        }

        protected override Player CreatePlayer(Ruleset ruleset)
        {
            Beatmap.Value.Mods.Value = Beatmap.Value.Mods.Value.Concat(new[] { ruleset.GetAutoplayMod() });
            return base.CreatePlayer(ruleset);
        }
    }
}
