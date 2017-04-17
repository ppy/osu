// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Objects.Types;
using osu.Game.Modes.Replays;
using osu.Game.Modes.Scoring;
using osu.Game.Modes.Taiko.Beatmaps;
using osu.Game.Modes.Taiko.Judgements;
using osu.Game.Modes.Taiko.Objects;
using osu.Game.Modes.Taiko.Objects.Drawables;
using osu.Game.Modes.Taiko.Scoring;
using osu.Game.Modes.UI;
using osu.Game.Modes.Taiko.Replays;
using OpenTK;

namespace osu.Game.Modes.Taiko.UI
{
    public class TaikoHitRenderer : HitRenderer<TaikoHitObject, TaikoJudgement>
    {
        public TaikoHitRenderer(WorkingBeatmap beatmap)
            : base(beatmap)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            loadBarLines();
        }

        private void loadBarLines()
        {
            var taikoPlayfield = Playfield as TaikoPlayfield;

            if (taikoPlayfield == null)
                return;

            TaikoHitObject lastObject = Beatmap.HitObjects[Beatmap.HitObjects.Count - 1];
            double lastHitTime = 1 + (lastObject as IHasEndTime)?.EndTime ?? lastObject.StartTime;

            var timingPoints = Beatmap.TimingInfo.ControlPoints.FindAll(cp => cp.TimingChange);

            if (timingPoints.Count == 0)
                return;

            int currentTimingPoint = 0;
            int currentBeat = 0;
            double time = timingPoints[currentTimingPoint].Time;
            while (time <= lastHitTime)
            {             
                int nextTimingPoint = currentTimingPoint + 1;
                if (nextTimingPoint < timingPoints.Count && time > timingPoints[nextTimingPoint].Time)
                {
                    currentTimingPoint = nextTimingPoint;
                    time = timingPoints[currentTimingPoint].Time;
                    currentBeat = 0;
                }

                var currentPoint = timingPoints[currentTimingPoint];

                var barLine = new BarLine
                {
                    StartTime = time,
                };

                barLine.ApplyDefaults(Beatmap.TimingInfo, Beatmap.BeatmapInfo.Difficulty);

                bool isMajor = currentBeat % (int)currentPoint.TimeSignature == 0;
                taikoPlayfield.AddBarLine(isMajor ? new DrawableBarLineMajor(barLine) : new DrawableBarLine(barLine));

                double bl = currentPoint.BeatLength;
                if (bl < 800)
                    bl *= (int)currentPoint.TimeSignature;

                time += bl;
                currentBeat++;
            }
        }

        protected override Vector2 GetPlayfieldAspectAdjust()
        {
            const float default_relative_height = TaikoPlayfield.DEFAULT_PLAYFIELD_HEIGHT / 768;
            const float default_aspect = 16f / 9f;

            float aspectAdjust = MathHelper.Clamp(DrawWidth / DrawHeight, 0.4f, 4) / default_aspect;

            return new Vector2(1, default_relative_height * aspectAdjust);
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
                    return new DrawableCentreHitStrong(centreHit);
                return new DrawableCentreHit(centreHit);
            }

            var rimHit = h as RimHit;
            if (rimHit != null)
            {
                if (h.IsStrong)
                    return new DrawableRimHitStrong(rimHit);
                return new DrawableRimHit(rimHit);
            }

            var drumRoll = h as DrumRoll;
            if (drumRoll != null)
            {
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
