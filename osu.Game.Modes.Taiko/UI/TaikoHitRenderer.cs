// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Modes.Objects;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Taiko.Objects.Drawables;
using osu.Game.Modes.Taiko.Objects;
using osu.Game.Modes.UI;
using osu.Framework.Graphics;
using OpenTK;
using osu.Game.Beatmaps;
using osu.Framework.Allocation;
using osu.Game.Beatmaps.Timing;
using System.Collections.Generic;

namespace osu.Game.Modes.Taiko.UI
{
    public class TaikoHitRenderer : HitRenderer<TaikoHitObject>
    {
        protected override HitObjectConverter<TaikoHitObject> Converter => new TaikoConverter();

        private Beatmap beatmap;

        public TaikoHitRenderer(Beatmap beatmap)
            : base(beatmap)
        {
            this.beatmap = beatmap;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            computeBarLines();
        }

        protected override Playfield<TaikoHitObject> CreatePlayfield() => new TaikoPlayfield()
        {
            RelativePositionAxes = Axes.Y,
            Position = new Vector2(0, 0.4f)
        };

        protected override DrawableHitObject<TaikoHitObject> GetVisualRepresentation(TaikoHitObject h)
        {
            if ((h.Type & TaikoHitType.HitCircle) > 0)
            {
                if ((h.Type & TaikoHitType.Don) > 0)
                {
                    if ((h.Type & TaikoHitType.Finisher) > 0)
                        return new DrawableHitCircleDonFinisher(h as HitCircle);
                    return new DrawableHitCircleDon(h as HitCircle);
                }
                else if ((h.Type & TaikoHitType.Katsu) > 0)
                {
                    if ((h.Type & TaikoHitType.Finisher) > 0)
                        return new DrawableHitCircleKatsuFinisher(h as HitCircle);
                    return new DrawableHitCircleKatsu(h as HitCircle);
                }
            }
            else if ((h.Type & TaikoHitType.DrumRoll) > 0)
            {
                if ((h.Type & TaikoHitType.Finisher) > 0)
                    return new DrawableDrumRollFinisher(h as DrumRoll);
                return new DrawableDrumRoll(h as DrumRoll);
            }
            else if ((h.Type & TaikoHitType.Bash) > 0)
                return new DrawableBash(h as Bash);

            return null;
        }

        private void computeBarLines()
        {
            double lastHitTime = beatmap.HitObjects[beatmap.HitObjects.Count - 1].EndTime + 1;

            List<ControlPoint> timingPoints = beatmap.ControlPoints?.FindAll(cp => cp.TimingChange);

            if (timingPoints == null || timingPoints.Count == 0)
                return;

            int currentIndex = 0;

            while (currentIndex < timingPoints.Count && timingPoints[currentIndex].BeatLength == 0)
                currentIndex++;

            double time = timingPoints[currentIndex].Time;
            double measureLength = timingPoints[currentIndex].BeatLength * (int)timingPoints[currentIndex].TimeSignature;

            // Find the bar line time closest to 0
            time -= measureLength * (int)(time / measureLength);

            // Always start barlines from a positive time
            while (time < 0)
                time += measureLength;

            double lastBeatLength = timingPoints[currentIndex].BeatLength;
            int currentBeat = 0;
            while (time <= lastHitTime)
            {
                ControlPoint current = timingPoints[currentIndex];

                if (time > current.Time || !current.OmitFirstBarLine)
                {
                    BarLine barLine = new BarLine()
                    {
                        StartTime = time
                    };

                    barLine.SetDefaultsFromBeatmap(beatmap);

                    addBarLine(barLine, currentBeat % (int)current.TimeSignature == 0);

                    currentBeat++;
                }

                double bl = current.BeatLength;

                if (bl < 800)
                    bl *= (int)current.TimeSignature;

                time += bl;

                if (currentIndex + 1 < timingPoints.Count && time >= timingPoints[currentIndex + 1].Time)
                {
                    currentIndex++;
                    time = timingPoints[currentIndex].Time;

                    currentBeat = 0;
                }
            }
        }

        private void addBarLine(BarLine barLine, bool major)
        {
            TaikoPlayfield tp = Playfield as TaikoPlayfield;
            if (major)
                tp.AddBarLine(new DrawableMajorBarLine(barLine));
            else
                tp.AddBarLine(new DrawableBarLine(barLine));
        }
    }
}
