// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Osu.Beatmaps;
using osu.Game.Modes.Osu.Judgements;
using osu.Game.Modes.Osu.Objects;
using osu.Game.Modes.Osu.Objects.Drawables;
using osu.Game.Modes.UI;
using osu.Game.Screens.Play;

namespace osu.Game.Modes.Osu.UI
{
    public class OsuHitRenderer : HitRenderer<OsuHitObject, OsuJudgementInfo>
    {
        public OsuHitRenderer(WorkingBeatmap beatmap)
            : base(beatmap)
        {
        }

        public override ScoreProcessor CreateScoreProcessor() => new OsuScoreProcessor(this);

        protected override IBeatmapConverter<OsuHitObject> CreateBeatmapConverter() => new OsuBeatmapConverter();

        protected override IBeatmapProcessor<OsuHitObject> CreateBeatmapProcessor() => new OsuBeatmapProcessor();

        protected override Playfield<OsuHitObject, OsuJudgementInfo> CreatePlayfield() => new OsuPlayfield();

        protected override KeyConversionInputManager CreateKeyConversionInputManager() => new OsuKeyConversionInputManager();

        protected override DrawableHitObject<OsuHitObject, OsuJudgementInfo> GetVisualRepresentation(OsuHitObject h)
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
    }
}
