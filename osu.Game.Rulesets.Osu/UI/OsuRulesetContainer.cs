// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Input;
using OpenTK;
using osu.Game.Beatmaps;
using osu.Game.Input.Handlers;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Rulesets.Osu.UI.Cursor;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.Replays;

namespace osu.Game.Rulesets.Osu.UI
{
    public class OsuRulesetContainer : RulesetContainer<OsuPlayfield, OsuHitObject>
    {
        public OsuRulesetContainer(Ruleset ruleset, WorkingBeatmap beatmap)
            : base(ruleset, beatmap)
        {
        }

        public override ScoreProcessor CreateScoreProcessor() => new OsuScoreProcessor(this);

        protected override Playfield CreatePlayfield() => new OsuPlayfield();

        public override PassThroughInputManager CreateInputManager() => new OsuInputManager(Ruleset.RulesetInfo);

        protected override DrawableHitObject<OsuHitObject> GetVisualRepresentation(OsuHitObject h)
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

        protected override ReplayInputHandler CreateReplayInputHandler(Replay replay) => new OsuReplayInputHandler(replay);

        public override double GameplayStartTime
        {
            get
            {
                var first = (OsuHitObject)Objects.First();
                return first.StartTime - first.TimePreempt;
            }
        }

        protected override Vector2 GetAspectAdjustedSize()
        {
            var aspectSize = DrawSize.X * 0.75f < DrawSize.Y ? new Vector2(DrawSize.X, DrawSize.X * 0.75f) : new Vector2(DrawSize.Y * 4f / 3f, DrawSize.Y);
            return new Vector2(aspectSize.X / DrawSize.X, aspectSize.Y / DrawSize.Y);
        }

        protected override CursorContainer CreateCursor() => new GameplayCursor();
    }
}
