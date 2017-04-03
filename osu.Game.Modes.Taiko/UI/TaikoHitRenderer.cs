// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Framework.Graphics;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Scoring;
using osu.Game.Modes.Taiko.Beatmaps;
using osu.Game.Modes.Taiko.Judgements;
using osu.Game.Modes.Taiko.Objects;
using osu.Game.Modes.Taiko.Objects.Drawable;
using osu.Game.Modes.Taiko.Scoring;
using osu.Game.Modes.UI;
using osu.Game.Modes.Replays;
using osu.Game.Modes.Taiko.Replays;

namespace osu.Game.Modes.Taiko.UI
{
    public class TaikoHitRenderer : HitRenderer<TaikoHitObject, TaikoJudgement>
    {
        public TaikoHitRenderer(WorkingBeatmap beatmap)
            : base(beatmap)
        {
        }

        public override ScoreProcessor CreateScoreProcessor() => new TaikoScoreProcessor(this);

        protected override IBeatmapConverter<TaikoHitObject> CreateBeatmapConverter() => new TaikoBeatmapConverter();

        protected override IBeatmapProcessor<TaikoHitObject> CreateBeatmapProcessor() => new TaikoBeatmapProcessor();

        protected override Playfield<TaikoHitObject, TaikoJudgement> CreatePlayfield() => new TaikoPlayfield
        {
            Anchor = Anchor.CentreLeft,
            Origin = Anchor.CentreLeft
        };

        protected override DrawableHitObject<TaikoHitObject, TaikoJudgement> GetVisualRepresentation(TaikoHitObject h)
        {
            var centreHit = h as CentreHit;
            if (centreHit != null)
            {
                if (h.IsStrong)
                    return new DrawableStrongCentreHit(centreHit);
                return new DrawableCentreHit(centreHit);
            }

            var rimHit = h as RimHit;
            if (rimHit != null)
            {
                if (h.IsStrong)
                    return new DrawableStrongRimHit(rimHit);
                return new DrawableRimHit(rimHit);
            }

            var drumRoll = h as DrumRoll;
            if (drumRoll != null)
            {
                if (h.IsStrong)
                    return new DrawableStrongDrumRoll(drumRoll);
                return new DrawableDrumRoll(drumRoll);
            }

            var swell = h as Swell;
            if (swell != null)
                return new DrawableSwell(swell);

            return null;
        }

        protected override FramedReplayInputHandler CreateReplayInputHandler(Replay replay) => new TaikoFramedReplayInputHandler(replay);
    }
}
