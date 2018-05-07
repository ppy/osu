// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Game.Rulesets.Objects.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using osu.Game.Beatmaps.Formats;
using osu.Game.Audio;
using System.Linq;
using osu.Framework.MathUtils;

namespace osu.Game.Rulesets.Objects.Legacy
{
    /// <summary>
    /// A HitObjectParser to parse legacy Beatmaps.
    /// </summary>
    public abstract class ConvertHitObjectParser : HitObjectParser
    {
        public override HitObject Parse(string text)
        {
            return Parse(text, 0);
        }

        public HitObject Parse(string text, double offset)
        {
            try
            {
                string[] split = text.Split(',');

                ConvertHitObjectType type = (ConvertHitObjectType)int.Parse(split[3]) & ~ConvertHitObjectType.ColourHax;
                bool combo = type.HasFlag(ConvertHitObjectType.NewCombo);
                type &= ~ConvertHitObjectType.NewCombo;

                var soundType = (LegacySoundType)int.Parse(split[4]);
                var bankInfo = new SampleBankInfo();

                HitObject result = null;

                if ((type & ConvertHitObjectType.Circle) > 0)
                {
                    result = CreateHit(new Vector2(int.Parse(split[0]), int.Parse(split[1])), combo);

                    if (split.Length > 5)
                        readCustomSampleBanks(split[5], bankInfo);
                }
                else if ((type & ConvertHitObjectType.Slider) > 0)
                {
                    var pos = new Vector2(int.Parse(split[0]), int.Parse(split[1]));

                    CurveType curveType = CurveType.Catmull;
                    double length = 0;
                    var points = new List<Vector2> { Vector2.Zero };

                    string[] pointsplit = split[5].Split('|');
                    foreach (string t in pointsplit)
                    {
                        if (t.Length == 1)
                        {
                            switch (t)
                            {
                                case @"C":
                                    curveType = CurveType.Catmull;
                                    break;
                                case @"B":
                                    curveType = CurveType.Bezier;
                                    break;
                                case @"L":
                                    curveType = CurveType.Linear;
                                    break;
                                case @"P":
                                    curveType = CurveType.PerfectCurve;
                                    break;
                            }
                            continue;
                        }

                        string[] temp = t.Split(':');
                        points.Add(new Vector2((int)Convert.ToDouble(temp[0], CultureInfo.InvariantCulture), (int)Convert.ToDouble(temp[1], CultureInfo.InvariantCulture)) - pos);
                    }

                    // osu-stable special-cased colinear perfect curves to a CurveType.Linear
                    bool isLinear(List<Vector2> p) => Precision.AlmostEquals(0, (p[1].Y - p[0].Y) * (p[2].X - p[0].X) - (p[1].X - p[0].X) * (p[2].Y - p[0].Y));
                    if (points.Count == 3 && curveType == CurveType.PerfectCurve && isLinear(points))
                        curveType = CurveType.Linear;

                    int repeatCount = Convert.ToInt32(split[6], CultureInfo.InvariantCulture);

                    if (repeatCount > 9000)
                        throw new ArgumentOutOfRangeException(nameof(repeatCount), @"Repeat count is way too high");

                    // osu-stable treated the first span of the slider as a repeat, but no repeats are happening
                    repeatCount = Math.Max(0, repeatCount - 1);


                    if (split.Length > 7)
                        length = Convert.ToDouble(split[7], CultureInfo.InvariantCulture);

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
                    var nodeSamples = new List<List<SampleInfo>>(nodes);
                    for (int i = 0; i < nodes; i++)
                        nodeSamples.Add(convertSoundType(nodeSoundTypes[i], nodeBankInfos[i]));

                    result = CreateSlider(pos, combo, points, length, curveType, repeatCount, nodeSamples);
                }
                else if ((type & ConvertHitObjectType.Spinner) > 0)
                {
                    result = CreateSpinner(new Vector2(512, 384) / 2, Convert.ToDouble(split[5], CultureInfo.InvariantCulture) + offset);

                    if (split.Length > 6)
                        readCustomSampleBanks(split[6], bankInfo);
                }
                else if ((type & ConvertHitObjectType.Hold) > 0)
                {
                    // Note: Hold is generated by BMS converts

                    double endTime = Convert.ToDouble(split[2], CultureInfo.InvariantCulture);

                    if (split.Length > 5 && !string.IsNullOrEmpty(split[5]))
                    {
                        string[] ss = split[5].Split(':');
                        endTime = Convert.ToDouble(ss[0], CultureInfo.InvariantCulture);
                        readCustomSampleBanks(string.Join(":", ss.Skip(1)), bankInfo);
                    }

                    result = CreateHold(new Vector2(int.Parse(split[0]), int.Parse(split[1])), combo, endTime + offset);
                }

                if (result == null)
                    throw new InvalidOperationException($@"Unknown hit object type {type}.");

                result.StartTime = Convert.ToDouble(split[2], CultureInfo.InvariantCulture) + offset;
                result.Samples = convertSoundType(soundType, bankInfo);

                return result;
            }
            catch (FormatException)
            {
                throw new FormatException("One or more hit objects were malformed.");
            }
        }

