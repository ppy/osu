// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Extensions;
using osu.Framework.Logging;
using osu.Game.Audio;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.IO;
using osu.Game.Rulesets.Objects.Legacy;
using osuTK.Graphics;

namespace osu.Game.Beatmaps.Formats
{
    public abstract class LegacyDecoder<T> : Decoder<T>
        where T : new()
    {
        public const int LATEST_VERSION = 14;

        protected readonly int FormatVersion;

        protected LegacyDecoder(int version)
        {
            FormatVersion = version;
        }

        protected override void ParseStreamInto(LineBufferedReader stream, T output)
        {
            Section section = Section.General;

            string? line;

            while ((line = stream.ReadLine()) != null)
            {
                ReadOnlySpan<char> lineSpan = line;

                if (ShouldSkipLine(line))
                    continue;

                if (section != Section.Metadata)
                {
                    // comments should not be stripped from metadata lines, as the song metadata may contain "//" as valid data.
                    lineSpan = StripComments(lineSpan);
                }

                lineSpan = lineSpan.TrimEnd();

                if (lineSpan[0] == '[' && lineSpan[^1] == ']')
                {
                    if (!Enum.TryParse(lineSpan[1..^1], out section))
                        Logger.Log($"Unknown section \"{lineSpan}\" in \"{output}\"");

                    OnBeginNewSection(section);
                    continue;
                }

                try
                {
                    ParseLine(output, section, lineSpan);
                }
                catch (Exception e)
                {
                    Logger.Log($"Failed to process line \"{lineSpan}\" into \"{output}\": {e.Message}");
                }
            }
        }

        protected virtual bool ShouldSkipLine(ReadOnlySpan<char> line) => line.IsWhiteSpace() || line.TrimStart().StartsWith("//".AsSpan(), StringComparison.Ordinal);

        /// <summary>
        /// Invoked when a new <see cref="Section"/> has been entered.
        /// </summary>
        /// <param name="section">The entered <see cref="Section"/>.</param>
        protected virtual void OnBeginNewSection(Section section)
        {
        }

        protected virtual void ParseLine(T output, Section section, ReadOnlySpan<char> line)
        {
            switch (section)
            {
                case Section.Colours:
                    HandleColours(output, line, false);
                    return;
            }
        }

        protected ReadOnlySpan<char> StripComments(ReadOnlySpan<char> line)
        {
            int index = line.IndexOf("//".AsSpan());
            if (index > 0)
                return line.Slice(0, index);

            return line;
        }

        protected void HandleColours<TModel>(TModel output, ReadOnlySpan<char> line, bool allowAlpha)
        {
            var pair = SplitKeyVal(line);

            bool isCombo = pair.Key.StartsWith(@"Combo", StringComparison.Ordinal);
            var colourValue = pair.Value;

            Span<Range> ranges = stackalloc Range[5];
            int splitCount = colourValue.Split(ranges, ',');

            if (splitCount != 3 && splitCount != 4)
                throw new InvalidOperationException($@"Color specified in incorrect format (should be R,G,B or R,G,B,A): {colourValue}");

            Color4 colour;

            try
            {
                byte alpha = allowAlpha && splitCount == 4 ? byte.Parse(colourValue[ranges[3]]) : (byte)255;
                colour = new Color4(byte.Parse(colourValue[ranges[0]]), byte.Parse(colourValue[ranges[1]]), byte.Parse(colourValue[ranges[2]]), alpha);
            }
            catch
            {
                throw new InvalidOperationException(@"Color must be specified with 8-bit integer components");
            }

            if (isCombo)
            {
                if (!(output is IHasComboColours tHasComboColours)) return;

                tHasComboColours.CustomComboColours.Add(colour);
            }
            else
            {
                if (!(output is IHasCustomColours tHasCustomColours)) return;

                tHasCustomColours.CustomColours[pair.Key.ToString()] = colour;
            }
        }

        protected ref struct SpanKeyValuePair
        {
            public ReadOnlySpan<char> Key;
            public ReadOnlySpan<char> Value;
        }

        protected SpanKeyValuePair SplitKeyVal(ReadOnlySpan<char> line, char separator = ':', bool shouldTrim = true)
        {
            Span<Range> ranges = stackalloc Range[2];

            int splitCount = line.Split(ranges, separator, shouldTrim ? StringSplitOptions.TrimEntries : StringSplitOptions.None);

            return new SpanKeyValuePair
            {
                Key = line[ranges[0]],
                Value = splitCount > 1 ? line[ranges[1]] : default
            };
        }

        protected string CleanFilename(ReadOnlySpan<char> path) =>
            path
            .ToString()
            // User error which is supported by stable (https://github.com/ppy/osu/issues/21204)
            .Replace(@"\\", @"\")
            .Trim('"')
            .ToStandardisedPath();

        public enum Section
        {
            General,
            Editor,
            Metadata,
            Difficulty,
            Events,
            TimingPoints,
            Colours,
            HitObjects,
            Variables,
            Fonts,
            CatchTheBeat,
            Mania,
        }

        internal class LegacySampleControlPoint : SampleControlPoint, IEquatable<LegacySampleControlPoint>
        {
            public int CustomSampleBank;

            public override HitSampleInfo ApplyTo(HitSampleInfo hitSampleInfo)
            {
                if (hitSampleInfo is ConvertHitObjectParser.LegacyHitSampleInfo legacy)
                {
                    return legacy.With(
                        newCustomSampleBank: legacy.CustomSampleBank > 0 ? legacy.CustomSampleBank : CustomSampleBank,
                        newVolume: hitSampleInfo.Volume > 0 ? hitSampleInfo.Volume : SampleVolume,
                        newBank: legacy.BankSpecified ? legacy.Bank : SampleBank
                    );
                }

                return base.ApplyTo(hitSampleInfo);
            }

            public override bool IsRedundant(ControlPoint? existing)
                => base.IsRedundant(existing)
                   && existing is LegacySampleControlPoint existingSample
                   && CustomSampleBank == existingSample.CustomSampleBank;

            public override void CopyFrom(ControlPoint other)
            {
                base.CopyFrom(other);

                CustomSampleBank = ((LegacySampleControlPoint)other).CustomSampleBank;
            }

            public override bool Equals(ControlPoint? other)
                => other is LegacySampleControlPoint otherLegacySampleControlPoint
                   && Equals(otherLegacySampleControlPoint);

            public bool Equals(LegacySampleControlPoint? other)
                => base.Equals(other)
                   && CustomSampleBank == other.CustomSampleBank;

            // ReSharper disable once NonReadonlyMemberInGetHashCode
            public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), CustomSampleBank);
        }
    }
}
