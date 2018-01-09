// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Input;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawable;
using osu.Game.Rulesets.Catch.Scoring;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.UI
{
    public class CatchRulesetContainer : ScrollingRulesetContainer<CatchPlayfield, CatchHitObject>
    {
        public CatchRulesetContainer(Ruleset ruleset, WorkingBeatmap beatmap, bool isForCurrentRuleset)
            : base(ruleset, beatmap, isForCurrentRuleset)
        {
        }

        public override ScoreProcessor CreateScoreProcessor() => new CatchScoreProcessor(this);

        protected override BeatmapProcessor<CatchHitObject> CreateBeatmapProcessor() => new CatchBeatmapProcessor();

        protected override BeatmapConverter<CatchHitObject> CreateBeatmapConverter() => new CatchBeatmapConverter();

        protected override Playfield CreatePlayfield() => new CatchPlayfield(Beatmap.BeatmapInfo.BaseDifficulty);

        public override PassThroughInputManager CreateInputManager() => new CatchInputManager(Ruleset.RulesetInfo);

        protected override DrawableHitObject<CatchHitObject> GetVisualRepresentation(CatchHitObject h)
        {
            var fruit = h as Fruit;
            if (fruit != null)
                return new DrawableFruit(fruit);

            var stream = h as JuiceStream;
            if (stream != null)
                return new DrawableJuiceStream(stream);

            return null;
        }
    }
}
