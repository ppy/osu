// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Legacy.Osu;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Screens.Play;
using osu.Game.Tests.Beatmaps;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Tests
{
    public class Benchmark : OsuTestGame
    {
        protected override void LoadComplete()
        {
            base.LoadComplete();

            var testRunner = new TestCaseTestRunner.TestRunner();
            Add(testRunner);

            Task.Run(() => { testRunner.RunTestBlocking(new TestCasePlayerBenchmark(RulesetStore.AvailableRulesets.First().CreateInstance())); });
        }

        public class TestCasePlayerBenchmark : PlayerTestCase
        {
            private TerminatingPlayer terminatingPlayer;

            public TestCasePlayerBenchmark(Ruleset ruleset)
                : base(ruleset)
            {
                // probably should be handled in a better way.
                SetUpSteps();
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                AddStep("seek to end", () => terminatingPlayer.SeekToEnd());
                AddWaitStep("wait", 1000);
            }

            protected override IBeatmap CreateBeatmap(Ruleset ruleset) => new TestBenchmarkBeatmap(ruleset.RulesetInfo);

            protected override Player CreatePlayer(Ruleset ruleset)
            {
                Beatmap.Value.Mods.Value = Beatmap.Value.Mods.Value.Concat(new[] { ruleset.GetAutoplayMod() });
                return terminatingPlayer = new TerminatingPlayer
                {
                    AllowPause = false,
                    AllowLeadIn = false,
                    AllowResults = false,
                };
            }

            private const int count_circle = 5000;
            private const int count_slider = 2000;
            private const int count_spinner = 500;

            private class TestBenchmarkBeatmap : TestBeatmap
            {
                public TestBenchmarkBeatmap(RulesetInfo ruleset)
                    : base(ruleset)
                {
                    var hitObjects = new List<HitObject>();

                    Vector2 getPos(int i) => new Vector2((i * 10f) % 512, ((i * 10f) / 512) % 384);

                    int time = 0;

                    for (int i = 0; i < count_circle; i++)
                        hitObjects.Add(new ConvertHit
                        {
                            StartTime = (time += 20),
                            Position = getPos(i)
                        });

                    for (int i = 0; i < count_slider; i++)
                        hitObjects.Add(new ConvertSlider
                        {
                            StartTime = (time += 100),
                            Path = new SliderPath(PathType.Bezier, new[]
                            {
                                Vector2.Zero,
                                new Vector2(0, 40),
                            }),
                            Position = getPos(i)
                        });

                    for (int i = 0; i < count_spinner; i++)
                    {
                        hitObjects.Add(new ConvertSpinner
                        {
                            Position = new Vector2(512, 384) / 2,
                            StartTime = (time += 200),
                            EndTime = time + 180,
                        });
                    }

                    HitObjects = hitObjects;
                }
            }

            private class TerminatingPlayer : Player
            {
                public void SeekToEnd()
                {
                    GameplayClockContainer.Seek(Beatmap.Value.Beatmap.HitObjects.Last().StartTime);
                }

                private OsuGameBase game;

                [BackgroundDependencyLoader]
                private void load(OsuGameBase game)
                {
                    this.game = game;
                }

                protected override void Update()
                {
                    base.Update();

                    if (!ValidForResume)
                        game.Exit();
                }
            }
        }
    }
}
