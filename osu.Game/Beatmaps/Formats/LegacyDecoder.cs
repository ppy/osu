// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.IO;
using osu.Framework.Logging;
using osu.Game.Audio;
using osu.Game.Beatmaps.ControlPoints;
using OpenTK.Graphics;

namespace osu.Game.Beatmaps.Formats
{
    public abstract class LegacyDecoder<T> : Decoder<T>
        where T : new()
    {
        protected readonly int FormatVersion;

        protected LegacyDecoder(int version)
        {
            FormatVersion = version;
        }

        protected override void ParseStreamInto(StreamReader stream, T output)
        {
            Section section = Section.None;

            string line;
            while ((line = stream.ReadLine()) != null)
            {
                if (ShouldSkipLine(line))
                    continue;

                if (line.StartsWith(@"[") && line.EndsWith(@"]"))
                {
                    if (!Enum.TryParse(line.Substring(1, line.Length - 2), out section))
                    {
                        Logger.Log($"Unknown section \"{line}\" in {output}");
                        section = Section.None;
                    }

                    continue;
                }

                try
                {
                    ParseLine(output, section, line);
                }
                catch (Exception e)
                {
                    Logger.Error(e, $"Failed to process line \"{line}\" into {output}");
                }
            }
        }

        protected virtual bool ShouldSkipLine(string line) => string.IsNullOrWhiteSpace(line) || line.StartsWith("//");

        protected virtual void ParseLine(T output, Section section, string line)
        {
            line = StripComments(line);

            switch (section)
            {
                case Section.Colours:
                    handleColours(output, line);
                    return;
            }
        }

        protected string StripComments(string line)
        {
            var index = line.IndexOf("//", StringComparison.Ordinal);
            if (index > 0)
                return line.Substring(0, index);
            return line;
        }

        private bool hasComboColours;

        private void handleColours(T output, string line)
        {
            var pair = SplitKeyVal(line);

            bool isCombo = pair.Key.StartsWith(@"Combo");

            string[] split = pair.Value.Split(',');

            if (split.Length != 3)
                throw new InvalidOperationException($@"Color specified in incorrect format (should be R,G,B): {pair.Value}");

            if (!byte.TryParse(split[0], out var r) || !byte.TryParse(split[1], out var g) || !byte.TryParse(split[2], out var b))
                throw new InvalidOperationException(@"Color must be specified with 8-bit integer components");

            Color4 colour = new Color4(r, g, b, 255);

            if (isCombo)
            {
                if (!(output is IHasComboColours tHasComboColours)) return;

                if (!hasComboColours)
                {
                    // remove default colours.
                    tHasComboColours.ComboColours.Clear();
                    hasComboColours = true;
                }

                tHasComboColours.ComboColours.Add(colour);
            }
            else
            {
                if (!(output is IHasCustomColours tHasCustomColours)) return;
                tHasCustomColours.CustomColours[pair.Key] = colour;
            }
        }

        protected KeyValuePair<string, string> SplitKeyVal(string line, char separator = ':')
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
            Fonts
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

        internal class LegacySampleControlPoint : SampleControlPoint
        {
            public int CustomSampleBank;

            public override SampleInfo ApplyTo(SampleInfo sampleInfo)
            {
                var baseInfo = base.ApplyTo(sampleInfo);

                if (string.IsNullOrEmpty(baseInfo.Suffix) && CustomSampleBank > 1)
                    baseInfo.Suffix = CustomSampleBank.ToString();

                return baseInfo;
            }

            public override bool EquivalentTo(ControlPoint other)
                => base.EquivalentTo(other)
                   && other is LegacySampleControlPoint legacy
                   && CustomSampleBank == legacy.CustomSampleBank;
        }
    }
}
