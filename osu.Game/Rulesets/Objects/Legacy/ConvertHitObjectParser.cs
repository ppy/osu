// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
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
using osu.Framework.MathUtils;

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

            ConvertHitObjectType type = (ConvertHitObjectType)Parsing.ParseInt(split[3]);

            int comboOffset = (int)(type & ConvertHitObjectType.ComboOffset) >> 4;
            type &= ~ConvertHitObjectType.ComboOffset;

            bool combo = type.HasFlag(ConvertHitObjectType.NewCombo);
            type &= ~ConvertHitObjectType.NewCombo;

            var soundType = (LegacySoundType)Parsing.ParseInt(split[4]);
            var bankInfo = new SampleBankInfo();

            HitObject result = null;

            if (type.HasFlag(ConvertHitObjectType.Circle))
            {
                result = CreateHit(pos, combo, comboOffset);

                if (split.Length > 5)
                    readCustomSampleBanks(split[5], bankInfo);
            }
            else if (type.HasFlag(ConvertHitObjectType.Slider))
            {
                PathType pathType = PathType.Catmull;
                double? length = null;

                string[] pointSplit = split[5].Split('|');

                int pointCount = 1;
                foreach (var t in pointSplit)
                    if (t.Length > 1)
                        pointCount++;

                var points = new Vector2[pointCount];

                int pointIndex = 1;

                foreach (string t in pointSplit)
                {
                    if (t.Length == 1)
                    {
                        switch (t)
                        {
                            case @"C":
                                pathType = PathType.Catmull;
                                break;

                            case @"B":
                                pathType = PathType.Bezier;
                                break;

                            case @"L":
                                pathType = PathType.Linear;
                                break;

                            case @"P":
                                pathType = PathType.PerfectCurve;
                                break;
                        }

                        continue;
                    }

                    string[] temp = t.Split(':');
                    points[pointIndex++] = new Vector2((int)Parsing.ParseDouble(temp[0], Parsing.MAX_COORDINATE_VALUE), (int)Parsing.ParseDouble(temp[1], Parsing.MAX_COORDINATE_VALUE)) - pos;
                }

                // osu-stable special-cased colinear perfect curves to a CurveType.Linear
                bool isLinear(Vector2[] p) => Precision.AlmostEquals(0, (p[1].Y - p[0].Y) * (p[2].X - p[0].X) - (p[1].X - p[0].X) * (p[2].Y - p[0].Y));

                if (points.Length == 3 && pathType == PathType.PerfectCurve && isLinear(points))
                    pathType = PathType.Linear;

                int repeatCount = Parsing.ParseInt(split[6]);

                if (repeatCount > 9000)
                    throw new ArgumentOutOfRangeException(nameof(repeatCount), @"Repeat count is way too high");

                // osu-stable treated the first span of the slider as a repeat, but no repeats are happening
                repeatCount = Math.Max(0, repeatCount - 1);

                if (split.Length > 7)
                {
                    length = Math.Max(0, Parsing.ParseDouble(split[7]));
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
                var nodeSoundTypes = new List<LegacySoundType>();
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

                        int sound;
                        int.TryParse(adds[i], out sound);
                        nodeSoundTypes[i] = (LegacySoundType)sound;
                    }
                }

                // Generate the final per-node samples
                var nodeSamples = new List<List<HitSampleInfo>>(nodes);
                for (int i = 0; i < nodes; i++)
                    nodeSamples.Add(convertSoundType(nodeSoundTypes[i], nodeBankInfos[i]));

                result = CreateSlider(pos, combo, comboOffset, points, length, pathType, repeatCount, nodeSamples);

                // The samples are played when the slider ends, which is the last node
                result.Samples = nodeSamples[nodeSamples.Count - 1];
            }
            else if (type.HasFlag(ConvertHitObjectType.Spinner))
            {
                double endTime = Math.Max(startTime, Parsing.ParseDouble(split[5]) + Offset);

                result = CreateSpinner(new Vector2(512, 384) / 2, combo, comboOffset, endTime);

                if (split.Length > 6)
                    readCustomSampleBanks(split[6], bankInfo);
            }
            else if (type.HasFlag(ConvertHitObjectType.Hold))
            {
                // Note: Hold is generated by BMS converts

                double endTime = Math.Max(startTime, Parsing.ParseDouble(split[2]));

                if (split.Length > 5 && !string.IsNullOrEmpty(split[5]))
                {
                    string[] ss = split[5].Split(':');
                    endTime = Math.Max(startTime, Parsing.ParseDouble(ss[0]));
                    readCustomSampleBanks(string.Join(":", ss.Skip(1)), bankInfo);
                }

                result = CreateHold(pos, combo, comboOffset, endTime + Offset);
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

            var bank = (LegacyBeatmapDecoder.LegacySampleBank)Parsing.ParseInt(split[0]);
            var addbank = (LegacyBeatmapDecoder.LegacySampleBank)Parsing.ParseInt(split[1]);

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
        /// <param name="pathType">The slider curve type.</param>
        /// <param name="repeatCount">The slider repeat count.</param>
        /// <param name="nodeSamples">The samples to be played when the slider nodes are hit. This includes the head and tail of the slider.</param>
        /// <returns>The hit object.</returns>
        protected abstract HitObject CreateSlider(Vector2 position, bool newCombo, int comboOffset, Vector2[] controlPoints, double? length, PathType pathType, int repeatCount,
                                                  List<List<HitSampleInfo>> nodeSamples);

        /// <summary>
        /// Creates a legacy Spinner-type hit object.
        /// </summary>
        /// <param name="position">The position of the hit object.</param>
        /// <param name="newCombo">Whether the hit object creates a new combo.</param>
        /// <param name="comboOffset">When starting a new combo, the offset of the new combo relative to the current one.</param>
        /// <param name="endTime">The spinner end time.</param>
        /// <returns>The hit object.</returns>
        protected abstract HitObject CreateSpinner(Vector2 position, bool newCombo, int comboOffset, double endTime);

        /// <summary>
        /// Creates a legacy Hold-type hit object.
        /// </summary>
        /// <param name="position">The position of the hit object.</param>
        /// <param name="newCombo">Whether the hit object creates a new combo.</param>
        /// <param name="comboOffset">When starting a new combo, the offset of the new combo relative to the current one.</param>
        /// <param name="endTime">The hold end time.</param>
        protected abstract HitObject CreateHold(Vector2 position, bool newCombo, int comboOffset, double endTime);

        private List<HitSampleInfo> convertSoundType(LegacySoundType type, SampleBankInfo bankInfo)
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
                    CustomSampleBank = bankInfo.CustomSampleBank
                }
            };

            if (type.HasFlag(LegacySoundType.Finish))
            {
                soundTypes.Add(new LegacyHitSampleInfo
                {
                    Bank = bankInfo.Add,
                    Name = HitSampleInfo.HIT_FINISH,
                    Volume = bankInfo.Volume,
                    CustomSampleBank = bankInfo.CustomSampleBank
                });
            }

            if (type.HasFlag(LegacySoundType.Whistle))
            {
                soundTypes.Add(new LegacyHitSampleInfo
                {
                    Bank = bankInfo.Add,
                    Name = HitSampleInfo.HIT_WHISTLE,
                    Volume = bankInfo.Volume,
                    CustomSampleBank = bankInfo.CustomSampleBank
                });
            }

            if (type.HasFlag(LegacySoundType.Clap))
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

        private class LegacyHitSampleInfo : HitSampleInfo
        {
            public int CustomSampleBank
            {
                set
                {
                    if (value > 1)
                        Suffix = value.ToString();
                }
            }
        }

        private class FileHitSampleInfo : HitSampleInfo
        {
            public string Filename;

            public override IEnumerable<string> LookupNames => new[]
            {
                Filename,
                Path.ChangeExtension(Filename, null)
            };
        }

        [Flags]
        private enum LegacySoundType
        {
            None = 0,
            Normal = 1,
            Whistle = 2,
            Finish = 4,
            Clap = 8
        }
    }
}
