// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Storyboards;

namespace osu.Game.Beatmaps.Formats
{
    public abstract class LegacyDecoder : BeatmapDecoder
    {
        public static void Register()
        {
            AddDecoder<LegacyBeatmapDecoder>(@"osu file format v14");
            AddDecoder<LegacyBeatmapDecoder>(@"osu file format v13");
            AddDecoder<LegacyBeatmapDecoder>(@"osu file format v12");
            AddDecoder<LegacyBeatmapDecoder>(@"osu file format v11");
            AddDecoder<LegacyBeatmapDecoder>(@"osu file format v10");
            AddDecoder<LegacyBeatmapDecoder>(@"osu file format v9");
            AddDecoder<LegacyBeatmapDecoder>(@"osu file format v8");
            AddDecoder<LegacyBeatmapDecoder>(@"osu file format v7");
            AddDecoder<LegacyBeatmapDecoder>(@"osu file format v6");
            AddDecoder<LegacyBeatmapDecoder>(@"osu file format v5");
            AddDecoder<LegacyBeatmapDecoder>(@"osu file format v4");
            AddDecoder<LegacyBeatmapDecoder>(@"osu file format v3");
            // TODO: differences between versions
        }

        protected Beatmap beatmap;
        protected Storyboard storyboard;

        protected int beatmapVersion;
        protected readonly Dictionary<string, string> variables = new Dictionary<string, string>();

        public override Beatmap DecodeBeatmap(StreamReader stream) => new LegacyBeatmap(base.DecodeBeatmap(stream));

        protected override Beatmap ParseBeatmap(StreamReader stream) => new LegacyBeatmap(base.ParseBeatmap(stream));

        protected override void ParseBeatmap(StreamReader stream, Beatmap beatmap)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            if (beatmap == null)
                throw new ArgumentNullException(nameof(beatmap));

            this.beatmap = beatmap;
            this.beatmap.BeatmapInfo.BeatmapVersion = beatmapVersion;

            ParseContent(stream);

            foreach (var hitObject in this.beatmap.HitObjects)
                hitObject.ApplyDefaults(this.beatmap.ControlPointInfo, this.beatmap.BeatmapInfo.BaseDifficulty);
        }

        protected override void ParseStoryboard(StreamReader stream, Storyboard storyboard)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            if (storyboard == null)
                throw new ArgumentNullException(nameof(storyboard));

            this.storyboard = storyboard;

            ParseContent(stream);
        }

        protected void ParseContent(StreamReader stream)
        {
            Section section = Section.None;

            string line;
            while ((line = stream.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
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

                processSection(section, line);
            }
        }

        protected abstract void processSection(Section section, string line);

        /// <summary>
        /// Decodes any beatmap variables present in a line into their real values.
        /// </summary>
        /// <param name="line">The line which may contains variables.</param>
        protected void decodeVariables(ref string line)
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
                if (line == origLine)
                    break;
            }
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
