// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using osu.Framework.Extensions;
using osu.Framework.Logging;
using osu.Game.Audio;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Beatmaps.Timing;
using osu.Game.IO;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Legacy;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Screens.Edit;
using osu.Game.Utils;

namespace osu.Game.Beatmaps.Formats
{
    public class LegacyBeatmapDecoder : LegacyDecoder<Beatmap>
    {
        /// <summary>
        /// An offset which needs to be applied to old beatmaps (v4 and lower) to correct timing changes that were applied at a game client level.
        /// </summary>
        public const int EARLY_VERSION_TIMING_OFFSET = 24;

        /// <summary>
        /// A small adjustment to the start time of sample control points to account for rounding/precision errors.
        /// </summary>
        /// <remarks>
        /// Compare: https://github.com/peppy/osu-stable-reference/blob/master/osu!/GameplayElements/HitObjects/HitObject.cs#L319
        /// </remarks>
        public const double CONTROL_POINT_LENIENCY = 5;

        /// <summary>
        /// The maximum allowed number of keys in mania beatmaps.
        /// </summary>
        public const int MAX_MANIA_KEY_COUNT = 18;

        internal static RulesetStore? RulesetStore;

        private Beatmap beatmap = null!;
        private ConvertHitObjectParser parser = null!;

        private LegacySampleBank defaultSampleBank;
        private int defaultSampleVolume = 100;

        public static void Register()
        {
            AddDecoder<Beatmap>(@"osu file format v", m => new LegacyBeatmapDecoder(Parsing.ParseInt(m.Split('v').Last())));
            SetFallbackDecoder<Beatmap>(() => new LegacyBeatmapDecoder());
        }

        /// <summary>
        /// Whether beatmap or runtime offsets should be applied. Defaults on; only disable for testing purposes.
        /// </summary>
        public bool ApplyOffsets = true;

        private readonly int offset;

        public LegacyBeatmapDecoder(int version = LATEST_VERSION)
            : base(version)
        {
            if (RulesetStore == null)
            {
                Logger.Log($"A {nameof(RulesetStore)} was not provided via {nameof(Decoder)}.{nameof(RegisterDependencies)}; falling back to default {nameof(AssemblyRulesetStore)}.");
                RulesetStore = new AssemblyRulesetStore();
            }

            offset = FormatVersion < 5 ? EARLY_VERSION_TIMING_OFFSET : 0;
        }

        protected override Beatmap CreateTemplateObject()
        {
            var templateBeatmap = base.CreateTemplateObject();
            templateBeatmap.ControlPointInfo = new LegacyControlPointInfo();
            return templateBeatmap;
        }

        protected override void ParseStreamInto(LineBufferedReader stream, Beatmap beatmap)
        {
            this.beatmap = beatmap;
            this.beatmap.BeatmapVersion = FormatVersion;
            parser = new ConvertHitObjectParser(getOffsetTime(), FormatVersion);

            ApplyLegacyDefaults(this.beatmap);

            base.ParseStreamInto(stream, beatmap);

            applyDifficultyRestrictions(beatmap.Difficulty, beatmap);

            flushPendingPoints();

            // Objects may be out of order *only* if a user has manually edited an .osu file.
            // Unfortunately there are ranked maps in this state (example: https://osu.ppy.sh/s/594828).
            // OrderBy is used to guarantee that the parsing order of hitobjects with equal start times is maintained (stably-sorted)
            // The parsing order of hitobjects matters in mania difficulty calculation
            this.beatmap.HitObjects = this.beatmap.HitObjects.OrderBy(h => h.StartTime).ToList();

            postProcessBreaks(this.beatmap);

            foreach (var hitObject in this.beatmap.HitObjects)
            {
                applyDefaults(hitObject);
                applySamples(hitObject);
            }
        }

