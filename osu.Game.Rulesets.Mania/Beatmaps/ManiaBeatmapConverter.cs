// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Game.Rulesets.Mania.Objects;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Mania.Beatmaps.Patterns;
using osu.Game.Rulesets.Mania.Beatmaps.Patterns.Legacy;
using osu.Game.Rulesets.Scoring.Legacy;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Rulesets.Mania.Beatmaps
{
    public class ManiaBeatmapConverter : BeatmapConverter<ManiaHitObject>
    {
        /// <summary>
        /// Maximum number of previous notes to consider for density calculation.
        /// </summary>
        private const int max_notes_for_density = 7;

        public int TargetColumns;
        public bool Dual;
        public readonly bool IsForCurrentRuleset;

        private readonly int originalTargetColumns;
        private double shortestJack;
        private const double jack_mult = 1.5;
        // Internal for testing purposes
        internal LegacyRandom Random { get; private set; }

        private Pattern lastPattern = new Pattern();

        private ManiaBeatmap beatmap;

        public ManiaBeatmapConverter(IBeatmap beatmap, Ruleset ruleset)
            : base(beatmap, ruleset)
        {
            IsForCurrentRuleset = beatmap.BeatmapInfo.Ruleset.Equals(ruleset.RulesetInfo);
            TargetColumns = GetColumnCount(LegacyBeatmapConversionDifficultyInfo.FromBeatmap(beatmap));

            if (IsForCurrentRuleset && TargetColumns > ManiaRuleset.MAX_STAGE_KEYS)
            {
                TargetColumns /= 2;
                Dual = true;
            }
            originalTargetColumns = TargetColumns;
        }

        public static int GetColumnCount(LegacyBeatmapConversionDifficultyInfo difficulty)
        {
            double roundedCircleSize = Math.Round(difficulty.CircleSize);

            if (difficulty.SourceRuleset.ShortName == ManiaRuleset.SHORT_NAME)
                return (int)Math.Max(1, roundedCircleSize);

            double roundedOverallDifficulty = Math.Round(difficulty.OverallDifficulty);

            if (difficulty.TotalObjectCount > 0 && difficulty.EndTimeObjectCount >= 0)
            {
                int countSliderOrSpinner = difficulty.EndTimeObjectCount;

                // In osu!stable, this division appears as if it happens on floats, but due to release-mode
                // optimisations, it actually ends up happening on doubles.
                double percentSpecialObjects = (double)countSliderOrSpinner / difficulty.TotalObjectCount;

                if (percentSpecialObjects < 0.2)
                    return 7;
                if (percentSpecialObjects < 0.3 || roundedCircleSize >= 5)
                    return roundedOverallDifficulty > 5 ? 7 : 6;
                if (percentSpecialObjects > 0.6)
                    return roundedOverallDifficulty > 4 ? 5 : 4;
            }

            return Math.Max(4, Math.Min((int)roundedOverallDifficulty + 1, 7));
        }

        public override bool CanConvert() => Beatmap.HitObjects.All(h => h is IHasXPosition);

        protected override Beatmap<ManiaHitObject> ConvertBeatmap(IBeatmap original, CancellationToken cancellationToken)
        {
            IBeatmapDifficultyInfo difficulty = original.Difficulty;

            int seed = (int)MathF.Round(difficulty.DrainRate + difficulty.CircleSize) * 20 + (int)(difficulty.OverallDifficulty * 41.2) + (int)MathF.Round(difficulty.ApproachRate);
            Random = new LegacyRandom(seed);

            if (IsForCurrentRuleset)
            {
                int KeyModColumns = TargetColumns;
                TargetColumns = originalTargetColumns;

                var beatmap = (ManiaBeatmap)base.ConvertBeatmap(original, cancellationToken);

                TargetColumns = KeyModColumns;
                convertSpecific(beatmap);

                return beatmap;
            }
            else
            {
                return (ManiaBeatmap)base.ConvertBeatmap(original, cancellationToken);
            }
        }

        protected override Beatmap<ManiaHitObject> CreateBeatmap()
        {
            beatmap = new ManiaBeatmap(new StageDefinition(TargetColumns), originalTargetColumns);

            if (Dual)
                beatmap.Stages.Add(new StageDefinition(TargetColumns));

            return beatmap;
        }

        protected override IEnumerable<ManiaHitObject> ConvertHitObject(HitObject original, IBeatmap beatmap, CancellationToken cancellationToken)
        {
            if (original is ManiaHitObject maniaOriginal)
            {
                yield return maniaOriginal;

                yield break;
            }

            var objects = IsForCurrentRuleset ? generateSpecific(original, beatmap) : generateConverted(original, beatmap);

            if (objects == null)
                yield break;

            foreach (ManiaHitObject obj in objects)
                yield return obj;
        }

        private readonly LimitedCapacityQueue<double> prevNoteTimes = new LimitedCapacityQueue<double>(max_notes_for_density);
        private double density = int.MaxValue;

        private void computeDensity(double newNoteTime)
        {
            prevNoteTimes.Enqueue(newNoteTime);

            if (prevNoteTimes.Count >= 2)
                density = (prevNoteTimes[^1] - prevNoteTimes[0]) / prevNoteTimes.Count;
        }

        private double lastTime;
        private Vector2 lastPosition;
        private PatternType lastStair = PatternType.Stair;

        private void recordNote(double time, Vector2 position)
        {
            lastTime = time;
            lastPosition = position;
        }

        /// <summary>
        /// Method that generates hit objects for osu!mania specific beatmaps.
        /// </summary>
        /// <param name="original">The original hit object.</param>
        /// <param name="originalBeatmap">The original beatmap. This is used to look-up any values dependent on a fully-loaded beatmap.</param>
        /// <returns>The hit objects generated.</returns>
        private IEnumerable<ManiaHitObject> generateSpecific(HitObject original, IBeatmap originalBeatmap)
        {
            var generator = new SpecificBeatmapPatternGenerator(Random, original, beatmap, lastPattern, originalBeatmap);

            foreach (var newPattern in generator.Generate())
            {
                lastPattern = newPattern;

                foreach (var obj in newPattern.HitObjects)
                    yield return obj;
            }
        }

        /// <summary>
        /// Method that generates hit objects for non-osu!mania beatmaps.
        /// </summary>
        /// <param name="original">The original hit object.</param>
        /// <param name="originalBeatmap">The original beatmap. This is used to look-up any values dependent on a fully-loaded beatmap.</param>
        /// <returns>The hit objects generated.</returns>
        private IEnumerable<ManiaHitObject> generateConverted(HitObject original, IBeatmap originalBeatmap)
        {
            Patterns.PatternGenerator conversion = null;

            switch (original)
            {
                case IHasPath:
                {
                    var generator = new PathObjectPatternGenerator(Random, original, beatmap, lastPattern, originalBeatmap);
                    conversion = generator;

                    var positionData = original as IHasPosition;

                    for (int i = 0; i <= generator.SpanCount; i++)
                    {
                        double time = original.StartTime + generator.SegmentDuration * i;

                        recordNote(time, positionData?.Position ?? Vector2.Zero);
                        computeDensity(time);
                    }

                    break;
                }

                case IHasDuration endTimeData:
                {
                    conversion = new EndTimeObjectPatternGenerator(Random, original, beatmap, lastPattern, originalBeatmap);

                    recordNote(endTimeData.EndTime, new Vector2(256, 192));
                    computeDensity(endTimeData.EndTime);
                    break;
                }

                case IHasPosition positionData:
                {
                    computeDensity(original.StartTime);

                    conversion = new HitObjectPatternGenerator(Random, original, beatmap, lastPattern, lastTime, lastPosition, density, lastStair, originalBeatmap);

                    recordNote(original.StartTime, positionData.Position);
                    break;
                }
            }

            if (conversion == null)
                yield break;

            foreach (var newPattern in conversion.Generate())
            {
                lastPattern = conversion is EndTimeObjectPatternGenerator ? lastPattern : newPattern;
                lastStair = (conversion as HitObjectPatternGenerator)?.StairType ?? lastStair;

                foreach (var obj in newPattern.HitObjects)
                    yield return obj;
            }
        }

        /// <summary>
        /// Conversion of osu!mania-specific beatmaps if KeyMod is active.
        /// </summary>
        private void convertSpecific(ManiaBeatmap beatmap)
        {
            while (TargetColumns - beatmap.TotalColumns > 0)
            {
                insertColumn(beatmap);

                int columns = beatmap.TotalColumns;
                beatmap.Stages.Clear();
                beatmap.Stages.Add(new StageDefinition(columns + 1));
            }

            if (TargetColumns - beatmap.TotalColumns < 0)
            {
                var jackMap = getInfo(beatmap);
                while (TargetColumns - beatmap.TotalColumns < 0)
                {
                    reduceColumn(beatmap);

                    int columns = beatmap.TotalColumns;
                    beatmap.Stages.Clear();
                    beatmap.Stages.Add(new StageDefinition(columns - 1));
                }
                reduceHitObjects(beatmap);
                fixHitObjects(beatmap);
                spaceHitObjects(beatmap, jackMap);
            }
        }

        private void insertColumn(ManiaBeatmap beatmap)
        {
            double turnTiming = -1;

            int patternOffset = 1;
            int patternOldOffset = 1;

            int patternMoveDirection = 1;

            foreach (var hitObject in beatmap.HitObjects)
            {
                var edgeHitObject = hitObject;
                if (hitObject.StartTime > turnTiming)
                {
                    int patternToColumn = patternOffset + (patternMoveDirection == 1 ? 0 : -1);

                    var bm = beatmap.HitObjects.Where(x => x.StartTime >= hitObject.StartTime
                                                      && x.Column == patternToColumn)
                                               .ToList();
                    if (bm.Count != 0)
                    {
                        edgeHitObject = bm.First();
                    }

                    //for even distribution of free space
                    if (bm.Count >= 2 &&
                        (patternOffset == beatmap.TotalColumns || patternOffset == 0))
                        edgeHitObject = bm[1];

                    patternOldOffset = patternOffset;
                    patternOffset += patternMoveDirection;
                    if (patternOffset >= beatmap.TotalColumns || patternOffset <= 0)
                        patternMoveDirection *= -1;

                    //any next note that not on the same chord
                    turnTiming = edgeHitObject.GetEndTime();
                }

                if (hitObject.Column >= patternOldOffset)
                {
                    hitObject.Column += 1;
                }
            }
        }

        private List<double> getInfo(ManiaBeatmap beatmap)
        {
            //maximum one beat for easy maps
            double minJack = 60000 / beatmap.BeatmapInfo.BPM;

            if (beatmap.BeatmapInfo.BPM > 300)
                minJack = 1000;
            var grouped = beatmap.HitObjects.GroupBy(x => x.Column);

            foreach (var group in grouped)
            {
                double endPoint = -10000;
                foreach (var hitObject in group)
                {
                    double jack = hitObject.StartTime - endPoint;

                    if (jack < 1) jack = 1;

                    if (jack < minJack)
                        minJack = jack;

                    endPoint = hitObject.GetEndTime();
                }
            }
            shortestJack = minJack;

            double jackToAvoid = minJack * jack_mult;
            var jackMap = new List<double>();
            foreach (var group in grouped)
            {
                double endPoint = -10000;
                foreach (var hitObject in group)
                {
                    double jack = hitObject.StartTime - endPoint;
                    if (jack < jackToAvoid)
                        jackMap.Add(hitObject.StartTime);

                    endPoint = hitObject.GetEndTime();
                }
                shortestJack = minJack;
            }
            return jackMap;
        }

        private void reduceColumn(ManiaBeatmap beatmap)
        {
            int currentColumn = beatmap.TotalColumns - 1;
            int MoveDirection = -1;

            foreach (var hitObject in beatmap.HitObjects)
            {
                if (hitObject.Column == currentColumn)
                {
                    currentColumn += MoveDirection;

                    if (currentColumn >= beatmap.TotalColumns - 1 || currentColumn <= 0)
                        MoveDirection *= -1;
                }
                if (hitObject.Column > currentColumn)
                    hitObject.Column -= 1;
            }
        }

        private void reduceHitObjects(ManiaBeatmap beatmap)
        {
            //hold notes treated as a separate chord to prevent their reduction
            foreach (var group in beatmap.HitObjects.GroupBy(x => new { x.StartTime, x.GetType().Name })
                                                    .ToList())
            {
                if (group.Count() == 1) continue;
                int newChordScale = (int)Math.Round(group.Count() / (double)beatmap.OriginalTotalColumns * TargetColumns);

                if (newChordScale == 0) newChordScale = 1;
                int noteToDel = group.Count() - newChordScale;

                //remove some notes at the same column
                for (int i = 0; noteToDel > 0 && i < group.Count(); i++)
                {
                    if (group.Where(x => x.Column == group.ElementAt(i).Column)
                             .Count() > 1)
                    {
                        beatmap.HitObjects.Remove(group.ElementAt(i));
                        noteToDel--;
                    }
                }

                Random rnd = new Random((int)group.Key.StartTime);
                var newGr = group.OrderBy(x => rnd.Next()).ToList();

                for (int i = 0; i < noteToDel; i++)
                {
                    beatmap.HitObjects.Remove(newGr.ElementAt(i));
                }
            }
        }

        private void fixHitObjects(ManiaBeatmap beatmap)
        {
            for (int i = 0; i < beatmap.HitObjects.Count(); i++)
            {
                var obstructions = beatmap.HitObjects.FindAll(x => x.Column == beatmap.HitObjects[i].Column
                                                              && x.GetEndTime() > beatmap.HitObjects[i].StartTime - shortestJack
                                                              && x.StartTime <= beatmap.HitObjects[i].StartTime);
                //no obstruction
                if (obstructions.Count() == 1)
                    continue;

                int newColumn = findFreePosition(beatmap, beatmap.HitObjects[i], shortestJack, out var bestHNToShorten);

                //obstruction, possible move to closest space
                if (newColumn != -1)
                {
                    beatmap.HitObjects[i].Column = newColumn;
                    continue;
                }
                //only obstruction is HoldNote, all space is obstructed,
                //possible to shorten HoldNote on current column to fit
                if (obstructions.Count() == 2
                    && obstructions.First().StartTime <= beatmap.HitObjects[i].StartTime - shortestJack
                    && obstructions.First() is HoldNote hn)
                {
                    shortenHoldNote(beatmap, beatmap.HitObjects[i], hn);
                    continue;
                }
                //obstruction, all space is obstructed,
                //possible to shorten HoldNote on any other column to fit
                if (bestHNToShorten != null)
                {
                    beatmap.HitObjects[i].Column = bestHNToShorten.Column;
                    shortenHoldNote(beatmap, beatmap.HitObjects[i], bestHNToShorten);
                    continue;
                }

                beatmap.HitObjects.Remove(beatmap.HitObjects[i]);
                i--;
            }
        }
        /// <summary>
        /// try to space out all jacks shorter than shortestJacks * jack_mult if they weren't presented in original map
        /// </summary>
        private void spaceHitObjects(ManiaBeatmap beatmap, List<double> jackMap)
        {
            double jackToAvoid = shortestJack * jack_mult;
            for (int i = 0; i < beatmap.HitObjects.Count(); i++)
            {
                var hitObject = beatmap.HitObjects[i];
                if (jackMap.FirstOrDefault(x => x == beatmap.HitObjects[i].StartTime) != default)
                    continue;

                var jack = beatmap.HitObjects.FirstOrDefault(x => x.Column == beatmap.HitObjects[i].Column
                                                              && x.StartTime > beatmap.HitObjects[i].StartTime - jackToAvoid
                                                              && x.StartTime < beatmap.HitObjects[i].StartTime);
                //no obstruction
                if (jack != null)
                {
                    int newColumn = findFreePosition(beatmap, beatmap.HitObjects[i], jackToAvoid, out var bestHNToShorten);

                    if (newColumn != -1)
                    {
                        beatmap.HitObjects[i].Column = newColumn;
                    }
                }

            }
        }

        private int findFreePosition(ManiaBeatmap beatmap, ManiaHitObject hitObject, double jackTime, out HoldNote bestHNToShorten)
        {
            bool HNFound = false;
            bestHNToShorten = null;
            for (int i = 0; ; i *= -1)
            {
                //i = -1, 1, -2, 2, -3...
                if (i <= 0) i--;

                int newColumn = hitObject.Column + i;

                if (newColumn < 0 || newColumn >= beatmap.TotalColumns)
                {
                    int nextNewColumn = hitObject.Column + i * -1;

                    if (nextNewColumn < 0 || nextNewColumn >= beatmap.TotalColumns)
                    {
                        return -1;
                    }
                    continue;
                }

                var obstructions = beatmap.HitObjects.FindAll(x => x.Column == newColumn
                                                              && x.GetEndTime() > hitObject.StartTime - jackTime
                                                              && x.StartTime < hitObject.StartTime + jackTime);
                if (obstructions.Count() == 0)
                    return newColumn;


                if (!HNFound && obstructions.Count() == 1
                    && obstructions.First().StartTime <= hitObject.StartTime - jackTime)
                {
                    switch (obstructions[0])
                    {
                        case HoldNote hold:

                            HNFound = true;
                            bestHNToShorten = hold;
                            break;
                    }
                }
            }
        }

        private void shortenHoldNote(ManiaBeatmap beatmap, ManiaHitObject hitObject, HoldNote holdNote)
        {
            double shorterEnd = hitObject.StartTime - shortestJack;
            if (shorterEnd - holdNote.StartTime >= shortestJack)
            {
                holdNote.EndTime = shorterEnd;
            }
            else
            {
                beatmap.HitObjects[beatmap.HitObjects.IndexOf(holdNote)] = new Note()
                {
                    StartTime = holdNote.StartTime,
                    Column = holdNote.Column
                };
            }
            return;
        }

        /// <summary>
        /// A pattern generator for osu!mania-specific beatmaps.
        /// </summary>
        private class SpecificBeatmapPatternGenerator : Patterns.Legacy.PatternGenerator
        {
            public SpecificBeatmapPatternGenerator(LegacyRandom random, HitObject hitObject, ManiaBeatmap beatmap, Pattern previousPattern, IBeatmap originalBeatmap)
                : base(random, hitObject, beatmap, previousPattern, originalBeatmap)
            {
            }

            public override IEnumerable<Pattern> Generate()
            {
                yield return generate();
            }

            private Pattern generate()
            {
                var positionData = HitObject as IHasXPosition;

                int column = GetColumn(positionData?.X ?? 0);

                var pattern = new Pattern();

                if (HitObject is IHasDuration endTimeData)
                {
                    pattern.Add(new HoldNote
                    {
                        StartTime = HitObject.StartTime,
                        Duration = endTimeData.Duration,
                        Column = column,
                        Samples = HitObject.Samples,
                        NodeSamples = (HitObject as IHasRepeats)?.NodeSamples ?? defaultNodeSamples
                    });
                }
                else if (HitObject is IHasXPosition)
                {
                    pattern.Add(new Note
                    {
                        StartTime = HitObject.StartTime,
                        Samples = HitObject.Samples,
                        Column = column
                    });
                }

                return pattern;
            }

            /// <remarks>
            /// osu!mania-specific beatmaps in stable only play samples at the start of the hold note.
            /// </remarks>
            private List<IList<HitSampleInfo>> defaultNodeSamples
                => new List<IList<HitSampleInfo>>
                {
                    HitObject.Samples,
                    new List<HitSampleInfo>()
                };
        }
    }
}
