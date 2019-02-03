// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Input;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Input.Handlers;
using osu.Game.Replays;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawable;
using osu.Game.Rulesets.Catch.Replays;
using osu.Game.Rulesets.Catch.Scoring;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;

namespace osu.Game.Rulesets.Catch.UI
{
    public class CatchRulesetContainer : ScrollingRulesetContainer<CatchPlayfield, CatchHitObject>
    {
        protected override ScrollVisualisationMethod VisualisationMethod => ScrollVisualisationMethod.Constant;

        protected override bool UserScrollSpeedAdjustment => false;

        public CatchRulesetContainer(Ruleset ruleset, WorkingBeatmap beatmap)
            : base(ruleset, beatmap)
        {
            Direction.Value = ScrollingDirection.Down;
            TimeRange.Value = BeatmapDifficulty.DifficultyRange(beatmap.BeatmapInfo.BaseDifficulty.ApproachRate, 1800, 1200, 450);
        }

        public override ScoreProcessor CreateScoreProcessor() => new CatchScoreProcessor(this);

        protected override ReplayInputHandler CreateReplayInputHandler(Replay replay) => new CatchFramedReplayInputHandler(replay);

        protected override Playfield CreatePlayfield() => new CatchPlayfield(Beatmap.BeatmapInfo.BaseDifficulty, GetVisualRepresentation);

        public override PassThroughInputManager CreateInputManager() => new CatchInputManager(Ruleset.RulesetInfo);

        public override DrawableHitObject<CatchHitObject> GetVisualRepresentation(CatchHitObject h)
        {
            switch (h)
            {
                case Banana banana:
                    return new DrawableBanana(banana);
                case Fruit fruit:
                    return new DrawableFruit(fruit);
                case JuiceStream stream:
                    return new DrawableJuiceStream(stream, GetVisualRepresentation);
                case BananaShower shower:
                    return new DrawableBananaShower(shower, GetVisualRepresentation);
                case TinyDroplet tiny:
                    return new DrawableTinyDroplet(tiny);
                case Droplet droplet:
                    return new DrawableDroplet(droplet);
            }

            return null;
        }
    }
}
