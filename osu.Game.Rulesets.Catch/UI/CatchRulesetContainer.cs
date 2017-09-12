// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Input;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Beatmaps;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawable;
using osu.Game.Rulesets.Catch.Scoring;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.UI
{
    public class CatchRulesetContainer : ScrollingRulesetContainer<CatchPlayfield, CatchBaseHit>
    {
        public CatchRulesetContainer(Ruleset ruleset, WorkingBeatmap beatmap, bool isForCurrentRuleset)
            : base(ruleset, beatmap, isForCurrentRuleset)
        {
        }

        public override ScoreProcessor CreateScoreProcessor() => new CatchScoreProcessor(this);

        protected override BeatmapConverter<CatchBaseHit> CreateBeatmapConverter() => new CatchBeatmapConverter();

        protected override Playfield CreatePlayfield() => new CatchPlayfield();

        public override PassThroughInputManager CreateInputManager() => new CatchInputManager(Ruleset.RulesetInfo);

        protected override DrawableHitObject<CatchBaseHit> GetVisualRepresentation(CatchBaseHit h)
        {
            if (h is Fruit)
                return new DrawableFruit(h);

            return null;
        }
    }
}
