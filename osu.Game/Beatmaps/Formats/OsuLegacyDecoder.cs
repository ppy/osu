//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using OpenTK.Graphics;
using osu.Game.Database;
using osu.Game.Beatmaps.Events;
using osu.Game.Beatmaps.Samples;
using osu.Game.Beatmaps.Timing;
using osu.Game.Modes;
using osu.Game.Modes.Objects;
using osu.Game.Screens.Play;

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
            // TODO: Not sure how far back to go, or differences between versions
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
        }

        private void handleGeneral(Beatmap beatmap, string key, string val)
        {
            var metadata = beatmap.BeatmapInfo.Metadata;
            switch (key)
            {
                case @"AudioFilename":
                    metadata.AudioFile = val;
                    break;
                case @"AudioLeadIn":
                    beatmap.BeatmapInfo.AudioLeadIn = int.Parse(val);
                    break;
                case @"PreviewTime":
                    metadata.PreviewTime = int.Parse(val);
                    break;
                case @"Countdown":
                    beatmap.BeatmapInfo.Countdown = int.Parse(val) == 1;
                    break;
                case @"SampleSet":
                    beatmap.BeatmapInfo.SampleSet = (SampleSet)Enum.Parse(typeof(SampleSet), val);
                    break;
                case @"StackLeniency":
                    beatmap.BeatmapInfo.StackLeniency = float.Parse(val, NumberFormatInfo.InvariantInfo);
                    break;
                case @"Mode":
                    beatmap.BeatmapInfo.Mode = (PlayMode)int.Parse(val);
                    break;
                case @"LetterboxInBreaks":
                    beatmap.BeatmapInfo.LetterboxInBreaks = int.Parse(val) == 1;
                    break;
                case @"SpecialStyle":
                    beatmap.BeatmapInfo.SpecialStyle = int.Parse(val) == 1;
                    break;
                case @"WidescreenStoryboard":
                    beatmap.BeatmapInfo.WidescreenStoryboard = int.Parse(val) == 1;
                    break;
            }
        }

        private void handleEditor(Beatmap beatmap, string key, string val)
        {
            switch (key)
            {
                case @"Bookmarks":
                    beatmap.BeatmapInfo.StoredBookmarks = val;
                    break;
                case @"DistanceSpacing":
                    beatmap.BeatmapInfo.DistanceSpacing = double.Parse(val, NumberFormatInfo.InvariantInfo);
                    break;
                case @"BeatDivisor":
                    beatmap.BeatmapInfo.BeatDivisor = int.Parse(val);
                    break;
                case @"GridSize":
                    beatmap.BeatmapInfo.GridSize = int.Parse(val);
                    break;
                case @"TimelineZoom":
                    beatmap.BeatmapInfo.TimelineZoom = double.Parse(val, NumberFormatInfo.InvariantInfo);
                    break;
            }
        }

        private void handleMetadata(Beatmap beatmap, string key, string val)
        {
            var metadata = beatmap.BeatmapInfo.Metadata;
            switch (key)
            {
                case @"Title":
                    metadata.Title = val;
                    break;
                case @"TitleUnicode":
                    metadata.TitleUnicode = val;
                    break;
                case @"Artist":
                    metadata.Artist = val;
                    break;
                case @"ArtistUnicode":
                    metadata.ArtistUnicode = val;
                    break;
                case @"Creator":
                    metadata.Author = val;
                    break;
                case @"Version":
                    beatmap.BeatmapInfo.Version = val;
                    break;
                case @"Source":
                    beatmap.BeatmapInfo.Metadata.Source = val;
                    break;
                case @"Tags":
                    beatmap.BeatmapInfo.Metadata.Tags = val;
                    break;
                case @"BeatmapID":
                    beatmap.BeatmapInfo.OnlineBeatmapID = int.Parse(val);
                    break;
                case @"BeatmapSetID":
                    beatmap.BeatmapInfo.OnlineBeatmapSetID = int.Parse(val);
                    metadata.OnlineBeatmapSetID = int.Parse(val);
                    break;
            }
        }

        private void handleDifficulty(Beatmap beatmap, string key, string val)
        {
            var difficulty = beatmap.BeatmapInfo.BaseDifficulty;
            switch (key)
            {
                case @"HPDrainRate":
                    difficulty.DrainRate = float.Parse(val, NumberFormatInfo.InvariantInfo);
                    break;
                case @"CircleSize":
                    difficulty.CircleSize = float.Parse(val, NumberFormatInfo.InvariantInfo);
                    break;
                case @"OverallDifficulty":
                    difficulty.OverallDifficulty = float.Parse(val, NumberFormatInfo.InvariantInfo);
                    break;
                case @"ApproachRate":
                    difficulty.ApproachRate = float.Parse(val, NumberFormatInfo.InvariantInfo);
                    break;
                case @"SliderMultiplier":
                    difficulty.SliderMultiplier = float.Parse(val, NumberFormatInfo.InvariantInfo);
                    break;
                case @"SliderTickRate":
                    difficulty.SliderTickRate = float.Parse(val, NumberFormatInfo.InvariantInfo);
                    break;
            }
        }

        private void handleEvents(Beatmap beatmap, string val)
        {
            if (val.StartsWith(@"//"))
                return;
            if (val.StartsWith(@" "))
                return; // TODO
            string[] split = val.Split(',');
            EventType type;
            int _type;
            if (!int.TryParse(split[0], out _type))
            {
                if (!Enum.TryParse(split[0], out type))
                    throw new InvalidDataException($@"Unknown event type {split[0]}");
            }
            else
                type = (EventType)_type;
            // TODO: Parse and store the rest of the event
            if (type == EventType.Background)
                beatmap.BeatmapInfo.Metadata.BackgroundFile = split[2].Trim('"');
        }

        private void handleTimingPoints(Beatmap beatmap, string val)
        {
            ControlPoint cp = null;

            string[] split = val.Split(',');

            if (split.Length > 2)
            {
                int kiai_flags = split.Length > 7 ? Convert.ToInt32(split[7], NumberFormatInfo.InvariantInfo) : 0;
                double beatLength = double.Parse(split[1].Trim(), NumberFormatInfo.InvariantInfo);
                cp = new ControlPoint
                {
                    Time = double.Parse(split[0].Trim(), NumberFormatInfo.InvariantInfo),
                    BeatLength = beatLength > 0 ? beatLength : 0,
                    VelocityAdjustment = beatLength < 0 ? -beatLength / 100.0 : 1,
                    TimingChange = split.Length <= 6 || split[6][0] == '1',
                };
            }

            if (cp != null)
                beatmap.ControlPoints.Add(cp);
        }

        private void handleColours(Beatmap beatmap, string key, string val)
        {
            string[] split = val.Split(',');
            if (split.Length != 3)
                throw new InvalidOperationException($@"Color specified in incorrect format (should be R,G,B): {val}");
            byte r, g, b;
            if (!byte.TryParse(split[0], out r) || !byte.TryParse(split[1], out g) || !byte.TryParse(split[2], out b))
                throw new InvalidOperationException($@"Color must be specified with 8-bit integer components");
            // Note: the combo index specified in the beatmap is discarded
            beatmap.ComboColors.Add(new Color4
            {
                R = r / 255f,
                G = g / 255f,
                B = b / 255f,
                A = 1f,
            });
        }

        protected override Beatmap ParseFile(TextReader stream)
        {
            var beatmap = new Beatmap
            {
                HitObjects = new List<HitObject>(),
                ControlPoints = new List<ControlPoint>(),
                ComboColors = new List<Color4>(),
                BeatmapInfo = new BeatmapInfo
                {
                    Metadata = new BeatmapMetadata(),
                    BaseDifficulty = new BaseDifficulty(),
                },
            };

            HitObjectParser parser = null;

            var section = Section.None;
            string line;
            while (true)
            {
                line = stream.ReadLine();
                if (line == null)
                    break;
                if (string.IsNullOrEmpty(line))
                    continue;
                if (line.StartsWith(@"osu file format v"))
                    continue;

                if (line.StartsWith(@"[") && line.EndsWith(@"]"))
                {
                    if (!Enum.TryParse(line.Substring(1, line.Length - 2), out section))
                        throw new InvalidDataException($@"Unknown osu section {line}");
                    continue;
                }

                string val = line, key = null;
                if (section != Section.Events && section != Section.TimingPoints && section != Section.HitObjects)
                {
                    key = val.Remove(val.IndexOf(':')).Trim();
                    val = val.Substring(val.IndexOf(':') + 1).Trim();
                }
                switch (section)
                {
                    case Section.General:
                        handleGeneral(beatmap, key, val);
                        parser = Ruleset.GetRuleset(beatmap.BeatmapInfo.Mode).CreateHitObjectParser();
                        break;
                    case Section.Editor:
                        handleEditor(beatmap, key, val);
                        break;
                    case Section.Metadata:
                        handleMetadata(beatmap, key, val);
                        break;
                    case Section.Difficulty:
                        handleDifficulty(beatmap, key, val);
                        break;
                    case Section.Events:
                        handleEvents(beatmap, val);
                        break;
                    case Section.TimingPoints:
                        handleTimingPoints(beatmap, val);
                        break;
                    case Section.Colours:
                        handleColours(beatmap, key, val);
                        break;
                    case Section.HitObjects:
                        var obj = parser?.Parse(val);

                        if (obj != null)
                        {
                            obj.SetDefaultsFromBeatmap(beatmap);
                            beatmap.HitObjects.Add(obj);
                        }
                        break;
                }
            }

            return beatmap;
        }
    }
}
