// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Play;
using osu.Game.Tests.Beatmaps;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual
{
    public abstract class PlayerTestCase : RateAdjustedBeatmapTestCase
    {
        private readonly Ruleset ruleset;

        protected Player Player;

        protected PlayerTestCase(Ruleset ruleset)
        {
            this.ruleset = ruleset;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Color4.Black,
                Depth = int.MaxValue
            });
        }

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep(ruleset.RulesetInfo.Name, loadPlayer);
            AddUntilStep(() => Player.IsLoaded, "player loaded");
        }

        protected virtual IBeatmap CreateBeatmap(Ruleset ruleset) => new TestBeatmap(ruleset.RulesetInfo);

        protected virtual bool AllowFail => false;

        private void loadPlayer()
        {
            var beatmap = CreateBeatmap(ruleset);

            Beatmap.Value = new TestWorkingBeatmap(beatmap, Clock);

            if (!AllowFail)
                Beatmap.Value.Mods.Value = new[] { ruleset.GetAllMods().First(m => m is ModNoFail) };

            Player = CreatePlayer(ruleset);
            LoadScreen(Player);
        }

        protected virtual Player CreatePlayer(Ruleset ruleset) => new Player(false, false);
    }
}
