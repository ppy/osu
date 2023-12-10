// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
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
                if (ShouldSkipLine(line))
                    continue;

                if (section != Section.Metadata)
                {
                    // comments should not be stripped from metadata lines, as the song metadata may contain "//" as valid data.
                    line = StripComments(line);
                }

                line = line.TrimEnd();

                if (line.StartsWith('[') && line.EndsWith(']'))
                {
                    if (!Enum.TryParse(line[1..^1], out section))
                        Logger.Log($"Unknown section \"{line}\" in \"{output}\"");

                    OnBeginNewSection(section);
                    continue;
                }

                try
                {
                    ParseLine(output, section, line);
                }
                catch (Exception e)
                {
                    Logger.Log($"Failed to process line \"{line}\" into \"{output}\": {e.Message}");
                }
            }
        }

        protected virtual bool ShouldSkipLine(string line) => string.IsNullOrWhiteSpace(line) || line.AsSpan().TrimStart().StartsWith("//".AsSpan(), StringComparison.Ordinal);

        /// <summary>
        /// Invoked when a new <see cref="Section"/> has been entered.
        /// </summary>
        /// <param name="section">The entered <see cref="Section"/>.</param>
        protected virtual void OnBeginNewSection(Section section)
        {
        }

        protected virtual void ParseLine(T output, Section section, string line)
        {
            switch (section)
            {
                case Section.Colours:
                    HandleColours(output, line, false);
                    return;
            }
        }

        protected string StripComments(string line)
        {
            int index = line.AsSpan().IndexOf("//".AsSpan());
            if (index > 0)
                return line.Substring(0, index);

            return line;
        }

        protected void HandleColours<TModel>(TModel output, string line, bool allowAlpha)
        {
            var pair = SplitKeyVal(line);

            bool isCombo = pair.Key.StartsWith(@"Combo", StringComparison.Ordinal);
            bool isSnap = pair.Key.StartsWith(@"Snap", StringComparison.Ordinal);

            string[] split = pair.Value.Split(',');

            if (split.Length != 3 && split.Length != 4)
                throw new InvalidOperationException($@"Color specified in incorrect format (should be R,G,B or R,G,B,A): {pair.Value}");

            Color4 colour;

            try
            {
                byte alpha = allowAlpha && split.Length == 4 ? byte.Parse(split[3]) : (byte)255;
                colour = new Color4(byte.Parse(split[0]), byte.Parse(split[1]), byte.Parse(split[2]), alpha);
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
            else if (isSnap)
            {
                if (!(output is IHasSnapColours tHasSnapColours)) return;

                tHasSnapColours.CustomSnapColours.Add(colour);
            }
            else
            {
                if (!(output is IHasCustomColours tHasCustomColours)) return;

                tHasCustomColours.CustomColours[pair.Key] = colour;
            }
        }

        protected KeyValuePair<string, string> SplitKeyVal(string line, char separator = ':', bool shouldTrim = true)
        {
            string[] split = line.Split(separator, 2, shouldTrim ? StringSplitOptions.TrimEntries : StringSplitOptions.None);

            return new KeyValuePair<string, string>
            (
                split[0],
                split.Length > 1 ? split[1] : string.Empty
            );
        }

        protected string CleanFilename(string path) => path
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
