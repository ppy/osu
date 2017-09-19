// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using OpenTK.Graphics;
using osu.Game.Beatmaps.Timing;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Rulesets.Objects.Legacy;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Storyboards;
using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.IO.File;

namespace osu.Game.Beatmaps.Formats
{
    public class OsuLegacyDecoder : BeatmapDecoder
    {
        public static void Register()
        {
            AddDecoder<OsuLegacyDecoder>(@"osu file format v14");
            AddDecoder<OsuLegacyDecoder>(@"osu file format v13");
            AddDecoder<OsuLegacyDecoder>(@"osu file format v12");
            AddDecoder<OsuLegacyDecoder>(@"osu file format v11");
            AddDecoder<OsuLegacyDecoder>(@"osu file format v10");
            AddDecoder<OsuLegacyDecoder>(@"osu file format v9");
            AddDecoder<OsuLegacyDecoder>(@"osu file format v8");
            AddDecoder<OsuLegacyDecoder>(@"osu file format v7");
            AddDecoder<OsuLegacyDecoder>(@"osu file format v6");
            AddDecoder<OsuLegacyDecoder>(@"osu file format v5");
            AddDecoder<OsuLegacyDecoder>(@"osu file format v4");
            AddDecoder<OsuLegacyDecoder>(@"osu file format v3");
            // TODO: differences between versions
        }

        private ConvertHitObjectParser parser;

        private readonly Dictionary<string, string> variables = new Dictionary<string, string>();

        private LegacySampleBank defaultSampleBank;
        private int defaultSampleVolume = 100;

        private readonly int beatmapVersion;

        public OsuLegacyDecoder()
        {
        }

        public OsuLegacyDecoder(string header)
        {
            beatmapVersion = int.Parse(header.Substring(17));
        }

        private enum Section
        {
            None,
            General,
            Editor,
            Metadata,
            Difficulty,
            Events,
            TimingPoints,
            Colours,
            HitObjects,
            Variables,
        }

        private void handleGeneral(Beatmap beatmap, string line)
        {
            var pair = splitKeyVal(line, ':');

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
                    metadata.PreviewTime = int.Parse(pair.Value);
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

        private void handleEditor(Beatmap beatmap, string line)
        {
            var pair = splitKeyVal(line, ':');

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

        private void handleMetadata(Beatmap beatmap, string line)
        {
            var pair = splitKeyVal(line, ':');

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
                    metadata.Author = pair.Value;
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
                    beatmap.BeatmapInfo.OnlineBeatmapSetID = int.Parse(pair.Value);
                    metadata.OnlineBeatmapSetID = int.Parse(pair.Value);
                    break;
            }
        }

        private void handleDifficulty(Beatmap beatmap, string line)
        {
            var pair = splitKeyVal(line, ':');

            var difficulty = beatmap.BeatmapInfo.Difficulty;
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
                    difficulty.SliderMultiplier = float.Parse(pair.Value, NumberFormatInfo.InvariantInfo);
                    break;
                case @"SliderTickRate":
                    difficulty.SliderTickRate = float.Parse(pair.Value, NumberFormatInfo.InvariantInfo);
                    break;
            }
        }

        /// <summary>
        /// Decodes any beatmap variables present in a line into their real values.
        /// </summary>
        /// <param name="line">The line which may contains variables.</param>
        private void decodeVariables(ref string line)
        {
            while (line.IndexOf('$') >= 0)
            {
                string origLine = line;
                string[] split = line.Split(',');
                for (int i = 0; i < split.Length; i++)
                {
                    var item = split[i];
                    if (item.StartsWith("$") && variables.ContainsKey(item))
                        split[i] = variables[item];
                }

                line = string.Join(",", split);
                if (line == origLine) break;
            }
        }

