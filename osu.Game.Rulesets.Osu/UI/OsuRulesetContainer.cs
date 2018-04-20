// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Cursor;
using osu.Framework.Input;
using OpenTK;
using osu.Game.Beatmaps;
using osu.Game.Input.Handlers;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Beatmaps;
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
    public class OsuRulesetContainer : RulesetContainer<OsuHitObject>
    {
        public OsuRulesetContainer(Ruleset ruleset, WorkingBeatmap beatmap, bool isForCurrentRuleset)
            : base(ruleset, beatmap, isForCurrentRuleset)
        {
        }

        public override ScoreProcessor CreateScoreProcessor() => new OsuScoreProcessor(this);

        protected override BeatmapConverter<OsuHitObject> CreateBeatmapConverter() => new OsuBeatmapConverter();

        protected override BeatmapProcessor<OsuHitObject> CreateBeatmapProcessor() => new OsuBeatmapProcessor();

        protected override Playfield CreatePlayfield() => new OsuPlayfield();

        public override PassThroughInputManager CreateInputManager() => new OsuInputManager(Ruleset.RulesetInfo);

        protected override DrawableHitObject<OsuHitObject> GetVisualRepresentation(OsuHitObject h)
        {
            if (h is HitCircle circle)
                return new DrawableHitCircle(circle);

            if (h is Slider slider)
                return new DrawableSlider(slider);

            if (h is Spinner spinner)
                return new DrawableSpinner(spinner);
            return null;
        }

        protected override ReplayInputHandler CreateReplayInputHandler(Replay replay) => new OsuReplayInputHandler(replay);

        protected override Vector2 GetAspectAdjustedSize()
        {
            var aspectSize = DrawSize.X * 0.75f < DrawSize.Y ? new Vector2(DrawSize.X, DrawSize.X * 0.75f) : new Vector2(DrawSize.Y * 4f / 3f, DrawSize.Y);
            return new Vector2(aspectSize.X / DrawSize.X, aspectSize.Y / DrawSize.Y);
        }

        protected override CursorContainer CreateCursor() => new GameplayCursor();
    }
}
