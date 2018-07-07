// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Objects.Legacy;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Beatmaps.Formats
{
    public class LegacyBeatmapDecoder : LegacyDecoder<Beatmap>
    {
        public const int LATEST_VERSION = 14;

        private Beatmap beatmap;

        private ConvertHitObjectParser parser;

        private LegacySampleBank defaultSampleBank;
        private int defaultSampleVolume = 100;

        public static void Register()
        {
            AddDecoder<Beatmap>(@"osu file format v", m => new LegacyBeatmapDecoder(int.Parse(m.Split('v').Last())));
        }

        /// <summary>
        /// Whether or not beatmap or runtime offsets should be applied. Defaults on; only disable for testing purposes.
        /// </summary>
        public bool ApplyOffsets = true;

        private readonly int offset;

        public LegacyBeatmapDecoder(int version = LATEST_VERSION)
            : base(version)
        {
            // BeatmapVersion 4 and lower had an incorrect offset (stable has this set as 24ms off)
            offset = FormatVersion < 5 ? 24 : 0;
        }

        protected override void ParseStreamInto(StreamReader stream, Beatmap beatmap)
        {
            this.beatmap = beatmap;
            this.beatmap.BeatmapInfo.BeatmapVersion = FormatVersion;

            base.ParseStreamInto(stream, beatmap);

            // Objects may be out of order *only* if a user has manually edited an .osu file.
            // Unfortunately there are ranked maps in this state (example: https://osu.ppy.sh/s/594828).
            // OrderBy is used to guarantee that the parsing order of hitobjects with equal start times is maintained (stably-sorted)
            // The parsing order of hitobjects matters in mania difficulty calculation
            this.beatmap.HitObjects = this.beatmap.HitObjects.OrderBy(h => h.StartTime).ToList();

            foreach (var hitObject in this.beatmap.HitObjects)
                hitObject.ApplyDefaults(this.beatmap.ControlPointInfo, this.beatmap.BeatmapInfo.BaseDifficulty);
        }

        protected override bool ShouldSkipLine(string line) => base.ShouldSkipLine(line) || line.StartsWith(" ") || line.StartsWith("_");

        protected override void ParseLine(Beatmap beatmap, Section section, string line)
        {
            switch (section)
            {
                case Section.General:
                    handleGeneral(line);
                    return;
                case Section.Editor:
                    handleEditor(line);
                    return;
                case Section.Metadata:
                    handleMetadata(line);
                    return;
                case Section.Difficulty:
                    handleDifficulty(line);
                    return;
                case Section.Events:
                    handleEvent(line);
                    return;
                case Section.TimingPoints:
                    handleTimingPoint(line);
                    return;
                case Section.HitObjects:
                    handleHitObject(line);
                    return;
            }

            base.ParseLine(beatmap, section, line);
        }

        private void handleGeneral(string line)
        {
            var pair = SplitKeyVal(line);

            var metadata = beatmap.BeatmapInfo.Metadata;
            switch (pair.Key)
            {
                case @"AudioFilename":
                    metadata.AudioFile = pair.Value;
                    break;
                case @"AudioLeadIn":
                    beatmap.BeatmapInfo.AudioLeadIn = int.Parse(pair.Value);
                    break;
                case @"PreviewTime":
                    metadata.PreviewTime = getOffsetTime(int.Parse(pair.Value));
                    break;
                case @"Countdown":
                    beatmap.BeatmapInfo.Countdown = int.Parse(pair.Value) == 1;
                    break;
                case @"SampleSet":
                    defaultSampleBank = (LegacySampleBank)Enum.Parse(typeof(LegacySampleBank), pair.Value);
                    break;
                case @"SampleVolume":
                    defaultSampleVolume = int.Parse(pair.Value);
                    break;
                case @"StackLeniency":
                    beatmap.BeatmapInfo.StackLeniency = float.Parse(pair.Value, NumberFormatInfo.InvariantInfo);
                    break;
                case @"Mode":
                    beatmap.BeatmapInfo.RulesetID = int.Parse(pair.Value);

                    switch (beatmap.BeatmapInfo.RulesetID)
                    {
                        case 0:
                            parser = new Rulesets.Objects.Legacy.Osu.ConvertHitObjectParser();
                            break;
                        case 1:
                            parser = new Rulesets.Objects.Legacy.Taiko.ConvertHitObjectParser();
                            break;
                        case 2:
                            parser = new Rulesets.Objects.Legacy.Catch.ConvertHitObjectParser();
                            break;
                        case 3:
                            parser = new Rulesets.Objects.Legacy.Mania.ConvertHitObjectParser();
                            break;
                    }

                    break;
                case @"LetterboxInBreaks":
                    beatmap.BeatmapInfo.LetterboxInBreaks = int.Parse(pair.Value) == 1;
                    break;
                case @"SpecialStyle":
                    beatmap.BeatmapInfo.SpecialStyle = int.Parse(pair.Value) == 1;
                    break;
                case @"WidescreenStoryboard":
                    beatmap.BeatmapInfo.WidescreenStoryboard = int.Parse(pair.Value) == 1;
                    break;
            }
        }

        private void handleEditor(string line)
        {
            var pair = SplitKeyVal(line);

            switch (pair.Key)
            {
                case @"Bookmarks":
                    beatmap.BeatmapInfo.StoredBookmarks = pair.Value;
                    break;
                case @"DistanceSpacing":
                    beatmap.BeatmapInfo.DistanceSpacing = double.Parse(pair.Value, NumberFormatInfo.InvariantInfo);
                    break;
                case @"BeatDivisor":
                    beatmap.BeatmapInfo.BeatDivisor = int.Parse(pair.Value);
                    break;
                case @"GridSize":
                    beatmap.BeatmapInfo.GridSize = int.Parse(pair.Value);
                    break;
                case @"TimelineZoom":
                    beatmap.BeatmapInfo.TimelineZoom = double.Parse(pair.Value, NumberFormatInfo.InvariantInfo);
                    break;
            }
        }

        private void handleMetadata(string line)
        {
            var pair = SplitKeyVal(line);

            var metadata = beatmap.BeatmapInfo.Metadata;
            switch (pair.Key)
            {
                case @"Title":
                    metadata.Title = pair.Value;
                    break;
                case @"TitleUnicode":
                    metadata.TitleUnicode = pair.Value;
                    break;
                case @"Artist":
                    metadata.Artist = pair.Value;
                    break;
                case @"ArtistUnicode":
                    metadata.ArtistUnicode = pair.Value;
                    break;
                case @"Creator":
                    metadata.AuthorString = pair.Value;
                    break;
                case @"Version":
                    beatmap.BeatmapInfo.Version = pair.Value;
                    break;
                case @"Source":
                    beatmap.BeatmapInfo.Metadata.Source = pair.Value;
                    break;
                case @"Tags":
                    beatmap.BeatmapInfo.Metadata.Tags = pair.Value;
                    break;
                case @"BeatmapID":
                    beatmap.BeatmapInfo.OnlineBeatmapID = int.Parse(pair.Value);
                    break;
                case @"BeatmapSetID":
                    beatmap.BeatmapInfo.BeatmapSet = new BeatmapSetInfo { OnlineBeatmapSetID = int.Parse(pair.Value) };
                    break;
            }
        }

        private void handleDifficulty(string line)
        {
            var pair = SplitKeyVal(line);

            var difficulty = beatmap.BeatmapInfo.BaseDifficulty;
            switch (pair.Key)
            {
                case @"HPDrainRate":
                    difficulty.DrainRate = float.Parse(pair.Value, NumberFormatInfo.InvariantInfo);
                    break;
                case @"CircleSize":
                    difficulty.CircleSize = float.Parse(pair.Value, NumberFormatInfo.InvariantInfo);
                    break;
                case @"OverallDifficulty":
                    difficulty.OverallDifficulty = float.Parse(pair.Value, NumberFormatInfo.InvariantInfo);
                    break;
                case @"ApproachRate":
                    difficulty.ApproachRate = float.Parse(pair.Value, NumberFormatInfo.InvariantInfo);
                    break;
                case @"SliderMultiplier":
                    difficulty.SliderMultiplier = double.Parse(pair.Value, NumberFormatInfo.InvariantInfo);
                    break;
                case @"SliderTickRate":
                    difficulty.SliderTickRate = double.Parse(pair.Value, NumberFormatInfo.InvariantInfo);
                    break;
            }
        }

        private void handleEvent(string line)
        {
            string[] split = line.Split(',');

            EventType type;
            if (!Enum.TryParse(split[0], out type))
                throw new InvalidDataException($@"Unknown event type {split[0]}");

            switch (type)
            {
                case EventType.Background:
                    string filename = split[2].Trim('"');
                    beatmap.BeatmapInfo.Metadata.BackgroundFile = filename;
                    break;
                case EventType.Break:
                    var breakEvent = new BreakPeriod
                    {
                        StartTime = getOffsetTime(double.Parse(split[1], NumberFormatInfo.InvariantInfo)),
                        EndTime = getOffsetTime(double.Parse(split[2], NumberFormatInfo.InvariantInfo))
                    };

                    if (!breakEvent.HasEffect)
                        return;

                    beatmap.Breaks.Add(breakEvent);
                    break;
            }
        }

        private void handleTimingPoint(string line)
        {
            try
            {
                string[] split = line.Split(',');

                double time = getOffsetTime(double.Parse(split[0].Trim(), NumberFormatInfo.InvariantInfo));
                double beatLength = double.Parse(split[1].Trim(), NumberFormatInfo.InvariantInfo);
                double speedMultiplier = beatLength < 0 ? 100.0 / -beatLength : 1;

                TimeSignatures timeSignature = TimeSignatures.SimpleQuadruple;
                if (split.Length >= 3)
                    timeSignature = split[2][0] == '0' ? TimeSignatures.SimpleQuadruple : (TimeSignatures)int.Parse(split[2]);

                LegacySampleBank sampleSet = defaultSampleBank;
                if (split.Length >= 4)
                    sampleSet = (LegacySampleBank)int.Parse(split[3]);

                int customSampleBank = 0;
                if (split.Length >= 5)
                    customSampleBank = int.Parse(split[4]);

                int sampleVolume = defaultSampleVolume;
                if (split.Length >= 6)
                    sampleVolume = int.Parse(split[5]);

                bool timingChange = true;
                if (split.Length >= 7)
                    timingChange = split[6][0] == '1';

                bool kiaiMode = false;
                bool omitFirstBarSignature = false;
                if (split.Length >= 8)
                {
                    int effectFlags = int.Parse(split[7]);
                    kiaiMode = (effectFlags & 1) > 0;
                    omitFirstBarSignature = (effectFlags & 8) > 0;
                }

                string stringSampleSet = sampleSet.ToString().ToLower();
                if (stringSampleSet == @"none")
                    stringSampleSet = @"normal";

                if (timingChange)
                {
                    handleTimingControlPoint(new TimingControlPoint
                    {
                        Time = time,
                        BeatLength = beatLength,
                        TimeSignature = timeSignature
                    });
                }

                handleDifficultyControlPoint(new DifficultyControlPoint
                {
                    Time = time,
                    SpeedMultiplier = speedMultiplier
                });

                handleEffectControlPoint(new EffectControlPoint
                {
                    Time = time,
                    KiaiMode = kiaiMode,
                    OmitFirstBarLine = omitFirstBarSignature
                });

                handleSampleControlPoint(new LegacySampleControlPoint
                {
                    Time = time,
                    SampleBank = stringSampleSet,
                    SampleVolume = sampleVolume,
                    CustomSampleBank = customSampleBank
                });
            }
            catch (FormatException e)
            {
            }
        }

        private void handleTimingControlPoint(TimingControlPoint newPoint)
        {
            beatmap.ControlPointInfo.TimingPoints.Add(newPoint);
        }

        private void handleDifficultyControlPoint(DifficultyControlPoint newPoint)
        {
            var existing = beatmap.ControlPointInfo.DifficultyPointAt(newPoint.Time);

            if (newPoint.EquivalentTo(existing))
                return;

            beatmap.ControlPointInfo.DifficultyPoints.RemoveAll(x => x.Time == newPoint.Time);
            beatmap.ControlPointInfo.DifficultyPoints.Add(newPoint);
        }

        private void handleEffectControlPoint(EffectControlPoint newPoint)
        {
            var existing = beatmap.ControlPointInfo.EffectPointAt(newPoint.Time);

            if (newPoint.EquivalentTo(existing))
                return;

            beatmap.ControlPointInfo.EffectPoints.Add(newPoint);
        }

        private void handleSampleControlPoint(SampleControlPoint newPoint)
        {
            var existing = beatmap.ControlPointInfo.SamplePointAt(newPoint.Time);

            if (newPoint.EquivalentTo(existing))
                return;

            beatmap.ControlPointInfo.SamplePoints.Add(newPoint);
        }

        private void handleHitObject(string line)
        {
            // If the ruleset wasn't specified, assume the osu!standard ruleset.
            if (parser == null)
                parser = new Rulesets.Objects.Legacy.Osu.ConvertHitObjectParser();

            var obj = parser.Parse(line, getOffsetTime());

            if (obj != null)
            {
                beatmap.HitObjects.Add(obj);
            }
        }

        private int getOffsetTime(int time) => time + (ApplyOffsets ? offset : 0);

        private double getOffsetTime() => ApplyOffsets ? offset : 0;

        private double getOffsetTime(double time) => time + (ApplyOffsets ? offset : 0);
    }
}