        private void handleEvents(Beatmap beatmap, string line, ref StoryboardSprite storyboardSprite, ref CommandTimelineGroup timelineGroup)
        {
            var depth = 0;
            while (line.StartsWith(" ") || line.StartsWith("_"))
            {
                ++depth;
                line = line.Substring(1);
            }

            decodeVariables(ref line);

            string[] split = line.Split(',');

            if (depth == 0)
            {
                storyboardSprite = null;

                EventType type;
                if (!Enum.TryParse(split[0], out type))
                    throw new InvalidDataException($@"Unknown event type {split[0]}");

                switch (type)
                {
                    case EventType.Video:
                    case EventType.Background:
                        string filename = split[2].Trim('"');

                        if (type == EventType.Background)
                            beatmap.BeatmapInfo.Metadata.BackgroundFile = filename;

                        break;
                    case EventType.Break:
                        var breakEvent = new BreakPeriod
                        {
                            StartTime = double.Parse(split[1], NumberFormatInfo.InvariantInfo),
                            EndTime = double.Parse(split[2], NumberFormatInfo.InvariantInfo)
                        };

                        if (!breakEvent.HasEffect)
                            return;

                        beatmap.Breaks.Add(breakEvent);
                        break;
                    case EventType.Sprite:
                        {
                            var layer = parseLayer(split[1]);
                            var origin = parseOrigin(split[2]);
                            var path = cleanFilename(split[3]);
                            var x = float.Parse(split[4], NumberFormatInfo.InvariantInfo);
                            var y = float.Parse(split[5], NumberFormatInfo.InvariantInfo);
                            storyboardSprite = new StoryboardSprite(path, origin, new Vector2(x, y));
                            beatmap.Storyboard.GetLayer(layer).Add(storyboardSprite);
                        }
                        break;
                    case EventType.Animation:
                        {
                            var layer = parseLayer(split[1]);
                            var origin = parseOrigin(split[2]);
                            var path = cleanFilename(split[3]);
                            var x = float.Parse(split[4], NumberFormatInfo.InvariantInfo);
                            var y = float.Parse(split[5], NumberFormatInfo.InvariantInfo);
                            var frameCount = int.Parse(split[6]);
                            var frameDelay = double.Parse(split[7], NumberFormatInfo.InvariantInfo);
                            var loopType = split.Length > 8 ? (AnimationLoopType)Enum.Parse(typeof(AnimationLoopType), split[8]) : AnimationLoopType.LoopForever;
                            storyboardSprite = new StoryboardAnimation(path, origin, new Vector2(x, y), frameCount, frameDelay, loopType);
                            beatmap.Storyboard.GetLayer(layer).Add(storyboardSprite);
                        }
                        break;
                    case EventType.Sample:
                        {
                            var time = double.Parse(split[1], CultureInfo.InvariantCulture);
                            var layer = parseLayer(split[2]);
                            var path = cleanFilename(split[3]);
                            var volume = split.Length > 4 ? float.Parse(split[4], CultureInfo.InvariantCulture) : 100;
                            beatmap.Storyboard.GetLayer(layer).Add(new StoryboardSample(path, time, volume));
                        }
                        break;
                }
            }
            else
            {
                if (depth < 2)
                    timelineGroup = storyboardSprite?.TimelineGroup;

                var commandType = split[0];
                switch (commandType)
                {
                    case "T":
                        {
                            var triggerName = split[1];
                            var startTime = split.Length > 2 ? double.Parse(split[2], CultureInfo.InvariantCulture) : double.MinValue;
                            var endTime = split.Length > 3 ? double.Parse(split[3], CultureInfo.InvariantCulture) : double.MaxValue;
                            var groupNumber = split.Length > 4 ? int.Parse(split[4]) : 0;
                            timelineGroup = storyboardSprite?.AddTrigger(triggerName, startTime, endTime, groupNumber);
                        }
                        break;
                    case "L":
                        {
                            var startTime = double.Parse(split[1], CultureInfo.InvariantCulture);
                            var loopCount = int.Parse(split[2]);
                            timelineGroup = storyboardSprite?.AddLoop(startTime, loopCount);
                        }
                        break;
                    default:
                        {
                            if (string.IsNullOrEmpty(split[3]))
                                split[3] = split[2];

                            var easing = (Easing)int.Parse(split[1]);
                            var startTime = double.Parse(split[2], CultureInfo.InvariantCulture);
                            var endTime = double.Parse(split[3], CultureInfo.InvariantCulture);

                            switch (commandType)
                            {
                                case "F":
                                    {
                                        var startValue = float.Parse(split[4], CultureInfo.InvariantCulture);
                                        var endValue = split.Length > 5 ? float.Parse(split[5], CultureInfo.InvariantCulture) : startValue;
                                        timelineGroup?.Alpha.Add(easing, startTime, endTime, startValue, endValue);
                                    }
                                    break;
                                case "S":
                                    {
                                        var startValue = float.Parse(split[4], CultureInfo.InvariantCulture);
                                        var endValue = split.Length > 5 ? float.Parse(split[5], CultureInfo.InvariantCulture) : startValue;
                                        timelineGroup?.Scale.Add(easing, startTime, endTime, new Vector2(startValue), new Vector2(endValue));
                                    }
                                    break;
                                case "V":
                                    {
                                        var startX = float.Parse(split[4], CultureInfo.InvariantCulture);
                                        var startY = float.Parse(split[5], CultureInfo.InvariantCulture);
                                        var endX = split.Length > 6 ? float.Parse(split[6], CultureInfo.InvariantCulture) : startX;
                                        var endY = split.Length > 7 ? float.Parse(split[7], CultureInfo.InvariantCulture) : startY;
                                        timelineGroup?.Scale.Add(easing, startTime, endTime, new Vector2(startX, startY), new Vector2(endX, endY));
                                    }
                                    break;
                                case "R":
                                    {
                                        var startValue = float.Parse(split[4], CultureInfo.InvariantCulture);
                                        var endValue = split.Length > 5 ? float.Parse(split[5], CultureInfo.InvariantCulture) : startValue;
                                        timelineGroup?.Rotation.Add(easing, startTime, endTime, MathHelper.RadiansToDegrees(startValue), MathHelper.RadiansToDegrees(endValue));
                                    }
                                    break;
                                case "M":
                                    {
                                        var startX = float.Parse(split[4], CultureInfo.InvariantCulture);
                                        var startY = float.Parse(split[5], CultureInfo.InvariantCulture);
                                        var endX = split.Length > 6 ? float.Parse(split[6], CultureInfo.InvariantCulture) : startX;
                                        var endY = split.Length > 7 ? float.Parse(split[7], CultureInfo.InvariantCulture) : startY;
                                        timelineGroup?.X.Add(easing, startTime, endTime, startX, endX);
                                        timelineGroup?.Y.Add(easing, startTime, endTime, startY, endY);
                                    }
                                    break;
                                case "MX":
                                    {
                                        var startValue = float.Parse(split[4], CultureInfo.InvariantCulture);
                                        var endValue = split.Length > 5 ? float.Parse(split[5], CultureInfo.InvariantCulture) : startValue;
                                        timelineGroup?.X.Add(easing, startTime, endTime, startValue, endValue);
                                    }
                                    break;
                                case "MY":
                                    {
                                        var startValue = float.Parse(split[4], CultureInfo.InvariantCulture);
                                        var endValue = split.Length > 5 ? float.Parse(split[5], CultureInfo.InvariantCulture) : startValue;
                                        timelineGroup?.Y.Add(easing, startTime, endTime, startValue, endValue);
                                    }
                                    break;
                                case "C":
                                    {
                                        var startRed = float.Parse(split[4], CultureInfo.InvariantCulture);
                                        var startGreen = float.Parse(split[5], CultureInfo.InvariantCulture);
                                        var startBlue = float.Parse(split[6], CultureInfo.InvariantCulture);
                                        var endRed = split.Length > 7 ? float.Parse(split[7], CultureInfo.InvariantCulture) : startRed;
                                        var endGreen = split.Length > 8 ? float.Parse(split[8], CultureInfo.InvariantCulture) : startGreen;
                                        var endBlue = split.Length > 9 ? float.Parse(split[9], CultureInfo.InvariantCulture) : startBlue;
                                        timelineGroup?.Colour.Add(easing, startTime, endTime,
                                            new Color4(startRed / 255f, startGreen / 255f, startBlue / 255f, 1),
                                            new Color4(endRed / 255f, endGreen / 255f, endBlue / 255f, 1));
                                    }
                                    break;
                                case "P":
                                    {
                                        var type = split[4];
                                        switch (type)
                                        {
                                            case "A": timelineGroup?.BlendingMode.Add(easing, startTime, endTime, BlendingMode.Additive, startTime == endTime ? BlendingMode.Additive : BlendingMode.Inherit); break;
                                            case "H": timelineGroup?.FlipH.Add(easing, startTime, endTime, true, startTime == endTime); break;
                                            case "V": timelineGroup?.FlipV.Add(easing, startTime, endTime, true, startTime == endTime); break;
                                        }
                                    }
                                    break;
                                default:
                                    throw new InvalidDataException($@"Unknown command type: {commandType}");
                            }
                        }
                        break;
                }
            }
        }

