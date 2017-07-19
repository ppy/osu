// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTK.Input;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Lists;
using osu.Framework.MathUtils;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Beatmaps;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Judgements;
using osu.Game.Rulesets.Mania.Mods;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Mania.Scoring;
using osu.Game.Rulesets.Mania.Timing;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Timing;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Mania.UI
{
    public class ManiaHitRenderer : SpeedAdjustedHitRenderer<ManiaHitObject, ManiaJudgement>
    {
        /// <summary>
        /// Preferred column count. This will only have an effect during the initialization of the play field.
        /// </summary>
        public int PreferredColumns;

        public IEnumerable<DrawableBarLine> BarLines;

        /// <summary>
        /// Per-column timing changes.
        /// </summary>
        private readonly List<SpeedAdjustmentContainer>[] hitObjectSpeedAdjustments;

        /// <summary>
        /// Bar line timing changes.
        /// </summary>
        private readonly List<SpeedAdjustmentContainer> barLineSpeedAdjustments = new List<SpeedAdjustmentContainer>();

        public ManiaHitRenderer(WorkingBeatmap beatmap, bool isForCurrentRuleset)
            : base(beatmap, isForCurrentRuleset)
        {
            // Generate the speed adjustment container lists
            hitObjectSpeedAdjustments = new List<SpeedAdjustmentContainer>[PreferredColumns];
            for (int i = 0; i < PreferredColumns; i++)
                hitObjectSpeedAdjustments[i] = new List<SpeedAdjustmentContainer>();

            // Generate the bar lines
            double lastObjectTime = (Objects.LastOrDefault() as IHasEndTime)?.EndTime ?? Objects.LastOrDefault()?.StartTime ?? double.MaxValue;

            SortedList<TimingControlPoint> timingPoints = Beatmap.ControlPointInfo.TimingPoints;
            var barLines = new List<DrawableBarLine>();

            for (int i = 0; i < timingPoints.Count; i++)
            {
                TimingControlPoint point = timingPoints[i];

                // Stop on the beat before the next timing point, or if there is no next timing point stop slightly past the last object
                double endTime = i < timingPoints.Count - 1 ? timingPoints[i + 1].Time - point.BeatLength : lastObjectTime + point.BeatLength * (int)point.TimeSignature;

                int index = 0;
                for (double t = timingPoints[i].Time; Precision.DefinitelyBigger(endTime, t); t += point.BeatLength, index++)
                {
                    barLines.Add(new DrawableBarLine(new BarLine
                    {
                        StartTime = t,
                        ControlPoint = point,
                        BeatIndex = index
                    }));
                }
            }

            BarLines = barLines;

            // Generate speed adjustments from mods first
            bool useDefaultSpeedAdjustments = true;

            if (Mods != null)
            {
                foreach (var speedAdjustmentMod in Mods.OfType<IGenerateSpeedAdjustments>())
                {
                    useDefaultSpeedAdjustments = false;
                    speedAdjustmentMod.ApplyToHitRenderer(this, ref hitObjectSpeedAdjustments, ref barLineSpeedAdjustments);
                }
            }

            // Generate the default speed adjustments
            if (useDefaultSpeedAdjustments)
                generateDefaultSpeedAdjustments();
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var maniaPlayfield = (ManiaPlayfield)Playfield;

            BarLines.ForEach(maniaPlayfield.Add);
        }

        protected override void ApplyBeatmap()
        {
            base.ApplyBeatmap();

            PreferredColumns = (int)Math.Max(1, Math.Round(Beatmap.BeatmapInfo.Difficulty.CircleSize));
        }

        protected override void ApplySpeedAdjustments()
        {
            var maniaPlayfield = (ManiaPlayfield)Playfield;

            for (int i = 0; i < PreferredColumns; i++)
                foreach (var change in hitObjectSpeedAdjustments[i])
                    maniaPlayfield.Columns.ElementAt(i).Add(change);

            foreach (var change in barLineSpeedAdjustments)
                maniaPlayfield.Add(change);
        }

        private void generateDefaultSpeedAdjustments()
        {
            DefaultControlPoints.ForEach(c =>
            {
                foreach (List<SpeedAdjustmentContainer> t in hitObjectSpeedAdjustments)
                    t.Add(new ManiaSpeedAdjustmentContainer(c, ScrollingAlgorithm.Basic));
                barLineSpeedAdjustments.Add(new ManiaSpeedAdjustmentContainer(c, ScrollingAlgorithm.Basic));
            });
        }

        protected sealed override Playfield<ManiaHitObject, ManiaJudgement> CreatePlayfield() => new ManiaPlayfield(PreferredColumns)
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            // Invert by default for now (should be moved to config/skin later)
            Scale = new Vector2(1, -1)
        };

        public override ScoreProcessor CreateScoreProcessor() => new ManiaScoreProcessor(this);

        protected override BeatmapConverter<ManiaHitObject> CreateBeatmapConverter() => new ManiaBeatmapConverter();

        protected override DrawableHitObject<ManiaHitObject, ManiaJudgement> GetVisualRepresentation(ManiaHitObject h)
        {
            var maniaPlayfield = (ManiaPlayfield)Playfield;

            Bindable<Key> key = maniaPlayfield.Columns.ElementAt(h.Column).Key;

            var holdNote = h as HoldNote;
            if (holdNote != null)
                return new DrawableHoldNote(holdNote, key);

            var note = h as Note;
            if (note != null)
                return new DrawableNote(note, key);

            return null;
        }

        protected override Vector2 GetPlayfieldAspectAdjust() => new Vector2(1, 0.8f);
    }
}
