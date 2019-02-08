// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Lists;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Play;
using osu.Game.Tests.Beatmaps;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual
{
    public abstract class TestCasePlayer : ScreenTestCase
    {
        private readonly Ruleset ruleset;

        protected Player Player;

        protected TestCasePlayer(Ruleset ruleset)
        {
            this.ruleset = ruleset;
        }

        protected TestCasePlayer()
        {
        }

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            Add(new Box
            {
                RelativeSizeAxes = Framework.Graphics.Axes.Both,
                Colour = Color4.Black,
                Depth = int.MaxValue
            });

            if (ruleset != null)
            {
                Player p = null;
                AddStep(ruleset.RulesetInfo.Name, () => p = loadPlayerFor(ruleset));
                AddCheckSteps(() => p);
            }
            else
            {
                foreach (var r in rulesets.AvailableRulesets)
                {
                    Player p = null;
                    AddStep(r.Name, () => p = loadPlayerFor(r));
                    AddCheckSteps(() => p);

                    AddUntilStep(() =>
                    {
                        p = null;

                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        int count = 0;

                        workingWeakReferences.ForEachAlive(_ => count++);
                        return count == 1;
                    }, "no leaked beatmaps");

                    AddUntilStep(() =>
                    {
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        int count = 0;

                        playerWeakReferences.ForEachAlive(_ => count++);
                        return count == 1;
                    }, "no leaked players");
                }
            }
        }

        protected virtual void AddCheckSteps(Func<Player> player)
        {
            AddUntilStep(() => player().IsLoaded, "player loaded");
        }

        protected virtual IBeatmap CreateBeatmap(Ruleset ruleset) => new TestBeatmap(ruleset.RulesetInfo);

        private readonly WeakList<WorkingBeatmap> workingWeakReferences = new WeakList<WorkingBeatmap>();
        private readonly WeakList<Player> playerWeakReferences = new WeakList<Player>();

        private Player loadPlayerFor(RulesetInfo ri)
        {
            Ruleset.Value = ri;
            return loadPlayerFor(ri.CreateInstance());
        }

        private Player loadPlayerFor(Ruleset r)
        {
            var beatmap = CreateBeatmap(r);
            var working = new TestWorkingBeatmap(beatmap);

            workingWeakReferences.Add(working);

            Beatmap.Value = working;
            Beatmap.Value.Mods.Value = new[] { r.GetAllMods().First(m => m is ModNoFail) };

            Player?.Exit();

            var player = CreatePlayer(r);

            playerWeakReferences.Add(player);

            LoadComponentAsync(player, p =>
            {
                Player = p;
                LoadScreen(p);
            });

            return player;
        }

        protected override void Update()
        {
            base.Update();

            // note that this will override any mod rate application
            Beatmap.Value.Track.Rate = Clock.Rate;
        }

        protected virtual Player CreatePlayer(Ruleset ruleset) => new Player
        {
            AllowPause = false,
            AllowLeadIn = false,
            AllowResults = false,
        };
    }
}
