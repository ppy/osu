// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.ComponentModel;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual
{
    [Description("Player instantiated with an autoplay mod.")]
    public class TestCaseAutoplay : TestCasePlayer
    {
        protected override Player CreatePlayer(WorkingBeatmap beatmap, Ruleset ruleset)
        {
            beatmap.Mods.Value = beatmap.Mods.Value.Concat(new[] { ruleset.GetAutoplayMod() });
            return new ScoreAccessiblePlayer
            {
                InitialBeatmap = beatmap,
                AllowPause = false,
                AllowLeadIn = false,
                AllowResults = false,
            };
        }

        protected override bool ContinueCondition(Player player) => base.ContinueCondition(player) &&  ((ScoreAccessiblePlayer)player).ScoreProcessor.TotalScore > 0;

        private class ScoreAccessiblePlayer : Player
        {
            public new ScoreProcessor ScoreProcessor => base.ScoreProcessor;
        }
    }
}