        private static string cleanFilename(string path)
            => FileSafety.PathStandardise(path.Trim('\"'));

        private static Anchor parseOrigin(string value)
        {
            var origin = (LegacyOrigins)Enum.Parse(typeof(LegacyOrigins), value);
            switch (origin)
            {
                case LegacyOrigins.TopLeft: return Anchor.TopLeft;
                case LegacyOrigins.TopCentre: return Anchor.TopCentre;
                case LegacyOrigins.TopRight: return Anchor.TopRight;
                case LegacyOrigins.CentreLeft: return Anchor.CentreLeft;
                case LegacyOrigins.Centre: return Anchor.Centre;
                case LegacyOrigins.CentreRight: return Anchor.CentreRight;
                case LegacyOrigins.BottomLeft: return Anchor.BottomLeft;
                case LegacyOrigins.BottomCentre: return Anchor.BottomCentre;
                case LegacyOrigins.BottomRight: return Anchor.BottomRight;
            }
            throw new InvalidDataException($@"Unknown origin: {value}");
        }

        private static string parseLayer(string value)
            => Enum.Parse(typeof(StoryLayer), value).ToString();

        private void handleTimingPoints(Beatmap beatmap, string line)
        {
            string[] split = line.Split(',');

            double time = double.Parse(split[0].Trim(), NumberFormatInfo.InvariantInfo);
            double beatLength = double.Parse(split[1].Trim(), NumberFormatInfo.InvariantInfo);
            double speedMultiplier = beatLength < 0 ? 100.0 / -beatLength : 1;

            TimeSignatures timeSignature = TimeSignatures.SimpleQuadruple;
            if (split.Length >= 3)
                timeSignature = split[2][0] == '0' ? TimeSignatures.SimpleQuadruple : (TimeSignatures)int.Parse(split[2]);

            LegacySampleBank sampleSet = defaultSampleBank;
            if (split.Length >= 4)
                sampleSet = (LegacySampleBank)int.Parse(split[3]);

            //SampleBank sampleBank = SampleBank.Default;
            //if (split.Length >= 5)
            //    sampleBank = (SampleBank)int.Parse(split[4]);

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

            DifficultyControlPoint difficultyPoint = beatmap.ControlPointInfo.DifficultyPointAt(time);
            SoundControlPoint soundPoint = beatmap.ControlPointInfo.SoundPointAt(time);
            EffectControlPoint effectPoint = beatmap.ControlPointInfo.EffectPointAt(time);

            if (timingChange)
            {
                beatmap.ControlPointInfo.TimingPoints.Add(new TimingControlPoint
                {
                    Time = time,
                    BeatLength = beatLength,
                    TimeSignature = timeSignature
                });
            }

            if (speedMultiplier != difficultyPoint.SpeedMultiplier)
            {
                beatmap.ControlPointInfo.DifficultyPoints.RemoveAll(x => x.Time == time);
                beatmap.ControlPointInfo.DifficultyPoints.Add(new DifficultyControlPoint
                {
                    Time = time,
                    SpeedMultiplier = speedMultiplier
                });
            }

            if (stringSampleSet != soundPoint.SampleBank || sampleVolume != soundPoint.SampleVolume)
            {
                beatmap.ControlPointInfo.SoundPoints.Add(new SoundControlPoint
                {
                    Time = time,
                    SampleBank = stringSampleSet,
                    SampleVolume = sampleVolume
                });
            }

            if (kiaiMode != effectPoint.KiaiMode || omitFirstBarSignature != effectPoint.OmitFirstBarLine)
            {
                beatmap.ControlPointInfo.EffectPoints.Add(new EffectControlPoint
                {
                    Time = time,
                    KiaiMode = kiaiMode,
                    OmitFirstBarLine = omitFirstBarSignature
                });
            }
        }

