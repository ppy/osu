using System;
using System.Collections.Generic;
using System.IO;
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
                    beatmap.Metadata.Mode = (PlayMode)int.Parse(val);
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

        public override Beatmap Decode(TextReader stream)
        {
            var beatmap = new Beatmap
            {
                Metadata = new BeatmapMetadata(),
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
                        throw new InvalidOperationException($@"Unknown osu section {line}");
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
                        // TODO
                        break;
                    case Section.Difficulty:
                        // TODO
                        break;
                    case Section.Events:
                        // TODO
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