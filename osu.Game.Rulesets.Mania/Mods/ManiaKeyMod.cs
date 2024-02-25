// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Mania.Mods
{
    public abstract class ManiaKeyMod : Mod, IApplicableToBeatmapConverter, IApplicableAfterBeatmapConversion
    {
        public override string Acronym => Name;
        public abstract int KeyCount { get; }
        public override ModType Type => ModType.Conversion;
        public override double ScoreMultiplier => 1; // TODO: Implement the mania key mod score multiplier
        public override bool Ranked => UsesDefaultConfiguration;

        private double shortestJack { get; set; }

        public void ApplyToBeatmapConverter(IBeatmapConverter beatmapConverter)
        {
            var mbc = (ManiaBeatmapConverter)beatmapConverter;

            if (mbc.IsForCurrentRuleset)
                return;

            mbc.TargetColumns = KeyCount;
        }

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            if (beatmap.BeatmapInfo.Ruleset.ShortName != "mania")
                return;

            var maniaBeatmap = (ManiaBeatmap)beatmap;

            while (KeyCount - maniaBeatmap.TotalColumns > 0)
            {
                insertColumn(maniaBeatmap);

                int columns = maniaBeatmap.TotalColumns;
                maniaBeatmap.Stages.Clear();
                maniaBeatmap.Stages.Add(new StageDefinition(columns + 1));
            }

            if (KeyCount - maniaBeatmap.TotalColumns < 0)
            {
                getInfo(maniaBeatmap);

                while (KeyCount - maniaBeatmap.TotalColumns < 0)
                {
                    reduceColumn(maniaBeatmap);

                    int columns = maniaBeatmap.TotalColumns;
                    maniaBeatmap.Stages.Clear();
                    maniaBeatmap.Stages.Add(new StageDefinition(columns - 1));
                }

                reduceHitObjects(maniaBeatmap);

                fixHitobjects(maniaBeatmap);
            }
            maniaBeatmap.Breaks.Clear();
        }

        //TODO: proper 1k upscale
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
                    turnTiming = edgeHitObject.GetEndTime() + .01;
                }

                if (hitObject.Column >= patternOldOffset)
                {
                    hitObject.Column += 1;
                }
            }
        }

        private void getInfo(ManiaBeatmap beatmap)
        {
            //maximum one beat for easy maps
            double minJack = 60000 / beatmap.BeatmapInfo.BPM;

            if (beatmap.BeatmapInfo.BPM > 300)
                minJack = 1000;

            foreach (var group in beatmap.HitObjects.GroupBy(x => x.Column))
            {
                double endPoint = -10000;
                foreach (var hitObject in group)
                {
                    if (hitObject.StartTime - endPoint < minJack)
                        minJack = hitObject.StartTime - endPoint;

                    if (minJack < 1) minJack = 1;

                    endPoint = hitObject.GetEndTime();
                }
            }
            shortestJack = minJack;
        }

        //warning: will produce notes at the same position
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
            foreach (var group in beatmap.HitObjects.GroupBy(x => new { x.StartTime, x.GetType().Name })
                                                    .ToList())
            {
                if (group.Count() == 1) continue;
                int newChordScale = (int)Math.Round(group.Count() / (double)beatmap.OriginalTotalColumns * KeyCount);

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

        private void fixHitobjects(ManiaBeatmap beatmap)
        {
            for (int i = 0; i < beatmap.HitObjects.Count(); i++)
            {
                var obstructions = beatmap.HitObjects.FindAll(x => x.Column == beatmap.HitObjects[i].Column
                                                 && x.GetEndTime() > beatmap.HitObjects[i].StartTime - shortestJack
                                                 && x.StartTime < beatmap.HitObjects[i].StartTime + shortestJack);
                //no obstruction
                if (obstructions.Count() == 1)
                    continue;

                int newColumn = moveNote(beatmap, beatmap.HitObjects[i], out var bestHNToShorten);

                //obstruction, possible move to closest space
                if (newColumn != -1)
                {
                    beatmap.HitObjects[i].Column = newColumn;
                    continue;
                }
                //only obstruction is HoldNote, all space is obstructed, possible to shorten HoldNote to fit
                if (bestHNToShorten != null
                    && obstructions.Count() == 2
                    && obstructions.First().StartTime < beatmap.HitObjects[i].StartTime)
                {
                    beatmap.HitObjects[i].Column = bestHNToShorten.Column;
                    shortenHoldNote(beatmap, beatmap.HitObjects[i], bestHNToShorten);
                    continue;
                }

                beatmap.HitObjects.Remove(beatmap.HitObjects[i]);
                i--;
            }
        }

        //TODO: info array about jacks in original map,
        //if too many jacks created were they shouldn't be try to move them apart
        private int moveNote(ManiaBeatmap beatmap, ManiaHitObject hitObject, out HoldNote? bestHNToShorten)
        {
            bool HDFound = false;
            bestHNToShorten = null;
            var findHoldNote = beatmap.HitObjects.Where(x => x.Column == hitObject.Column
                                                  && x.StartTime <= hitObject.StartTime - shortestJack)
                                           .LastOrDefault();
            switch (findHoldNote)
            {
                case HoldNote hold:

                    HDFound = true;
                    bestHNToShorten = hold;
                    break;
            }

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

                if (!HDFound)
                {
                    findHoldNote = beatmap.HitObjects.Where(x => x.Column == newColumn
                                                      && x.StartTime <= hitObject.StartTime - shortestJack)
                                               .LastOrDefault();
                    switch (findHoldNote)
                    {
                        case HoldNote hold:

                            HDFound = true;
                            bestHNToShorten = hold;
                            break;
                    }
                }

                if (beatmap.HitObjects.FindIndex(x => x.Column == newColumn
                                                 && x.GetEndTime() > hitObject.StartTime - shortestJack
                                                 && x.StartTime < hitObject.StartTime + shortestJack) == -1)
                {
                    return newColumn;
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

        public override Type[] IncompatibleMods => new[]
                {
            typeof(ManiaModKey1),
            typeof(ManiaModKey2),
            typeof(ManiaModKey3),
            typeof(ManiaModKey4),
            typeof(ManiaModKey5),
            typeof(ManiaModKey6),
            typeof(ManiaModKey7),
            typeof(ManiaModKey8),
            typeof(ManiaModKey9),
            typeof(ManiaModKey10),
        }.Except(new[] { GetType() }).ToArray();
    }
}
