using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using OpenTK.Graphics;
using osu.Game.Beatmaps.Events;
using osu.Game.Beatmaps.Objects;
using osu.Game.Beatmaps.Samples;
using osu.Game.Beatmaps.Timing;
using osu.Game.GameModes.Play;

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
            switch (key)
            {
                case @"AudioFilename":
                    beatmap.Metadata.AudioFile = val;
                    break;
                case @"AudioLeadIn":
                    beatmap.AudioLeadIn = int.Parse(val);
                    break;
                case @"PreviewTime":
                    beatmap.Metadata.PreviewTime = int.Parse(val);
                    break;
                case @"Countdown":
                    beatmap.Countdown = int.Parse(val) == 1;
                    break;
                case @"SampleSet":
                    beatmap.SampleSet = (SampleSet)Enum.Parse(typeof(SampleSet), val);
                    break;
                case @"StackLeniency":
                    beatmap.StackLeniency = float.Parse(val, NumberFormatInfo.InvariantInfo);
                    break;
                case @"Mode":
                    beatmap.Mode = (PlayMode)int.Parse(val);
                    break;
                case @"LetterboxInBreaks":
                    beatmap.LetterboxInBreaks = int.Parse(val) == 1;
                    break;
                case @"SpecialStyle":
                    beatmap.SpecialStyle = int.Parse(val) == 1;
                    break;
                case @"WidescreenStoryboard":
                    beatmap.WidescreenStoryboard = int.Parse(val) == 1;
                    break;
            }
        }

        private void handleEditor(Beatmap beatmap, string key, string val)
        {
            switch (key)
            {
                case @"Bookmarks":
                    beatmap.StoredBookmarks = val;
                    break;
                case @"DistanceSpacing":
                    beatmap.DistanceSpacing = double.Parse(val, NumberFormatInfo.InvariantInfo);
                    break;
                case @"BeatDivisor":
                    beatmap.BeatDivisor = int.Parse(val);
                    break;
                case @"GridSize":
                    beatmap.GridSize = int.Parse(val);
                    break;
                case @"TimelineZoom":
                    beatmap.TimelineZoom = double.Parse(val, NumberFormatInfo.InvariantInfo);
                    break;
            }
        }

        private void handleMetadata(Beatmap beatmap, string key, string val)
        {
            switch (key)
            {
                case @"Title":
                    beatmap.Metadata.Title = val;
                    break;
                case @"TitleUnicode":
                    beatmap.Metadata.TitleUnicode = val;
                    break;
                case @"Artist":
                    beatmap.Metadata.Artist = val;
                    break;
                case @"ArtistUnicode":
                    beatmap.Metadata.ArtistUnicode = val;
                    break;
                case @"Creator":
                    beatmap.Metadata.Author = val;
                    break;
                case @"Version":
                    beatmap.Version = val;
                    break;
                case @"Source":
                    beatmap.Metadata.Source = val;
                    break;
                case @"Tags":
                    beatmap.Metadata.Tags = val;
                    break;
                case @"BeatmapID":
                    beatmap.BeatmapID = int.Parse(val);
                    break;
                case @"BeatmapSetID":
                    beatmap.BeatmapSetID = int.Parse(val);
                    beatmap.Metadata.BeatmapSetID = int.Parse(val);
                    break;
            }
        }

        private void handleDifficulty(Beatmap beatmap, string key, string val)
        {
            switch (key)
            {
                case @"HPDrainRate":
                    beatmap.BaseDifficulty.DrainRate = float.Parse(val, NumberFormatInfo.InvariantInfo);
                    break;
                case @"CircleSize":
                    beatmap.BaseDifficulty.CircleSize = float.Parse(val, NumberFormatInfo.InvariantInfo);
                    break;
                case @"OverallDifficulty":
                    beatmap.BaseDifficulty.OverallDifficulty = float.Parse(val, NumberFormatInfo.InvariantInfo);
                    break;
                case @"ApproachRate":
                    beatmap.BaseDifficulty.ApproachRate = float.Parse(val, NumberFormatInfo.InvariantInfo);
                    break;
                case @"SliderMultiplier":
                    beatmap.BaseDifficulty.SliderMultiplier = float.Parse(val, NumberFormatInfo.InvariantInfo);
                    break;
                case @"SliderTickRate":
                    beatmap.BaseDifficulty.SliderTickRate = float.Parse(val, NumberFormatInfo.InvariantInfo);
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
                beatmap.Metadata.BackgroundFile = split[2].Trim('"');
        }

        private void handleTimingPoints(Beatmap beatmap, string val)
        {
            // TODO
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

        public override void Decode(TextReader stream, Beatmap beatmap)
        {
            // We don't overwrite these two because they're DB bound
            if (beatmap.Metadata == null) beatmap.Metadata = new BeatmapMetadata();
            if (beatmap.BaseDifficulty == null) beatmap.BaseDifficulty = new BaseDifficulty();
            // These are fine though
            beatmap.HitObjects = new List<HitObject>();
            beatmap.ControlPoints = new List<ControlPoint>();
            beatmap.ComboColors = new List<Color4>();
            
            var section = Section.None;
            string line;
            while (true)
            {
                line = stream.ReadLine();
                if (line == null)
                    break;
                line = line.Trim();
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
                        beatmap.HitObjects.Add(HitObject.Parse(beatmap.Mode, val));
                        break;
                }
            }
        }
    }
}