        private void handleColours(Beatmap beatmap, string line, ref bool hasCustomColours)
        {
            var pair = splitKeyVal(line, ':');

            string[] split = pair.Value.Split(',');

            if (split.Length != 3)
                throw new InvalidOperationException($@"Color specified in incorrect format (should be R,G,B): {pair.Value}");

            byte r, g, b;
            if (!byte.TryParse(split[0], out r) || !byte.TryParse(split[1], out g) || !byte.TryParse(split[2], out b))
                throw new InvalidOperationException(@"Color must be specified with 8-bit integer components");

            if (!hasCustomColours)
            {
                beatmap.ComboColors.Clear();
                hasCustomColours = true;
            }

            // Note: the combo index specified in the beatmap is discarded
            if (pair.Key.StartsWith(@"Combo"))
            {
                beatmap.ComboColors.Add(new Color4
                {
                    R = r / 255f,
                    G = g / 255f,
                    B = b / 255f,
                    A = 1f,
                });
            }
        }

        private void handleVariables(string line)
        {
            var pair = splitKeyVal(line, '=');
            variables[pair.Key] = pair.Value;
        }

        protected override Beatmap ParseFile(StreamReader stream)
        {
            return new LegacyBeatmap(base.ParseFile(stream));
        }

        public override Beatmap Decode(StreamReader stream)
        {
            return new LegacyBeatmap(base.Decode(stream));
        }

