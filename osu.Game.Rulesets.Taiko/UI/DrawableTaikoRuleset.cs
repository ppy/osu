// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.Taiko.Replays;
using System.Linq;
using osu.Framework.Input;
using osu.Game.Configuration;
using osu.Game.Input.Handlers;
using osu.Game.Replays;
using osu.Game.Rulesets.UI.Scrolling;

namespace osu.Game.Rulesets.Taiko.UI
{
    public class DrawableTaikoRuleset : DrawableScrollingRuleset<TaikoHitObject>
    {
        protected override ScrollVisualisationMethod VisualisationMethod => ScrollVisualisationMethod.Overlapping;

        protected override bool UserScrollSpeedAdjustment => false;

        public DrawableTaikoRuleset(Ruleset ruleset, WorkingBeatmap beatmap)
            : base(ruleset, beatmap)
        {
            Direction.Value = ScrollingDirection.Left;
            TimeRange.Value = 7000;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            loadBarLines();
        }

        private void loadBarLines()
        {
            TaikoHitObject lastObject = Beatmap.HitObjects[Beatmap.HitObjects.Count - 1];
            double lastHitTime = 1 + ((lastObject as IHasEndTime)?.EndTime ?? lastObject.StartTime);

            var timingPoints = Beatmap.ControlPointInfo.TimingPoints.ToList();

            if (timingPoints.Count == 0)
                return;

            int currentIndex = 0;
            int currentBeat = 0;
            double time = timingPoints[currentIndex].Time;
            while (time <= lastHitTime)
            {
                int nextIndex = currentIndex + 1;
                if (nextIndex < timingPoints.Count && time > timingPoints[nextIndex].Time)
                {
                    currentIndex = nextIndex;
                    time = timingPoints[currentIndex].Time;
                    currentBeat = 0;
                }

                var currentPoint = timingPoints[currentIndex];

                var barLine = new BarLine
                {
                    StartTime = time,
                };

                barLine.ApplyDefaults(Beatmap.ControlPointInfo, Beatmap.BeatmapInfo.BaseDifficulty);

                bool isMajor = currentBeat % (int)currentPoint.TimeSignature == 0;
                Playfield.Add(isMajor ? new DrawableBarLineMajor(barLine) : new DrawableBarLine(barLine));

                time += currentPoint.BeatLength * (int)currentPoint.TimeSignature;
                currentBeat++;
            }
        }

        public override ScoreProcessor CreateScoreProcessor() => new TaikoScoreProcessor(this);

        public override PlayfieldAdjustmentContainer CreatePlayfieldAdjustmentContainer() => new TaikoPlayfieldAdjustmentContainer();

        protected override PassThroughInputManager CreateInputManager() => new TaikoInputManager(Ruleset.RulesetInfo);

        protected override Playfield CreatePlayfield() => new TaikoPlayfield(Beatmap.ControlPointInfo);

        public override DrawableHitObject<TaikoHitObject> CreateDrawableRepresentation(TaikoHitObject h)
        {
            switch (h)
            {
                case CentreHit centreHit:
                    return new DrawableCentreHit(centreHit);
                case RimHit rimHit:
                    return new DrawableRimHit(rimHit);
                case DrumRoll drumRoll:
                    return new DrawableDrumRoll(drumRoll);
                case Swell swell:
                    return new DrawableSwell(swell);
            }

            return null;
        }

        protected override ReplayInputHandler CreateReplayInputHandler(Replay replay) => new TaikoFramedReplayInputHandler(replay);
    }
}
