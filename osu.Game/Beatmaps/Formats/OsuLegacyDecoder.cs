using System;
using System.Collections.Generic;
using System.IO;
using osu.Game.Beatmaps.Events;
using osu.Game.Beatmaps.Objects;
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
                    // TODO
                    break;
                case "PreviewTime":
                    beatmap.Metadata.PreviewTime = int.Parse(val);
                    break;
                case "Countdown":
                    // TODO
                    break;
                case "SampleSet":
                    // TODO
                    break;
                case "StackLeniency":
                    // TODO
                    break;
                case "Mode":
                    beatmap.Mode = (PlayMode)int.Parse(val);
                    break;
                case "LetterboxInBreaks":
                    // TODO
                    break;
                case "SpecialStyle":
                    // TODO
                    break;
                case "WidescreenStoryboard":
                    // TODO
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
                        // TODO
                        break;
                    case Section.Metadata:
                        HandleMetadata(beatmap, key, val);
                        break;
                    case Section.Difficulty:
                        // TODO
                        break;
                    case Section.Events:
                        HandleEvents(beatmap, val);
                        break;
                    case Section.TimingPoints:
                        // TODO
                        break;
                    case Section.HitObjects:
                        // TODO
                        break;
                }
            }
            return beatmap;
        }
    }
}