        /// <summary>
        /// Ensures that all <see cref="BeatmapDifficulty"/> settings are within the allowed ranges.
        /// See also: https://github.com/peppy/osu-stable-reference/blob/0e425c0d525ef21353c8293c235cc0621d28338b/osu!/GameplayElements/Beatmaps/Beatmap.cs#L567-L614
        /// </summary>
        private static void applyDifficultyRestrictions(BeatmapDifficulty difficulty, Beatmap beatmap)
        {
            difficulty.DrainRate = Math.Clamp(difficulty.DrainRate, 0, 10);

            // mania uses "circle size" for key count, thus different allowable range
            difficulty.CircleSize = beatmap.BeatmapInfo.Ruleset.OnlineID != 3
                ? Math.Clamp(difficulty.CircleSize, 0, 10)
                : Math.Clamp(difficulty.CircleSize, 1, MAX_MANIA_KEY_COUNT);

            difficulty.OverallDifficulty = Math.Clamp(difficulty.OverallDifficulty, 0, 10);
            difficulty.ApproachRate = Math.Clamp(difficulty.ApproachRate, 0, 10);

            difficulty.SliderMultiplier = Math.Clamp(difficulty.SliderMultiplier, 0.4, 3.6);
            difficulty.SliderTickRate = Math.Clamp(difficulty.SliderTickRate, 0.5, 8);
        }

        /// <summary>
        /// Processes the beatmap such that a new combo is started the first hitobject following each break.
        /// </summary>
        private static void postProcessBreaks(Beatmap beatmap)
        {
            int currentBreak = 0;
            bool forceNewCombo = false;

            foreach (var h in beatmap.HitObjects.OfType<ConvertHitObject>())
            {
                while (currentBreak < beatmap.Breaks.Count && beatmap.Breaks[currentBreak].EndTime < h.StartTime)
                {
                    forceNewCombo = true;
                    currentBreak++;
                }

                h.NewCombo |= forceNewCombo;
                forceNewCombo = false;
            }
        }

        private void applyDefaults(HitObject hitObject)
        {
            DifficultyControlPoint difficultyControlPoint = (beatmap.ControlPointInfo as LegacyControlPointInfo)?.DifficultyPointAt(hitObject.StartTime) ?? DifficultyControlPoint.DEFAULT;

            if (hitObject is IHasGenerateTicks hasGenerateTicks)
                hasGenerateTicks.GenerateTicks = difficultyControlPoint.GenerateTicks;

            if (hitObject is IHasSliderVelocity hasSliderVelocity)
                hasSliderVelocity.SliderVelocityMultiplier = difficultyControlPoint.SliderVelocity;

            hitObject.ApplyDefaults(beatmap.ControlPointInfo, beatmap.Difficulty);
        }

        private void applySamples(HitObject hitObject)
        {
            if (hitObject is IHasRepeats hasRepeats)
            {
                SampleControlPoint sampleControlPoint = (beatmap.ControlPointInfo as LegacyControlPointInfo)?.SamplePointAt(hitObject.StartTime + CONTROL_POINT_LENIENCY + 1)
                                                        ?? SampleControlPoint.DEFAULT;
                hitObject.Samples = hitObject.Samples.Select(o => sampleControlPoint.ApplyTo(o)).ToList();

                for (int i = 0; i < hasRepeats.NodeSamples.Count; i++)
                {
                    double time = hitObject.StartTime + i * hasRepeats.Duration / hasRepeats.SpanCount() + CONTROL_POINT_LENIENCY;
                    var nodeSamplePoint = (beatmap.ControlPointInfo as LegacyControlPointInfo)?.SamplePointAt(time) ?? SampleControlPoint.DEFAULT;

                    hasRepeats.NodeSamples[i] = hasRepeats.NodeSamples[i].Select(o => nodeSamplePoint.ApplyTo(o)).ToList();
                }
            }
            else
            {
                SampleControlPoint sampleControlPoint = (beatmap.ControlPointInfo as LegacyControlPointInfo)?.SamplePointAt(hitObject.GetEndTime() + CONTROL_POINT_LENIENCY)
                                                        ?? SampleControlPoint.DEFAULT;
                hitObject.Samples = hitObject.Samples.Select(o => sampleControlPoint.ApplyTo(o)).ToList();
            }
        }

