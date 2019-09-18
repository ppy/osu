// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Input;
using osu.Game.Beatmaps;
using osu.Game.Input.Handlers;
using osu.Game.Replays;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Configuration;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;

namespace osu.Game.Rulesets.Osu.UI
{
    public class DrawableOsuRuleset : DrawableRuleset<OsuHitObject>
    {
        protected new OsuRulesetConfigManager Config => (OsuRulesetConfigManager)base.Config;

        public DrawableOsuRuleset(Ruleset ruleset, IWorkingBeatmap beatmap, IReadOnlyList<Mod> mods)
            : base(ruleset, beatmap, mods)
        {
        }

        public override ScoreProcessor CreateScoreProcessor() => new OsuScoreProcessor(this);

        protected override Playfield CreatePlayfield() => new OsuPlayfield();

        protected override PassThroughInputManager CreateInputManager() => new OsuInputManager(Ruleset.RulesetInfo);

        public override PlayfieldAdjustmentContainer CreatePlayfieldAdjustmentContainer() => new OsuPlayfieldAdjustmentContainer();

        protected override ResumeOverlay CreateResumeOverlay() => new OsuResumeOverlay();

        public override DrawableHitObject<OsuHitObject> CreateDrawableRepresentation(OsuHitObject h)
        {
            switch (h)
            {
                case HitCircle circle:
                    return new DrawableHitCircle(circle);

                case Slider slider:
                    return new DrawableSlider(slider);

                case Spinner spinner:
                    return new DrawableSpinner(spinner);
            }

            return null;
        }

        protected override ReplayInputHandler CreateReplayInputHandler(Replay replay) => new OsuFramedReplayInputHandler(replay);

        public override double GameplayStartTime
        {
            get
            {
                if (Objects.FirstOrDefault() is OsuHitObject first)
                    return first.StartTime - Math.Max(2000, first.TimePreempt);

                return 0;
            }
        }
    }
}
