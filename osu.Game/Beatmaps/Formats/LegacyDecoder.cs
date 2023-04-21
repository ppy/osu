// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using osu.Framework.Extensions;
using osu.Framework.Logging;
using osu.Game.Audio;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Extensions;
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

        protected virtual bool ShouldSkipLine(string line) => string.IsNullOrWhiteSpace(line) || line.AsSpan().TrimStart().StartsWith("//".AsSpan(), StringComparison.Ordinal);

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
            int index = line.IndexOf("//");
            if (index > 0)
                return line[..index];

            return line;
        }

        protected void HandleColours<TModel>(TModel output, ReadOnlySpan<char> line, bool allowAlpha)
        {
            var pair = SplitKeyVal(line);

            bool isCombo = pair.Key.StartsWith(@"Combo", StringComparison.Ordinal);

            Color4 colour;

            try
            {
                var split = pair.Value.Split(',');

                split.MoveNext();
                byte r = byte.Parse(split.CurrentSpan);
                split.MoveNext();
                byte g = byte.Parse(split.CurrentSpan);
                split.MoveNext();
                byte b = byte.Parse(split.CurrentSpan);

                byte alpha = allowAlpha && split.MoveNext() ? byte.Parse(split.CurrentSpan) : (byte)255;
                colour = new Color4(r, g, b, alpha);
            }
            catch
            {
                throw new InvalidOperationException($@"Color must be specified in R,G,B or R,G,B,A with 8-bit integer components. Got {pair.Value}.");
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

        protected KeyValuePair<string, string> SplitKeyVal(string line, char separator = ':', bool shouldTrim = true)
        {
            string[] split = line.Split(separator, 2, shouldTrim ? StringSplitOptions.TrimEntries : StringSplitOptions.None);

            return new KeyValuePair<string, string>
            (
                split[0],
                split.Length > 1 ? split[1] : string.Empty
            );
        }

        protected ref struct SpanPair
        {
            // Use tuple when ref struct in generic parameter is a thing
            public ReadOnlySpan<char> Key;
            public ReadOnlySpan<char> Value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected SpanPair SplitKeyVal(ReadOnlySpan<char> line, char separator = ':', bool shouldTrim = true)
        {
            // https://github.com/dotnet/runtime/issues/934
            int index = line.IndexOf(separator);

            if (shouldTrim)
            {
                return index == -1
                    ? new SpanPair
                    {
                        Key = line.Trim(),
                        Value = default
                    }
                    : new SpanPair
                    {
                        Key = line[0..index].Trim(),
                        Value = line[(index + 1)..].Trim()
                    };
            }
            else
            {
                return index == -1
                    ? new SpanPair
                    {
                        Key = line,
                        Value = default
                    }
                    : new SpanPair
                    {
                        Key = line[0..index],
                        Value = line[(index + 1)..]
                    };
            }
        }

        protected string CleanFilename(ReadOnlySpan<char> path) => path
                                                       .Trim('"')
                                                       .ToString()
                                                       // User error which is supported by stable (https://github.com/ppy/osu/issues/21204)
                                                       .Replace(@"\\", @"\")
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

        [Obsolete("Do not use unless you're a legacy ruleset and 100% sure.")]
        public class LegacyDifficultyControlPoint : DifficultyControlPoint, IEquatable<LegacyDifficultyControlPoint>
        {
            /// <summary>
            /// Legacy BPM multiplier that introduces floating-point errors for rulesets that depend on it.
            /// DO NOT USE THIS UNLESS 100% SURE.
            /// </summary>
            public double BpmMultiplier { get; private set; }

            /// <summary>
            /// Whether or not slider ticks should be generated at this control point.
            /// This exists for backwards compatibility with maps that abuse NaN slider velocity behavior on osu!stable (e.g. /b/2628991).
            /// </summary>
            public bool GenerateTicks { get; private set; } = true;

            public LegacyDifficultyControlPoint(int rulesetId, double beatLength)
                : this()
            {
                // Note: In stable, the division occurs on floats, but with compiler optimisations turned on actually seems to occur on doubles via some .NET black magic (possibly inlining?).
                if (rulesetId == 1 || rulesetId == 3)
                    BpmMultiplier = beatLength < 0 ? Math.Clamp((float)-beatLength, 10, 10000) / 100.0 : 1;
                else
                    BpmMultiplier = beatLength < 0 ? Math.Clamp((float)-beatLength, 10, 1000) / 100.0 : 1;

                GenerateTicks = !double.IsNaN(beatLength);
            }

            public LegacyDifficultyControlPoint()
            {
                SliderVelocityBindable.Precision = double.Epsilon;
            }

            public override bool IsRedundant(ControlPoint? existing)
                => base.IsRedundant(existing)
                   && GenerateTicks == ((existing as LegacyDifficultyControlPoint)?.GenerateTicks ?? true);

            public override void CopyFrom(ControlPoint other)
            {
                base.CopyFrom(other);

                BpmMultiplier = ((LegacyDifficultyControlPoint)other).BpmMultiplier;
                GenerateTicks = ((LegacyDifficultyControlPoint)other).GenerateTicks;
            }

            public override bool Equals(ControlPoint? other)
                => other is LegacyDifficultyControlPoint otherLegacyDifficultyControlPoint
                   && Equals(otherLegacyDifficultyControlPoint);

            public bool Equals(LegacyDifficultyControlPoint? other)
                => base.Equals(other)
                   && BpmMultiplier == other.BpmMultiplier
                   && GenerateTicks == other.GenerateTicks;

            // ReSharper disable twice NonReadonlyMemberInGetHashCode
            public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), BpmMultiplier, GenerateTicks);
        }

        internal class LegacySampleControlPoint : SampleControlPoint, IEquatable<LegacySampleControlPoint>
        {
            public int CustomSampleBank;

            public override HitSampleInfo ApplyTo(HitSampleInfo hitSampleInfo)
            {
                var baseInfo = base.ApplyTo(hitSampleInfo);

                if (baseInfo is ConvertHitObjectParser.LegacyHitSampleInfo legacy && legacy.CustomSampleBank == 0)
                    return legacy.With(newCustomSampleBank: CustomSampleBank);

                return baseInfo;
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