        /// <summary>
        /// Some `BeatmapInfo` members have default values that differ from the default values used by stable.
        /// In addition, legacy beatmaps will sometimes not contain some configuration keys, in which case
        /// the legacy default values should be used.
        /// This method's intention is to restore those legacy defaults.
        /// See also: https://osu.ppy.sh/wiki/en/Client/File_formats/Osu_%28file_format%29
        /// </summary>
        internal static void ApplyLegacyDefaults(Beatmap beatmap)
        {
            beatmap.WidescreenStoryboard = false;
            // in a perfect world this would throw if osu! ruleset couldn't be found,
            // but unfortunately there are "legitimate" cases where it's not there (i.e. ruleset test projects),
            // so attempt to trudge on with whatever it is that's in `BeatmapInfo` if the lookup fails.
            beatmap.BeatmapInfo.Ruleset = RulesetStore?.GetRuleset(0) ?? beatmap.BeatmapInfo.Ruleset;
        }

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
                    metadata.AudioFile = pair.Value.ToStandardisedPath();
                    break;

                case @"AudioLeadIn":
                    beatmap.AudioLeadIn = Parsing.ParseInt(pair.Value);
                    break;

                case @"PreviewTime":
                    int time = Parsing.ParseInt(pair.Value);
                    metadata.PreviewTime = time == -1 ? time : getOffsetTime(time);
                    break;

                case @"SampleSet":
                    defaultSampleBank = Enum.Parse<LegacySampleBank>(pair.Value);
                    break;

                case @"SampleVolume":
                    defaultSampleVolume = Parsing.ParseInt(pair.Value);
                    break;

                case @"StackLeniency":
                    beatmap.StackLeniency = Parsing.ParseFloat(pair.Value);
                    break;

                case @"Mode":
                    beatmap.BeatmapInfo.Ruleset = RulesetStore?.GetRuleset(Parsing.ParseInt(pair.Value)) ?? throw new ArgumentException("Ruleset is not available locally.");
                    break;

                case @"LetterboxInBreaks":
                    beatmap.LetterboxInBreaks = Parsing.ParseInt(pair.Value) == 1;
                    break;

                case @"SpecialStyle":
                    beatmap.SpecialStyle = Parsing.ParseInt(pair.Value) == 1;
                    break;

                case @"WidescreenStoryboard":
                    beatmap.WidescreenStoryboard = Parsing.ParseInt(pair.Value) == 1;
                    break;

                case @"EpilepsyWarning":
                    beatmap.EpilepsyWarning = Parsing.ParseInt(pair.Value) == 1;
                    break;

                case @"SamplesMatchPlaybackRate":
                    beatmap.SamplesMatchPlaybackRate = Parsing.ParseInt(pair.Value) == 1;
                    break;

                case @"Countdown":
                    beatmap.Countdown = Enum.Parse<CountdownType>(pair.Value);
                    break;

