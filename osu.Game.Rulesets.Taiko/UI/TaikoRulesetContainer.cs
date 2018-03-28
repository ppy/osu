// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Beatmaps;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.Taiko.Replays;
using OpenTK;
using System.Linq;
using osu.Framework.Input;
using osu.Game.Input.Handlers;
using osu.Game.Rulesets.UI.Scrolling;

namespace osu.Game.Rulesets.Taiko.UI
{
    public class TaikoRulesetContainer : ScrollingRulesetContainer<TaikoPlayfield, TaikoHitObject>
    {
        public TaikoRulesetContainer(Ruleset ruleset, WorkingBeatmap beatmap, bool isForCurrentRuleset)
            : base(ruleset, beatmap, isForCurrentRuleset)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            loadBarLines();
        }

        private void loadBarLines()
        {
            TaikoHitObject lastObject = Beatmap.HitObjects[Beatmap.HitObjects.Count - 1];
            double lastHitTime = 1 + (lastObject as IHasEndTime)?.EndTime ?? lastObject.StartTime;

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

                double bl = currentPoint.BeatLength;
                if (bl < 800)
                    bl *= (int)currentPoint.TimeSignature;

                time += bl;
                currentBeat++;
            }
        }

        protected override Vector2 GetAspectAdjustedSize()
        {
            const float default_relative_height = TaikoPlayfield.DEFAULT_HEIGHT / 768;
            const float default_aspect = 16f / 9f;

            float aspectAdjust = MathHelper.Clamp(DrawWidth / DrawHeight, 0.4f, 4) / default_aspect;

            return new Vector2(1, default_relative_height * aspectAdjust);
        }

        protected override Vector2 PlayfieldArea => Vector2.One;

        public override ScoreProcessor CreateScoreProcessor() => new TaikoScoreProcessor(this);

        protected override BeatmapConverter<TaikoHitObject> CreateBeatmapConverter() => new TaikoBeatmapConverter(IsForCurrentRuleset);

        public override PassThroughInputManager CreateInputManager() => new TaikoInputManager(Ruleset.RulesetInfo);

        protected override Playfield CreatePlayfield() => new TaikoPlayfield(Beatmap.ControlPointInfo)
        {
            Anchor = Anchor.CentreLeft,
            Origin = Anchor.CentreLeft
        };

        protected override DrawableHitObject<TaikoHitObject> GetVisualRepresentation(TaikoHitObject h)
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

        protected override ReplayInputHandler CreateReplayInputHandler(Replay replay) => new TaikoFramedReplayInputHandler(replay);
    }
}
