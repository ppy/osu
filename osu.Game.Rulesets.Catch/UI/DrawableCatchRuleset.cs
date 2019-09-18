// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Input;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Input.Handlers;
using osu.Game.Replays;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawable;
using osu.Game.Rulesets.Catch.Replays;
using osu.Game.Rulesets.Catch.Scoring;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;

namespace osu.Game.Rulesets.Catch.UI
{
    public class DrawableCatchRuleset : DrawableScrollingRuleset<CatchHitObject>
    {
        protected override ScrollVisualisationMethod VisualisationMethod => ScrollVisualisationMethod.Constant;

        protected override bool UserScrollSpeedAdjustment => false;

        public DrawableCatchRuleset(Ruleset ruleset, IWorkingBeatmap beatmap, IReadOnlyList<Mod> mods)
            : base(ruleset, beatmap, mods)
        {
            Direction.Value = ScrollingDirection.Down;
            TimeRange.Value = BeatmapDifficulty.DifficultyRange(beatmap.Beatmap.BeatmapInfo.BaseDifficulty.ApproachRate, 1800, 1200, 450);
        }

        public override ScoreProcessor CreateScoreProcessor() => new CatchScoreProcessor(this);

        protected override ReplayInputHandler CreateReplayInputHandler(Replay replay) => new CatchFramedReplayInputHandler(replay);

        protected override Playfield CreatePlayfield() => new CatchPlayfield(Beatmap.BeatmapInfo.BaseDifficulty, CreateDrawableRepresentation);

        public override PlayfieldAdjustmentContainer CreatePlayfieldAdjustmentContainer() => new CatchPlayfieldAdjustmentContainer();

        protected override PassThroughInputManager CreateInputManager() => new CatchInputManager(Ruleset.RulesetInfo);

        public override DrawableHitObject<CatchHitObject> CreateDrawableRepresentation(CatchHitObject h)
        {
            switch (h)
            {
                case Banana banana:
                    return new DrawableBanana(banana);

                case Fruit fruit:
                    return new DrawableFruit(fruit);

                case JuiceStream stream:
                    return new DrawableJuiceStream(stream, CreateDrawableRepresentation);

                case BananaShower shower:
                    return new DrawableBananaShower(shower, CreateDrawableRepresentation);

                case TinyDroplet tiny:
                    return new DrawableTinyDroplet(tiny);

                case Droplet droplet:
                    return new DrawableDroplet(droplet);
            }

            return null;
        }
    }
}
