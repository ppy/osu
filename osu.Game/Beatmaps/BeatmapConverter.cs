// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using osu.Framework.Lists;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Beatmaps
{
    public abstract class BeatmapConverter<T> : IBeatmapConverter
        where T : HitObject
    {
        private event Action<HitObject, IEnumerable<HitObject>> objectConverted;

        event Action<HitObject, IEnumerable<HitObject>> IBeatmapConverter.ObjectConverted
        {
            add => objectConverted += value;
            remove => objectConverted -= value;
        }

        public IBeatmap Beatmap { get; }

        protected BeatmapConverter(IBeatmap beatmap, Ruleset ruleset)
        {
            Beatmap = beatmap;
        }

        public abstract bool CanConvert();

        public IBeatmap Convert(CancellationToken cancellationToken = default)
        {
            var original = Beatmap.Clone();

            original.BeatmapInfo = original.BeatmapInfo.Clone();
            original.ControlPointInfo = original.ControlPointInfo.DeepClone();

            original.Breaks = new SortedList<BreakPeriod>(Comparer<BreakPeriod>.Default);
            original.Breaks.AddRange(Beatmap.Breaks);

            return ConvertBeatmap(original, cancellationToken);
        }

        protected virtual Beatmap<T> ConvertBeatmap(IBeatmap original, CancellationToken cancellationToken)
        {
            var beatmap = CreateBeatmap();

            beatmap.BeatmapInfo = original.BeatmapInfo;
            beatmap.ControlPointInfo = original.ControlPointInfo;
            beatmap.HitObjects = convertHitObjects(original.HitObjects, original, cancellationToken).OrderBy(s => s.StartTime).ToList();
            beatmap.Breaks = original.Breaks;

            beatmap.UnhandledEventLines = new List<string>(original.UnhandledEventLines);

            beatmap.AudioLeadIn = original.AudioLeadIn;
            beatmap.StackLeniency = original.StackLeniency;
            beatmap.SpecialStyle = original.SpecialStyle;
            beatmap.LetterboxInBreaks = original.LetterboxInBreaks;
            beatmap.WidescreenStoryboard = original.WidescreenStoryboard;
            beatmap.EpilepsyWarning = original.EpilepsyWarning;
            beatmap.SamplesMatchPlaybackRate = original.SamplesMatchPlaybackRate;
            beatmap.DistanceSpacing = original.DistanceSpacing;
            beatmap.GridSize = original.GridSize;
            beatmap.TimelineZoom = original.TimelineZoom;
            beatmap.Countdown = original.Countdown;
            beatmap.CountdownOffset = original.CountdownOffset;
            beatmap.Bookmarks = original.Bookmarks;

            return beatmap;
        }

        private List<T> convertHitObjects(IReadOnlyList<HitObject> hitObjects, IBeatmap beatmap, CancellationToken cancellationToken)
        {
            var result = new List<T>(hitObjects.Count);

            foreach (var obj in hitObjects)
            {
                if (obj is T tObj)
                {
                    result.Add(tObj);
                    continue;
                }

                var converted = ConvertHitObject(obj, beatmap, cancellationToken);

                if (objectConverted != null)
                {
                    converted = converted.ToList();
                    objectConverted.Invoke(obj, converted);
                }

                foreach (var c in converted)
                {
                    if (c != null)
                        result.Add(c);
                }
            }

            return result;
        }
        
        protected virtual IEnumerable<T> ConvertHitObject(HitObject original, IBeatmap beatmap, CancellationToken cancellationToken)
        {
            return Enumerable.Empty<T>();
        }

        protected virtual Beatmap<T> CreateBeatmap() => new Beatmap<T>();
    }
}
