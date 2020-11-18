﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osu.Game.Rulesets.Objects.Types;
using System;
using System.Collections.Generic;
using System.IO;
using osu.Game.Beatmaps.Formats;
using osu.Game.Audio;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Utils;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Objects.Legacy
{
    /// <summary>
    /// A HitObjectParser to parse legacy Beatmaps.
    /// </summary>
    public abstract class ConvertHitObjectParser : HitObjectParser
    {
        /// <summary>
        /// The offset to apply to all time values.
        /// </summary>
        protected readonly double Offset;

        /// <summary>
        /// The beatmap version.
        /// </summary>
        protected readonly int FormatVersion;

        protected bool FirstObject { get; private set; } = true;

        protected ConvertHitObjectParser(double offset, int formatVersion)
        {
            Offset = offset;
            FormatVersion = formatVersion;
        }

        [CanBeNull]
        public override HitObject Parse(string text)
        {
            string[] split = text.Split(',');

            Vector2 pos = new Vector2((int)Parsing.ParseFloat(split[0], Parsing.MAX_COORDINATE_VALUE), (int)Parsing.ParseFloat(split[1], Parsing.MAX_COORDINATE_VALUE));

            double startTime = Parsing.ParseDouble(split[2]) + Offset;

            LegacyHitObjectType type = (LegacyHitObjectType)Parsing.ParseInt(split[3]);

            int comboOffset = (int)(type & LegacyHitObjectType.ComboOffset) >> 4;
            type &= ~LegacyHitObjectType.ComboOffset;

            bool combo = type.HasFlag(LegacyHitObjectType.NewCombo);
            type &= ~LegacyHitObjectType.NewCombo;

            var soundType = (LegacyHitSoundType)Parsing.ParseInt(split[4]);
            var bankInfo = new SampleBankInfo();

            HitObject result = null;

            if (type.HasFlag(LegacyHitObjectType.Circle))
            {
                result = CreateHit(pos, combo, comboOffset);

                if (split.Length > 5)
                    readCustomSampleBanks(split[5], bankInfo);
            }
            else if (type.HasFlag(LegacyHitObjectType.Slider))
            {
                double? length = null;

                int repeatCount = Parsing.ParseInt(split[6]);

                if (repeatCount > 9000)
                    throw new FormatException(@"Repeat count is way too high");

                // osu-stable treated the first span of the slider as a repeat, but no repeats are happening
                repeatCount = Math.Max(0, repeatCount - 1);

                if (split.Length > 7)
                {
                    length = Math.Max(0, Parsing.ParseDouble(split[7], Parsing.MAX_COORDINATE_VALUE));
                    if (length == 0)
                        length = null;
                }

                if (split.Length > 10)
                    readCustomSampleBanks(split[10], bankInfo);

                // One node for each repeat + the start and end nodes
                int nodes = repeatCount + 2;

                // Populate node sample bank infos with the default hit object sample bank
                var nodeBankInfos = new List<SampleBankInfo>();
                for (int i = 0; i < nodes; i++)
                    nodeBankInfos.Add(bankInfo.Clone());

                // Read any per-node sample banks
                if (split.Length > 9 && split[9].Length > 0)
                {
                    string[] sets = split[9].Split('|');

                    for (int i = 0; i < nodes; i++)
                    {
                        if (i >= sets.Length)
                            break;

                        SampleBankInfo info = nodeBankInfos[i];
                        readCustomSampleBanks(sets[i], info);
                    }
                }

                // Populate node sound types with the default hit object sound type
                var nodeSoundTypes = new List<LegacyHitSoundType>();
                for (int i = 0; i < nodes; i++)
                    nodeSoundTypes.Add(soundType);

                // Read any per-node sound types
                if (split.Length > 8 && split[8].Length > 0)
                {
                    string[] adds = split[8].Split('|');

                    for (int i = 0; i < nodes; i++)
                    {
                        if (i >= adds.Length)
                            break;

                        int.TryParse(adds[i], out var sound);
                        nodeSoundTypes[i] = (LegacyHitSoundType)sound;
                    }
                }

                // Generate the final per-node samples
                var nodeSamples = new List<IList<HitSampleInfo>>(nodes);
                for (int i = 0; i < nodes; i++)
                    nodeSamples.Add(convertSoundType(nodeSoundTypes[i], nodeBankInfos[i]));

                result = CreateSlider(pos, combo, comboOffset, convertPathString(split[5], pos), length, repeatCount, nodeSamples);
            }
            else if (type.HasFlag(LegacyHitObjectType.Spinner))
            {
                double duration = Math.Max(0, Parsing.ParseDouble(split[5]) + Offset - startTime);

                result = CreateSpinner(new Vector2(512, 384) / 2, combo, comboOffset, duration);

                if (split.Length > 6)
                    readCustomSampleBanks(split[6], bankInfo);
            }
            else if (type.HasFlag(LegacyHitObjectType.Hold))
            {
                // Note: Hold is generated by BMS converts

                double endTime = Math.Max(startTime, Parsing.ParseDouble(split[2]));

                if (split.Length > 5 && !string.IsNullOrEmpty(split[5]))
                {
                    string[] ss = split[5].Split(':');
                    endTime = Math.Max(startTime, Parsing.ParseDouble(ss[0]));
                    readCustomSampleBanks(string.Join(':', ss.Skip(1)), bankInfo);
                }

                result = CreateHold(pos, combo, comboOffset, endTime + Offset - startTime);
            }

            if (result == null)
                throw new InvalidDataException($"Unknown hit object type: {split[3]}");

            result.StartTime = startTime;

            if (result.Samples.Count == 0)
                result.Samples = convertSoundType(soundType, bankInfo);

            FirstObject = false;

            return result;
        }

        private void readCustomSampleBanks(string str, SampleBankInfo bankInfo)
        {
            if (string.IsNullOrEmpty(str))
                return;

            string[] split = str.Split(':');

            var bank = (LegacySampleBank)Parsing.ParseInt(split[0]);
            var addbank = (LegacySampleBank)Parsing.ParseInt(split[1]);

            string stringBank = bank.ToString().ToLowerInvariant();
            if (stringBank == @"none")
                stringBank = null;
            string stringAddBank = addbank.ToString().ToLowerInvariant();
            if (stringAddBank == @"none")
                stringAddBank = null;

            bankInfo.Normal = stringBank;
            bankInfo.Add = string.IsNullOrEmpty(stringAddBank) ? stringBank : stringAddBank;

            if (split.Length > 2)
                bankInfo.CustomSampleBank = Parsing.ParseInt(split[2]);

            if (split.Length > 3)
                bankInfo.Volume = Math.Max(0, Parsing.ParseInt(split[3]));

            bankInfo.Filename = split.Length > 4 ? split[4] : null;
        }

        private PathType convertPathType(string input)
        {
            switch (input[0])
            {
                default:
                case 'C':
                    return PathType.Catmull;

                case 'B':
                    return PathType.Bezier;

                case 'L':
                    return PathType.Linear;

                case 'P':
                    return PathType.PerfectCurve;
            }
        }

        /// <summary>
        /// Converts a given point string into a set of path control points.
        /// </summary>
        /// <remarks>
        /// A point string takes the form: X|1:1|2:2|2:2|3:3|Y|1:1|2:2.
        /// This has three segments:
        /// <list type="number">
        ///     <item>
        ///         <description>X: { (1,1), (2,2) } (implicit segment)</description>
        ///     </item>
        ///     <item>
        ///         <description>X: { (2,2), (3,3) } (implicit segment)</description>
        ///     </item>
        ///     <item>
        ///         <description>Y: { (3,3), (1,1), (2, 2) } (explicit segment)</description>
        ///     </item>
        /// </list>
        /// </remarks>
        /// <param name="pointString">The point string.</param>
        /// <param name="offset">The positional offset to apply to the control points.</param>
        /// <returns>All control points in the resultant path.</returns>
        private PathControlPoint[] convertPathString(string pointString, Vector2 offset)
        {
            // This code takes on the responsibility of handling explicit segments of the path ("X" & "Y" from above). Implicit segments are handled by calls to convertPoints().
            string[] pointSplit = pointString.Split('|');

            var controlPoints = new List<Memory<PathControlPoint>>();
            int startIndex = 0;
            int endIndex = 0;
            bool first = true;

            while (++endIndex < pointSplit.Length)
            {
                // Keep incrementing endIndex while it's not the start of a new segment (indicated by having a type descriptor of length 1).
                if (pointSplit[endIndex].Length > 1)
                    continue;

                // Multi-segmented sliders DON'T contain the end point as part of the current segment as it's assumed to be the start of the next segment.
                // The start of the next segment is the index after the type descriptor.
                string endPoint = endIndex < pointSplit.Length - 1 ? pointSplit[endIndex + 1] : null;

                controlPoints.AddRange(convertPoints(pointSplit.AsMemory().Slice(startIndex, endIndex - startIndex), endPoint, first, offset));
                startIndex = endIndex;
                first = false;
            }

            if (endIndex > startIndex)
                controlPoints.AddRange(convertPoints(pointSplit.AsMemory().Slice(startIndex, endIndex - startIndex), null, first, offset));

            return mergePointsLists(controlPoints);
        }

        /// <summary>
        /// Converts a given point list into a set of path segments.
        /// </summary>
        /// <param name="points">The point list.</param>
        /// <param name="endPoint">Any extra endpoint to consider as part of the points. This will NOT be returned.</param>
        /// <param name="first">Whether this is the first segment in the set. If <c>true</c> the first of the returned segments will contain a zero point.</param>
        /// <param name="offset">The positional offset to apply to the control points.</param>
        /// <returns>The set of points contained by <paramref name="points"/> as one or more segments of the path, prepended by an extra zero point if <paramref name="first"/> is <c>true</c>.</returns>
        private IEnumerable<Memory<PathControlPoint>> convertPoints(ReadOnlyMemory<string> points, string endPoint, bool first, Vector2 offset)
        {
            PathType type = convertPathType(points.Span[0]);

            int readOffset = first ? 1 : 0; // First control point is zero for the first segment.
            int readablePoints = points.Length - 1; // Total points readable from the base point span.
            int endPointLength = endPoint != null ? 1 : 0; // Extra length if an endpoint is given that lies outside the base point span.

            var vertices = new PathControlPoint[readOffset + readablePoints + endPointLength];

            // Fill any non-read points.
            for (int i = 0; i < readOffset; i++)
                vertices[i] = new PathControlPoint();

            // Parse into control points.
            for (int i = 1; i < points.Length; i++)
                readPoint(points.Span[i], offset, out vertices[readOffset + i - 1]);

            // If an endpoint is given, add it to the end.
            if (endPoint != null)
                readPoint(endPoint, offset, out vertices[^1]);

            // Edge-case rules (to match stable).
            if (type == PathType.PerfectCurve)
            {
                if (vertices.Length != 3)
                    type = PathType.Bezier;
                else if (isLinear(vertices))
                {
                    // osu-stable special-cased colinear perfect curves to a linear path
                    type = PathType.Linear;
                }
            }

            // The first control point must have a definite type.
            vertices[0].Type.Value = type;

            // A path can have multiple implicit segments of the same type if there are two sequential control points with the same position.
            // To handle such cases, this code may return multiple path segments with the final control point in each segment having a non-null type.
            // For the point string X|1:1|2:2|2:2|3:3, this code returns the segments:
            // X: { (1,1), (2, 2) }
            // X: { (3, 3) }
            // Note: (2, 2) is not returned in the second segments, as it is implicit in the path.
            int startIndex = 0;
            int endIndex = 0;

            while (++endIndex < vertices.Length - endPointLength)
            {
                if (vertices[endIndex].Position.Value != vertices[endIndex - 1].Position.Value)
                    continue;

                // Force a type on the last point, and return the current control point set as a segment.
                vertices[endIndex - 1].Type.Value = type;
                yield return vertices.AsMemory().Slice(startIndex, endIndex - startIndex);

                // Skip the current control point - as it's the same as the one that's just been returned.
                startIndex = endIndex + 1;
            }

            if (endIndex > startIndex)
                yield return vertices.AsMemory().Slice(startIndex, endIndex - startIndex);

            static void readPoint(string value, Vector2 startPos, out PathControlPoint point)
            {
                string[] vertexSplit = value.Split(':');

                Vector2 pos = new Vector2((int)Parsing.ParseDouble(vertexSplit[0], Parsing.MAX_COORDINATE_VALUE), (int)Parsing.ParseDouble(vertexSplit[1], Parsing.MAX_COORDINATE_VALUE)) - startPos;
                point = new PathControlPoint { Position = { Value = pos } };
            }

            static bool isLinear(PathControlPoint[] p) => Precision.AlmostEquals(0, (p[1].Position.Value.Y - p[0].Position.Value.Y) * (p[2].Position.Value.X - p[0].Position.Value.X)
                                                                                    - (p[1].Position.Value.X - p[0].Position.Value.X) * (p[2].Position.Value.Y - p[0].Position.Value.Y));
        }

        private PathControlPoint[] mergePointsLists(List<Memory<PathControlPoint>> controlPointList)
        {
            int totalCount = 0;

            foreach (var arr in controlPointList)
                totalCount += arr.Length;

            var mergedArray = new PathControlPoint[totalCount];
            var mergedArrayMemory = mergedArray.AsMemory();
            int copyIndex = 0;

            foreach (var arr in controlPointList)
            {
                arr.CopyTo(mergedArrayMemory.Slice(copyIndex));
                copyIndex += arr.Length;
            }

            return mergedArray;
        }

        /// <summary>
        /// Creates a legacy Hit-type hit object.
        /// </summary>
        /// <param name="position">The position of the hit object.</param>
        /// <param name="newCombo">Whether the hit object creates a new combo.</param>
        /// <param name="comboOffset">When starting a new combo, the offset of the new combo relative to the current one.</param>
        /// <returns>The hit object.</returns>
        protected abstract HitObject CreateHit(Vector2 position, bool newCombo, int comboOffset);

        /// <summary>
        /// Creats a legacy Slider-type hit object.
        /// </summary>
        /// <param name="position">The position of the hit object.</param>
        /// <param name="newCombo">Whether the hit object creates a new combo.</param>
        /// <param name="comboOffset">When starting a new combo, the offset of the new combo relative to the current one.</param>
        /// <param name="controlPoints">The slider control points.</param>
        /// <param name="length">The slider length.</param>
        /// <param name="repeatCount">The slider repeat count.</param>
        /// <param name="nodeSamples">The samples to be played when the slider nodes are hit. This includes the head and tail of the slider.</param>
        /// <returns>The hit object.</returns>
        protected abstract HitObject CreateSlider(Vector2 position, bool newCombo, int comboOffset, PathControlPoint[] controlPoints, double? length, int repeatCount,
                                                  List<IList<HitSampleInfo>> nodeSamples);

        /// <summary>
        /// Creates a legacy Spinner-type hit object.
        /// </summary>
        /// <param name="position">The position of the hit object.</param>
        /// <param name="newCombo">Whether the hit object creates a new combo.</param>
        /// <param name="comboOffset">When starting a new combo, the offset of the new combo relative to the current one.</param>
        /// <param name="duration">The spinner duration.</param>
        /// <returns>The hit object.</returns>
        protected abstract HitObject CreateSpinner(Vector2 position, bool newCombo, int comboOffset, double duration);

        /// <summary>
        /// Creates a legacy Hold-type hit object.
        /// </summary>
        /// <param name="position">The position of the hit object.</param>
        /// <param name="newCombo">Whether the hit object creates a new combo.</param>
        /// <param name="comboOffset">When starting a new combo, the offset of the new combo relative to the current one.</param>
        /// <param name="duration">The hold duration.</param>
        protected abstract HitObject CreateHold(Vector2 position, bool newCombo, int comboOffset, double duration);

        private List<HitSampleInfo> convertSoundType(LegacyHitSoundType type, SampleBankInfo bankInfo)
        {
            // Todo: This should return the normal SampleInfos if the specified sample file isn't found, but that's a pretty edge-case scenario
            if (!string.IsNullOrEmpty(bankInfo.Filename))
            {
                return new List<HitSampleInfo>
                {
                    new FileHitSampleInfo
                    {
                        Filename = bankInfo.Filename,
                        Volume = bankInfo.Volume
                    }
                };
            }

            var soundTypes = new List<HitSampleInfo>
            {
                new LegacyHitSampleInfo
                {
                    Bank = bankInfo.Normal,
                    Name = HitSampleInfo.HIT_NORMAL,
                    Volume = bankInfo.Volume,
                    CustomSampleBank = bankInfo.CustomSampleBank,
                    // if the sound type doesn't have the Normal flag set, attach it anyway as a layered sample.
                    // None also counts as a normal non-layered sample: https://osu.ppy.sh/help/wiki/osu!_File_Formats/Osu_(file_format)#hitsounds
                    IsLayered = type != LegacyHitSoundType.None && !type.HasFlag(LegacyHitSoundType.Normal)
                }
            };

            if (type.HasFlag(LegacyHitSoundType.Finish))
            {
                soundTypes.Add(new LegacyHitSampleInfo
                {
                    Bank = bankInfo.Add,
                    Name = HitSampleInfo.HIT_FINISH,
                    Volume = bankInfo.Volume,
                    CustomSampleBank = bankInfo.CustomSampleBank
                });
            }

            if (type.HasFlag(LegacyHitSoundType.Whistle))
            {
                soundTypes.Add(new LegacyHitSampleInfo
                {
                    Bank = bankInfo.Add,
                    Name = HitSampleInfo.HIT_WHISTLE,
                    Volume = bankInfo.Volume,
                    CustomSampleBank = bankInfo.CustomSampleBank
                });
            }

            if (type.HasFlag(LegacyHitSoundType.Clap))
            {
                soundTypes.Add(new LegacyHitSampleInfo
                {
                    Bank = bankInfo.Add,
                    Name = HitSampleInfo.HIT_CLAP,
                    Volume = bankInfo.Volume,
                    CustomSampleBank = bankInfo.CustomSampleBank
                });
            }

            return soundTypes;
        }

        private class SampleBankInfo
        {
            public string Filename;

            public string Normal;
            public string Add;
            public int Volume;

            public int CustomSampleBank;

            public SampleBankInfo Clone() => (SampleBankInfo)MemberwiseClone();
        }

        public class LegacyHitSampleInfo : HitSampleInfo
        {
            private int customSampleBank;

            public int CustomSampleBank
            {
                get => customSampleBank;
                set
                {
                    customSampleBank = value;

                    if (value >= 2)
                        Suffix = value.ToString();
                }
            }

            /// <summary>
            /// Whether this hit sample is layered.
            /// </summary>
            /// <remarks>
            /// Layered hit samples are automatically added in all modes (except osu!mania), but can be disabled
            /// using the <see cref="LegacySkinConfiguration.LegacySetting.LayeredHitSounds"/> skin config option.
            /// </remarks>
            public bool IsLayered { get; set; }
        }

        private class FileHitSampleInfo : LegacyHitSampleInfo
        {
            public string Filename;

            public FileHitSampleInfo()
            {
                // Make sure that the LegacyBeatmapSkin does not fall back to the user skin.
                // Note that this does not change the lookup names, as they are overridden locally.
                CustomSampleBank = 1;
            }

            public override IEnumerable<string> LookupNames => new[]
            {
                Filename,
                Path.ChangeExtension(Filename, null)
            };
        }
    }
}
