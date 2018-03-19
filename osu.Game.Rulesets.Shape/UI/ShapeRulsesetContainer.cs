using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.Shape.Judgements;
using osu.Game.Rulesets.Shape.Objects;
using osu.Game.Rulesets.Shape.Objects.Drawables;
using osu.Game.Rulesets.Shape.Beatmaps;
using osu.Game.Rulesets.Shape.UI;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using OpenTK;
using osu.Game.Rulesets.Shape.Scoring;
using osu.Framework.Input;
using System;

namespace osu.Game.Rulesets.Shape
{
    internal class ShapeRulesetContainer : RulesetContainer<ShapeHitObject>
    {
        public ShapeRulesetContainer(Ruleset ruleset, WorkingBeatmap beatmap, bool isForCurrentRuleset)
            : base(ruleset, beatmap, isForCurrentRuleset)
        {
        }

        public override ScoreProcessor CreateScoreProcessor() => new ShapeScoreProcessor(this);

        protected override BeatmapConverter<ShapeHitObject> CreateBeatmapConverter() => new ShapeBeatmapConverter();

        protected override BeatmapProcessor<ShapeHitObject> CreateBeatmapProcessor() => new ShapeBeatmapProcessor();

        protected override Playfield CreatePlayfield() => new ShapePlayfield();

        public override PassThroughInputManager CreateInputManager() => new ShapeInputManager(Ruleset.RulesetInfo);

        protected override DrawableHitObject<ShapeHitObject> GetVisualRepresentation(ShapeHitObject h)
        {
            var shape = h as BaseShape;
            if (shape != null)
                return new DrawableBaseShape(shape);
            return null;
        }

        protected override Vector2 GetAspectAdjustedSize() => new Vector2(0.75f);
    }
}
