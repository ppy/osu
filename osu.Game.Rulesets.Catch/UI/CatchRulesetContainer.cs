﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Input;
using osu.Game.Beatmaps;
using osu.Game.Input.Handlers;
using osu.Game.Rulesets.Catch.Beatmaps;
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
        public CatchRulesetContainer(Ruleset ruleset, WorkingBeatmap beatmap, bool isForCurrentRuleset)
            : base(ruleset, beatmap, isForCurrentRuleset)
        {
        }

        public override ScoreProcessor CreateScoreProcessor() => new CatchScoreProcessor(this);

        protected override ReplayInputHandler CreateReplayInputHandler(Replay replay) => new CatchFramedReplayInputHandler(replay);

        protected override BeatmapProcessor<CatchHitObject> CreateBeatmapProcessor() => new CatchBeatmapProcessor();

        protected override BeatmapConverter<CatchHitObject> CreateBeatmapConverter() => new CatchBeatmapConverter();

        protected override Playfield CreatePlayfield() => new CatchPlayfield(Beatmap.BeatmapInfo.BaseDifficulty, GetVisualRepresentation);

        public override PassThroughInputManager CreateInputManager() => new CatchInputManager(Ruleset.RulesetInfo);

        protected override DrawableHitObject<CatchHitObject> GetVisualRepresentation(CatchHitObject h)
        {
            switch (h)
            {
                case Fruit fruit:
                    return new DrawableFruit(fruit);
                case JuiceStream stream:
                    return new DrawableJuiceStream(stream, GetVisualRepresentation);
                case BananaShower banana:
                    return new DrawableBananaShower(banana, GetVisualRepresentation);
                case TinyDroplet tiny:
                    return new DrawableDroplet(tiny) { Scale = new Vector2(0.5f) };
                case Droplet droplet:
                    return new DrawableDroplet(droplet);
            }

            return null;
        }
    }
}
