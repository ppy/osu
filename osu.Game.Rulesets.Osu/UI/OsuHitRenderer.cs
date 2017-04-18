// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Beatmaps;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;

namespace osu.Game.Rulesets.Osu.UI
{
    public class OsuHitRenderer : HitRenderer<OsuHitObject, OsuJudgement>
    {
        public OsuHitRenderer(WorkingBeatmap beatmap)
            : base(beatmap)
        {
        }

        public override ScoreProcessor CreateScoreProcessor() => new OsuScoreProcessor(this);

        protected override BeatmapConverter<OsuHitObject> CreateBeatmapConverter() => new OsuBeatmapConverter();

        protected override BeatmapProcessor<OsuHitObject> CreateBeatmapProcessor() => new OsuBeatmapProcessor();

        protected override Playfield<OsuHitObject, OsuJudgement> CreatePlayfield() => new OsuPlayfield();

        protected override KeyConversionInputManager CreateKeyConversionInputManager() => new OsuKeyConversionInputManager();

        protected override DrawableHitObject<OsuHitObject, OsuJudgement> GetVisualRepresentation(OsuHitObject h)
        {
            var circle = h as HitCircle;
            if (circle != null)
                return new DrawableHitCircle(circle);

            var slider = h as Slider;
            if (slider != null)
                return new DrawableSlider(slider);

            var spinner = h as Spinner;
            if (spinner != null)
                return new DrawableSpinner(spinner);
            return null;
        }

        protected override Vector2 GetPlayfieldAspectAdjust() => new Vector2(0.75f);
    }
}
