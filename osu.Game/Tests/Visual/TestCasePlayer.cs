// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Play;
using osu.Game.Tests.Beatmaps;
using OpenTK.Graphics;

namespace osu.Game.Tests.Visual
{
    public abstract class TestCasePlayer : ScreenTestCase
    {
        private readonly Ruleset ruleset;

        protected Player Player;

        private TestWorkingBeatmap working;

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
                AddUntilStep(() => p.IsLoaded);
            }
            else
            {
                foreach (var r in rulesets.AvailableRulesets)
                {
                    Player p = null;
                    AddStep(r.Name, () => p = loadPlayerFor(r));
                    AddUntilStep(() => p.IsLoaded);
                }
            }
        }

        protected virtual Beatmap CreateBeatmap(Ruleset ruleset) => new TestBeatmap(ruleset.RulesetInfo);

        private Player loadPlayerFor(RulesetInfo ri) => loadPlayerFor(ri.CreateInstance());

        private Player loadPlayerFor(Ruleset r)
        {
            var beatmap = CreateBeatmap(r);

            working = new TestWorkingBeatmap(beatmap);
            working.Mods.Value = new[] { r.GetAllMods().First(m => m is ModNoFail) };

            if (Player != null)
                Remove(Player);

            var player = CreatePlayer(working, r);

            LoadComponentAsync(player, LoadScreen);

            return player;
        }

        protected override void Update()
        {
            base.Update();

            if (working != null)
                // note that this will override any mod rate application
                working.Track.Rate = Clock.Rate;
        }

        protected virtual Player CreatePlayer(WorkingBeatmap beatmap, Ruleset ruleset) => new Player
        {
            InitialBeatmap = beatmap,
            AllowPause = false,
            AllowLeadIn = false,
            AllowResults = false,
        };
    }
}
