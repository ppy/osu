// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Input;
using osu.Game.Beatmaps;
using osu.Game.Input.Handlers;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawable;
using osu.Game.Rulesets.Catch.Replays;
using osu.Game.Rulesets.Catch.Scoring;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;
using OpenTK;

namespace osu.Game.Rulesets.Catch.UI
{
    public class CatchRulesetContainer : ScrollingRulesetContainer<CatchPlayfield, CatchHitObject>
    {
        public CatchRulesetContainer(Ruleset ruleset, WorkingBeatmap beatmap)
            : base(ruleset, beatmap)
        {
        }

        public override ScoreProcessor CreateScoreProcessor() => new CatchScoreProcessor(this);

        protected override ReplayInputHandler CreateReplayInputHandler(Replay replay) => new CatchFramedReplayInputHandler(replay);

        protected override Playfield CreatePlayfield() => new CatchPlayfield(Beatmap.BeatmapInfo.BaseDifficulty, GetVisualRepresentation);

        public override PassThroughInputManager CreateInputManager() => new CatchInputManager(Ruleset.RulesetInfo);

        protected override Vector2 PlayfieldArea => new Vector2(0.86f); // matches stable's vertical offset for catcher plate

        protected override DrawableHitObject<CatchHitObject> GetVisualRepresentation(CatchHitObject h)
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