                case @"CountdownOffset":
                    beatmap.CountdownOffset = Parsing.ParseInt(pair.Value);
                    break;
            }
        }

        private void handleEditor(string line)
        {
            var pair = SplitKeyVal(line);

            switch (pair.Key)
            {
                case @"Bookmarks":
                    beatmap.Bookmarks = pair.Value.Split(',').Select(v =>
                    {
                        bool result = int.TryParse(v, out int val);
                        return new { result, val };
                    }).Where(p => p.result).Select(p => p.val).ToArray();
                    break;

                case @"DistanceSpacing":
                    beatmap.DistanceSpacing = Math.Max(0, Parsing.ParseDouble(pair.Value));
                    break;

                case @"BeatDivisor":
                    beatmap.BeatmapInfo.BeatDivisor = Math.Clamp(Parsing.ParseInt(pair.Value), BindableBeatDivisor.MINIMUM_DIVISOR, BindableBeatDivisor.MAXIMUM_DIVISOR);
                    break;

                case @"GridSize":
                    beatmap.GridSize = Parsing.ParseInt(pair.Value);
                    break;

                case @"TimelineZoom":
                    beatmap.TimelineZoom = Math.Max(0, Parsing.ParseDouble(pair.Value));
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
                    metadata.Author.Username = pair.Value;
                    break;

                case @"Version":
                    beatmap.BeatmapInfo.DifficultyName = pair.Value;
                    break;

                case @"Source":
                    metadata.Source = pair.Value;
                    break;

                case @"Tags":
                    metadata.Tags = pair.Value;
                    break;

                case @"BeatmapID":
                    beatmap.BeatmapInfo.OnlineID = Parsing.ParseInt(pair.Value);
                    break;

                case @"BeatmapSetID":
                    beatmap.BeatmapInfo.BeatmapSet = new BeatmapSetInfo { OnlineID = Parsing.ParseInt(pair.Value) };
                    break;
            }
        }

        private void handleDifficulty(string line)
        {
            var pair = SplitKeyVal(line);

            var difficulty = beatmap.Difficulty;

            switch (pair.Key)
            {
                case @"HPDrainRate":
                    difficulty.DrainRate = Parsing.ParseFloat(pair.Value);
                    break;

                case @"CircleSize":
                    difficulty.CircleSize = Parsing.ParseFloat(pair.Value);
                    break;

                case @"OverallDifficulty":
                    difficulty.OverallDifficulty = Parsing.ParseFloat(pair.Value);
                    if (!hasApproachRate)
                        difficulty.ApproachRate = difficulty.OverallDifficulty;
                    break;

                case @"ApproachRate":
                    difficulty.ApproachRate = Parsing.ParseFloat(pair.Value);
                    hasApproachRate = true;
                    break;

                case @"SliderMultiplier":
                    difficulty.SliderMultiplier = Parsing.ParseDouble(pair.Value);
                    break;

                case @"SliderTickRate":
                    difficulty.SliderTickRate = Parsing.ParseDouble(pair.Value);
                    break;
            }
        }

        private void handleEvent(string line)
        {
            string[] split = line.Split(',');

            // Until we have full storyboard encoder coverage, let's track any lines which aren't handled
            // and store them to a temporary location such that they aren't lost on editor save / export.
            bool lineSupportedByEncoder = false;

            if (Enum.TryParse(split[0], out LegacyEventType type))
            {
                switch (type)
                {
                    case LegacyEventType.Sprite:
                        // Generally, the background is the first thing defined in a beatmap file.
                        // In some older beatmaps, it is not present and replaced by a storyboard-level background instead.
                        // Allow the first sprite (by file order) to act as the background in such cases.
                        if (string.IsNullOrEmpty(beatmap.BeatmapInfo.Metadata.BackgroundFile))
                        {
                            beatmap.BeatmapInfo.Metadata.BackgroundFile = CleanFilename(split[3]);
                            lineSupportedByEncoder = true;
                        }

                        break;

                    case LegacyEventType.Video:
                        string filename = CleanFilename(split[2]);

                        // Some very old beatmaps had incorrect type specifications for their backgrounds (ie. using 1 for VIDEO
                        // instead of 0 for BACKGROUND). To handle this gracefully, check the file extension against known supported
                        // video extensions and handle similar to a background if it doesn't match.
                        if (!SupportedExtensions.VIDEO_EXTENSIONS.Contains(Path.GetExtension(filename).ToLowerInvariant()))
                        {
                            beatmap.BeatmapInfo.Metadata.BackgroundFile = filename;
                            lineSupportedByEncoder = true;
                        }

                        break;

                    case LegacyEventType.Background:
                        beatmap.BeatmapInfo.Metadata.BackgroundFile = CleanFilename(split[2]);
                        lineSupportedByEncoder = true;
                        break;

                    case LegacyEventType.Break:
                        double start = getOffsetTime(Parsing.ParseDouble(split[1]));
                        double end = Math.Max(start, getOffsetTime(Parsing.ParseDouble(split[2])));

                        beatmap.Breaks.Add(new BreakPeriod(start, end));
                        lineSupportedByEncoder = true;
                        break;
                }
            }

            if (!lineSupportedByEncoder)
                beatmap.UnhandledEventLines.Add(line);
        }

        private void handleTimingPoint(string line)
        {
            string[] split = line.Split(',');

            double time = getOffsetTime(Parsing.ParseDouble(split[0].Trim()));

            // beatLength is allowed to be NaN to handle an edge case in which some beatmaps use NaN slider velocity to disable slider tick generation (see LegacyDifficultyControlPoint).
            double beatLength = Parsing.ParseDouble(split[1].Trim(), allowNaN: true);

            // If beatLength is NaN, speedMultiplier should still be 1 because all comparisons against NaN are false.
            double speedMultiplier = beatLength < 0 ? 100.0 / -beatLength : 1;

            TimeSignature timeSignature = TimeSignature.SimpleQuadruple;
            if (split.Length >= 3)
                timeSignature = split[2][0] == '0' ? TimeSignature.SimpleQuadruple : new TimeSignature(Parsing.ParseInt(split[2]));

            LegacySampleBank sampleSet = defaultSampleBank;
            if (split.Length >= 4)
                sampleSet = (LegacySampleBank)Parsing.ParseInt(split[3]);

            int customSampleBank = 0;
            if (split.Length >= 5)
                customSampleBank = Parsing.ParseInt(split[4]);

            int sampleVolume = defaultSampleVolume;
            if (split.Length >= 6)
                sampleVolume = Parsing.ParseInt(split[5]);

            bool timingChange = true;
            if (split.Length >= 7)
                timingChange = split[6][0] == '1';

            bool kiaiMode = false;
            bool omitFirstBarSignature = false;

            if (split.Length >= 8)
            {
                LegacyEffectFlags effectFlags = (LegacyEffectFlags)Parsing.ParseInt(split[7]);
                kiaiMode = effectFlags.HasFlag(LegacyEffectFlags.Kiai);
                omitFirstBarSignature = effectFlags.HasFlag(LegacyEffectFlags.OmitFirstBarLine);
            }

            string stringSampleSet = sampleSet.ToString().ToLowerInvariant();
            if (stringSampleSet == @"none")
                stringSampleSet = HitSampleInfo.BANK_NORMAL;

            if (timingChange)
            {
                if (double.IsNaN(beatLength))
                    throw new InvalidDataException("Beat length cannot be NaN in a timing control point");

                var controlPoint = CreateTimingControlPoint();

                controlPoint.BeatLength = beatLength;
                controlPoint.TimeSignature = timeSignature;
                controlPoint.OmitFirstBarLine = omitFirstBarSignature;

                addControlPoint(time, controlPoint, true);
            }

            int onlineRulesetID = beatmap.BeatmapInfo.Ruleset.OnlineID;

            addControlPoint(time, new DifficultyControlPoint
            {
                GenerateTicks = !double.IsNaN(beatLength),
                SliderVelocity = speedMultiplier,
            }, timingChange);

            var effectPoint = new EffectControlPoint
            {
                KiaiMode = kiaiMode,
            };

            // osu!taiko and osu!mania use effect points rather than difficulty points for scroll speed adjustments.
            if (onlineRulesetID == 1 || onlineRulesetID == 3)
                effectPoint.ScrollSpeed = speedMultiplier;

            addControlPoint(time, effectPoint, timingChange);

            addControlPoint(time, new LegacySampleControlPoint
            {
                SampleBank = stringSampleSet,
                SampleVolume = sampleVolume,
                CustomSampleBank = customSampleBank,
            }, timingChange);
        }

        private readonly List<ControlPoint> pendingControlPoints = new List<ControlPoint>();
        private readonly HashSet<Type> pendingControlPointTypes = new HashSet<Type>();
        private double pendingControlPointsTime;
        private bool hasApproachRate;

        private void addControlPoint(double time, ControlPoint point, bool timingChange)
        {
            if (time != pendingControlPointsTime)
                flushPendingPoints();

            if (timingChange)
                pendingControlPoints.Insert(0, point);
            else
                pendingControlPoints.Add(point);

            pendingControlPointsTime = time;
        }

        private void flushPendingPoints()
        {
            // Changes from non-timing-points are added to the end of the list (see addControlPoint()) and should override any changes from timing-points (added to the start of the list).
            for (int i = pendingControlPoints.Count - 1; i >= 0; i--)
            {
                var type = pendingControlPoints[i].GetType();
                if (!pendingControlPointTypes.Add(type))
                    continue;

                beatmap.ControlPointInfo.Add(pendingControlPointsTime, pendingControlPoints[i]);
            }

            pendingControlPoints.Clear();
            pendingControlPointTypes.Clear();
        }

        private void handleHitObject(string line)
        {
            var obj = parser.Parse(line);
            obj.ApplyDefaults(beatmap.ControlPointInfo, beatmap.Difficulty);

            beatmap.HitObjects.Add(obj);
        }

        private int getOffsetTime(int time) => time + (ApplyOffsets ? offset : 0);

        private double getOffsetTime() => ApplyOffsets ? offset : 0;

        private double getOffsetTime(double time) => time + (ApplyOffsets ? offset : 0);

        protected virtual TimingControlPoint CreateTimingControlPoint() => new TimingControlPoint();
    }
}
