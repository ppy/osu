using System;
using System.Collections.Generic;
using System.IO;
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
            AddDecoder<OsuLegacyDecoder>("osu file format v14");
            AddDecoder<OsuLegacyDecoder>("osu file format v13");
            AddDecoder<OsuLegacyDecoder>("osu file format v12");
            AddDecoder<OsuLegacyDecoder>("osu file format v11");
            AddDecoder<OsuLegacyDecoder>("osu file format v10");
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

        private void HandleGeneral(Beatmap beatmap, string key, string val)
        {
            switch (key)
            {
                case "AudioFilename":
                    beatmap.Metadata.AudioFile = val;
                    break;
                case "AudioLeadIn":
                    beatmap.AudioLeadIn = int.Parse(val);
                    break;
                case "PreviewTime":
                    beatmap.Metadata.PreviewTime = int.Parse(val);
                    break;
                case "Countdown":
                    beatmap.Countdown = int.Parse(val) == 1;
                    break;
                case "SampleSet":
                    beatmap.SampleSet = (SampleSet)Enum.Parse(typeof(SampleSet), val);
                    break;
                case "StackLeniency":
                    beatmap.StackLeniency = float.Parse(val);
                    break;
                case "Mode":
                    beatmap.Mode = (PlayMode)int.Parse(val);
                    break;
                case "LetterboxInBreaks":
                    beatmap.LetterboxInBreaks = int.Parse(val) == 1;
                    break;
                case "SpecialStyle":
                    beatmap.SpecialStyle = int.Parse(val) == 1;
                    break;
                case "WidescreenStoryboard":
                    beatmap.WidescreenStoryboard = int.Parse(val) == 1;
                    break;
            }
        }

        private void HandleEditor(Beatmap beatmap, string key, string val)
        {
            switch (key)
            {
                case "Bookmarks":
                    beatmap.StoredBookmarks = val;
                    break;
                case "DistanceSpacing":
                    beatmap.DistanceSpacing = double.Parse(val);
                    break;
                case "BeatDivisor":
                    beatmap.BeatDivisor = int.Parse(val);
                    break;
                case "GridSize":
                    beatmap.GridSize = int.Parse(val);
                    break;
                case "TimelineZoom":
                    beatmap.TimelineZoom = double.Parse(val);
                    break;
            }
        }

        private void HandleMetadata(Beatmap beatmap, string key, string val)
        {
            switch (key)
            {
                case "Title":
                    beatmap.Metadata.Title = val;
                    break;
                case "TitleUnicode":
                    beatmap.Metadata.TitleUnicode = val;
                    break;
                case "Artist":
                    beatmap.Metadata.Artist = val;
                    break;
                case "ArtistUnicode":
                    beatmap.Metadata.ArtistUnicode = val;
                    break;
                case "Creator":
                    beatmap.Metadata.Author = val;
                    break;
                case "Version":
                    beatmap.Version = val;
                    break;
                case "Source":
                    beatmap.Metadata.Source = val;
                    break;
                case "Tags":
                    beatmap.Metadata.Tags = val;
                    break;
                case "BeatmapID":
                    beatmap.BeatmapID = int.Parse(val);
                    break;
                case "BeatmapSetID":
                    beatmap.BeatmapSetID = int.Parse(val);
                    beatmap.Metadata.BeatmapSetID = int.Parse(val);
                    break;
            }
        }

        private void HandleDifficulty(Beatmap beatmap, string key, string val)
        {
            switch (key)
            {
                case "HPDrainRate":
                    beatmap.BaseDifficulty.DrainRate = float.Parse(val);
                    break;
                case "CircleSize":
                    beatmap.BaseDifficulty.CircleSize = float.Parse(val);
                    break;
                case "OverallDifficulty":
                    beatmap.BaseDifficulty.OverallDifficulty = float.Parse(val);
                    break;
                case "ApproachRate":
                    beatmap.BaseDifficulty.ApproachRate = float.Parse(val);
                    break;
                case "SliderMultiplier":
                    beatmap.BaseDifficulty.SliderMultiplier = float.Parse(val);
                    break;
                case "SliderTickRate":
                    beatmap.BaseDifficulty.SliderTickRate = float.Parse(val);
                    break;
            }
        }

        private void HandleEvents(Beatmap beatmap, string val)
        {
            if (val.StartsWith("//"))
                return;
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

        private void HandleTimingPoints(Beatmap beatmap, string val)
        {
            // TODO
        }

        public override Beatmap Decode(TextReader stream)
        {
            var beatmap = new Beatmap
            {
                Metadata = new BeatmapMetadata(),
                BaseDifficulty = new BaseDifficulty(),
                HitObjects = new List<HitObject>(),
                ControlPoints = new List<ControlPoint>(),
            };
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
                if (line.StartsWith("osu file format v"))
                    continue;
                    
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    if (!Enum.TryParse(line.Substring(1, line.Length - 2), out section))
                        throw new InvalidDataException($@"Unknown osu section {line}");
                    continue;
                }
                
                string val = line, key = null;
                if (section != Section.Events && section != Section.TimingPoints
                    && section != Section.HitObjects)
                {
                    key = val.Remove(val.IndexOf(':')).Trim();
                    val = val.Substring(val.IndexOf(':') + 1).Trim();
                }
                switch (section)
                {
                    case Section.General:
                        HandleGeneral(beatmap, key, val);
                        break;
                    case Section.Editor:
                        HandleEditor(beatmap, key, val);
                        break;
                    case Section.Metadata:
                        HandleMetadata(beatmap, key, val);
                        break;
                    case Section.Difficulty:
                        HandleDifficulty(beatmap, key, val);
                        break;
                    case Section.Events:
                        HandleEvents(beatmap, val);
                        break;
                    case Section.TimingPoints:
                        HandleTimingPoints(beatmap, val);
                        break;
                    case Section.HitObjects:
                        beatmap.HitObjects.Add(HitObject.Parse(beatmap.Mode, val));
                        break;
                }
            }
            return beatmap;
        }
    }
}