        private void readCustomSampleBanks(string str, SampleBankInfo bankInfo)
        {
            if (string.IsNullOrEmpty(str))
                return;

            string[] split = str.Split(':');

            var bank = (LegacyBeatmapDecoder.LegacySampleBank)Convert.ToInt32(split[0]);
            var addbank = (LegacyBeatmapDecoder.LegacySampleBank)Convert.ToInt32(split[1]);

            // Let's not implement this for now, because this doesn't fit nicely into the bank structure
            //string sampleFile = split2.Length > 4 ? split2[4] : string.Empty;

            string stringBank = bank.ToString().ToLower();
            if (stringBank == @"none")
                stringBank = null;
            string stringAddBank = addbank.ToString().ToLower();
            if (stringAddBank == @"none")
                stringAddBank = null;

            bankInfo.Normal = stringBank;
            bankInfo.Add = stringAddBank;

            if (split.Length > 3)
                bankInfo.Volume = int.Parse(split[3]);
        }

        /// <summary>
        /// Creates a legacy Hit-type hit object.
        /// </summary>
        /// <param name="position">The position of the hit object.</param>
        /// <param name="newCombo">Whether the hit object creates a new combo.</param>
        /// <returns>The hit object.</returns>
        protected abstract HitObject CreateHit(Vector2 position, bool newCombo);

        /// <summary>
        /// Creats a legacy Slider-type hit object.
        /// </summary>
        /// <param name="position">The position of the hit object.</param>
        /// <param name="newCombo">Whether the hit object creates a new combo.</param>
        /// <param name="controlPoints">The slider control points.</param>
        /// <param name="length">The slider length.</param>
        /// <param name="curveType">The slider curve type.</param>
        /// <param name="repeatCount">The slider repeat count.</param>
        /// <param name="repeatSamples">The samples to be played when the repeat nodes are hit. This includes the head and tail of the slider.</param>
        /// <returns>The hit object.</returns>
        protected abstract HitObject CreateSlider(Vector2 position, bool newCombo, List<Vector2> controlPoints, double length, CurveType curveType, int repeatCount, List<List<SampleInfo>> repeatSamples);

        /// <summary>
        /// Creates a legacy Spinner-type hit object.
        /// </summary>
        /// <param name="position">The position of the hit object.</param>
        /// <param name="endTime">The spinner end time.</param>
        /// <returns>The hit object.</returns>
        protected abstract HitObject CreateSpinner(Vector2 position, double endTime);

        /// <summary>
        /// Creates a legacy Hold-type hit object.
        /// </summary>
        /// <param name="position">The position of the hit object.</param>
        /// <param name="newCombo">Whether the hit object creates a new combo.</param>
        /// <param name="endTime">The hold end time.</param>
        protected abstract HitObject CreateHold(Vector2 position, bool newCombo, double endTime);

        private List<SampleInfo> convertSoundType(LegacySoundType type, SampleBankInfo bankInfo)
        {
            var soundTypes = new List<SampleInfo>
            {
                new SampleInfo
                {
                    Bank = bankInfo.Normal,
                    Name = SampleInfo.HIT_NORMAL,
                    Volume = bankInfo.Volume
                }
            };

            if ((type & LegacySoundType.Finish) > 0)
            {
                soundTypes.Add(new SampleInfo
                {
                    Bank = bankInfo.Add,
                    Name = SampleInfo.HIT_FINISH,
                    Volume = bankInfo.Volume
                });
            }

            if ((type & LegacySoundType.Whistle) > 0)
            {
                soundTypes.Add(new SampleInfo
                {
                    Bank = bankInfo.Add,
                    Name = SampleInfo.HIT_WHISTLE,
                    Volume = bankInfo.Volume
                });
            }

            if ((type & LegacySoundType.Clap) > 0)
            {
                soundTypes.Add(new SampleInfo
                {
                    Bank = bankInfo.Add,
                    Name = SampleInfo.HIT_CLAP,
                    Volume = bankInfo.Volume
                });
            }

            return soundTypes;
        }

        private class SampleBankInfo
        {
            public string Normal;
            public string Add;
            public int Volume;

            public SampleBankInfo Clone()
            {
                return (SampleBankInfo)MemberwiseClone();
            }
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
