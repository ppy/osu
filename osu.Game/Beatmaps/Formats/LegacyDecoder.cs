// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.IO;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Storyboards;

namespace osu.Game.Beatmaps.Formats
{
    public abstract class LegacyDecoder : Decoder
    {
        public static void Register()
        {
            AddDecoder(@"osu file format v14", m => new LegacyBeatmapDecoder(m));
            AddDecoder(@"osu file format v13", m => new LegacyBeatmapDecoder(m));
            AddDecoder(@"osu file format v12", m => new LegacyBeatmapDecoder(m));
            AddDecoder(@"osu file format v11", m => new LegacyBeatmapDecoder(m));
            AddDecoder(@"osu file format v10", m => new LegacyBeatmapDecoder(m));
            AddDecoder(@"osu file format v9", m => new LegacyBeatmapDecoder(m));
            AddDecoder(@"osu file format v8", m => new LegacyBeatmapDecoder(m));
            AddDecoder(@"osu file format v7", m => new LegacyBeatmapDecoder(m));
            AddDecoder(@"osu file format v6", m => new LegacyBeatmapDecoder(m));
            AddDecoder(@"osu file format v5", m => new LegacyBeatmapDecoder(m));
            AddDecoder(@"osu file format v4", m => new LegacyBeatmapDecoder(m));
            AddDecoder(@"osu file format v3", m => new LegacyBeatmapDecoder(m));
            // TODO: differences between versions
        }

        protected int BeatmapVersion;

        public override Decoder GetStoryboardDecoder() => new LegacyStoryboardDecoder(BeatmapVersion);

        public override Beatmap DecodeBeatmap(StreamReader stream) => new LegacyBeatmap(base.DecodeBeatmap(stream));

        protected override void ParseBeatmap(StreamReader stream, Beatmap beatmap)
        {
            throw new NotImplementedException();
        }

        protected override void ParseStoryboard(StreamReader stream, Storyboard storyboard)
        {
            throw new NotImplementedException();
        }

        protected void ParseContent(StreamReader stream)
        {
            Section section = Section.None;

            string line;
            while ((line = stream.ReadLine()) != null)
            {
                if (ShouldSkipLine(line))
                    continue;

                // It's already set in ParseBeatmap... why do it again?
                //if (line.StartsWith(@"osu file format v"))
                //{
                //    Beatmap.BeatmapInfo.BeatmapVersion = int.Parse(line.Substring(17));
                //    continue;
                //}

                if (line.StartsWith(@"[") && line.EndsWith(@"]"))
                {
                    if (!Enum.TryParse(line.Substring(1, line.Length - 2), out section))
                        throw new InvalidDataException($@"Unknown osu section {line}");
                    continue;
                }

                ProcessSection(section, line);
            }
        }

        protected virtual bool ShouldSkipLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//"))
                return true;
            return false;
        }

        protected abstract void ProcessSection(Section section, string line);

        protected KeyValuePair<string, string> SplitKeyVal(string line, char separator)
        {
            var split = line.Trim().Split(new[] { separator }, 2);

            return new KeyValuePair<string, string>
            (
                split[0].Trim(),
                split.Length > 1 ? split[1].Trim() : string.Empty
            );
        }

        protected enum Section
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
        }

        internal enum StoryLayer
        {
            Background = 0,
            Fail = 1,
            Pass = 2,
            Foreground = 3
        }
    }
}