        protected override void ParseFile(StreamReader stream, Beatmap beatmap)
        {
            beatmap.BeatmapInfo.BeatmapVersion = beatmapVersion;

            Section section = Section.None;
            bool hasCustomColours = false;
            StoryboardSprite storyboardSprite = null;
            CommandTimelineGroup timelineGroup = null;

            string line;
            while ((line = stream.ReadLine()) != null)
            {
                if (string.IsNullOrEmpty(line))
                    continue;

                if (line.StartsWith("//"))
                    continue;

                if (line.StartsWith(@"osu file format v"))
                {
                    beatmap.BeatmapInfo.BeatmapVersion = int.Parse(line.Substring(17));
                    continue;
                }

                if (line.StartsWith(@"[") && line.EndsWith(@"]"))
                {
                    if (!Enum.TryParse(line.Substring(1, line.Length - 2), out section))
                        throw new InvalidDataException($@"Unknown osu section {line}");
                    continue;
                }

                switch (section)
                {
                    case Section.General:
                        handleGeneral(beatmap, line);
                        break;
                    case Section.Editor:
                        handleEditor(beatmap, line);
                        break;
                    case Section.Metadata:
                        handleMetadata(beatmap, line);
                        break;
                    case Section.Difficulty:
                        handleDifficulty(beatmap, line);
                        break;
                    case Section.Events:
                        handleEvents(beatmap, line, ref storyboardSprite, ref timelineGroup);
                        break;
                    case Section.TimingPoints:
                        handleTimingPoints(beatmap, line);
                        break;
                    case Section.Colours:
                        handleColours(beatmap, line, ref hasCustomColours);
                        break;
                    case Section.HitObjects:

                        // If the ruleset wasn't specified, assume the osu!standard ruleset.
                        if (parser == null)
                            parser = new Rulesets.Objects.Legacy.Osu.ConvertHitObjectParser();

                        var obj = parser.Parse(line);

                        if (obj != null)
                            beatmap.HitObjects.Add(obj);

                        break;
                    case Section.Variables:
                        handleVariables(line);
                        break;
                }
            }

            foreach (var hitObject in beatmap.HitObjects)
                hitObject.ApplyDefaults(beatmap.ControlPointInfo, beatmap.BeatmapInfo.Difficulty);
        }

        private KeyValuePair<string, string> splitKeyVal(string line, char separator)
        {
            return new KeyValuePair<string, string>
            (
                line.Remove(line.IndexOf(separator)).Trim(),
                line.Substring(line.IndexOf(separator) + 1).Trim()
            );
        }

        internal enum LegacySampleBank
        {
            None = 0,
            Normal = 1,
            Soft = 2,
            Drum = 3
        }

        internal enum EventType
        {
            Background = 0,
            Video = 1,
            Break = 2,
            Colour = 3,
            Sprite = 4,
            Sample = 5,
            Animation = 6
        }

        internal enum LegacyOrigins
        {
            TopLeft,
            Centre,
            CentreLeft,
            TopRight,
            BottomCentre,
            TopCentre,
            Custom,
            CentreRight,
            BottomLeft,
            BottomRight
        };

        internal enum StoryLayer
        {
            Background = 0,
            Fail = 1,
            Pass = 2,
            Foreground = 3
        }
    }
}
