// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Game.Rulesets.Objects.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using osu.Game.Beatmaps.Formats;
using osu.Game.Audio;
using System.Linq;

namespace osu.Game.Rulesets.Objects.Legacy
{
    /// <summary>
    /// A HitObjectParser to parse legacy Beatmaps.
    /// </summary>
    internal abstract class HitObjectParser : Objects.HitObjectParser
    {
        public override HitObject Parse(string text)
        {
            string[] split = text.Split(',');
            HitObjectType type = (HitObjectType)int.Parse(split[3]) & ~HitObjectType.ColourHax;
            bool combo = type.HasFlag(HitObjectType.NewCombo);
            type &= ~HitObjectType.NewCombo;

            var soundType = (LegacySoundType)int.Parse(split[4]);
            var bankInfo = new SampleBankInfo();
            List<SampleInfo> startSamples = null;

            HitObject result;

            if ((type & HitObjectType.Circle) > 0)
            {
                result = CreateHit(new Vector2(int.Parse(split[0]), int.Parse(split[1])), combo);

                if (split.Length > 5)
                    readCustomSampleBanks(split[5], bankInfo);
            }
            else if ((type & HitObjectType.Slider) > 0)
            {
                CurveType curveType = CurveType.Catmull;
                double length = 0;
                var points = new List<Vector2> { new Vector2(int.Parse(split[0]), int.Parse(split[1])) };

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
                    points.Add(new Vector2((int)Convert.ToDouble(temp[0], CultureInfo.InvariantCulture), (int)Convert.ToDouble(temp[1], CultureInfo.InvariantCulture)));
                }

                int repeatCount = Convert.ToInt32(split[6], CultureInfo.InvariantCulture);

                if (repeatCount > 9000)
                    throw new ArgumentOutOfRangeException(nameof(repeatCount), @"Repeat count is way too high");

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
                string[] adds = null;
                if (split.Length > 8 && split[8].Length > 0)
                {
                    adds = split[8].Split('|');
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
                for (int i = 0; i <= repeatCount; i++)
                    nodeSamples.Add(convertSoundType(nodeSoundTypes[i], nodeBankInfos[i]));

                // Extract the first node as the first sample
                startSamples = nodeSamples[0];

                // Repeat samples are all the samples excluding the one from the first node (note this includes the end node)
                var repeatSamples = nodeSamples.Skip(1).ToList();

                result = CreateSlider(new Vector2(int.Parse(split[0]), int.Parse(split[1])), combo, points, length, curveType, repeatCount, repeatSamples);
            }
            else if ((type & HitObjectType.Spinner) > 0)
            {
                result = CreateSpinner(new Vector2(512, 384) / 2, Convert.ToDouble(split[5], CultureInfo.InvariantCulture));

                if (split.Length > 6)
                    readCustomSampleBanks(split[6], bankInfo);
            }
            else if ((type & HitObjectType.Hold) > 0)
            {
                // Note: Hold is generated by BMS converts

                // Todo: Apparently end time is determined by samples??
                // Shouldn't need implementation until mania

                result = new Hold
                {
                    Position = new Vector2(int.Parse(split[0]), int.Parse(split[1])),
                    NewCombo = combo
                };
            }
            else
                throw new InvalidOperationException($@"Unknown hit object type {type}");

            result.StartTime = Convert.ToDouble(split[2], CultureInfo.InvariantCulture);
            result.Samples = startSamples ?? convertSoundType(soundType, bankInfo);

            return result;
        }

        private void readCustomSampleBanks(string str, SampleBankInfo bankInfo)
        {
            if (string.IsNullOrEmpty(str))
                return;

            string[] split = str.Split(':');

            var bank = (OsuLegacyDecoder.LegacySampleBank)Convert.ToInt32(split[0]);
            var addbank = (OsuLegacyDecoder.LegacySampleBank)Convert.ToInt32(split[1]);

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
        /// <param name="repeatSamples">The slider repeat sounds (this includes the end node, but NOT the start node).</param>
        /// <returns>The hit object.</returns>
        protected abstract HitObject CreateSlider(Vector2 position, bool newCombo, List<Vector2> controlPoints, double length, CurveType curveType, int repeatCount, List<List<SampleInfo>> repeatSamples);

        /// <summary>
        /// Creates a legacy Spinner-type hit object.
        /// </summary>
        /// <param name="position">The position of the hit object.</param>
        /// <param name="endTime">The spinner end time.</param>
        /// <returns>The hit object.</returns>
        protected abstract HitObject CreateSpinner(Vector2 position, double endTime);

        private List<SampleInfo> convertSoundType(LegacySoundType type, SampleBankInfo bankInfo)
        {
            var soundTypes = new List<SampleInfo>();

            if ((type & LegacySoundType.Normal) > 0)
            {
                soundTypes.Add(new SampleInfo
                {
                    Bank = bankInfo.Normal,
                    Name = SampleInfo.HIT_NORMAL,
                    Volume = bankInfo.Volume
                });
            }
            
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
            public string Normal = null;
            public string Add = null;
            public int Volume = 0;

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
