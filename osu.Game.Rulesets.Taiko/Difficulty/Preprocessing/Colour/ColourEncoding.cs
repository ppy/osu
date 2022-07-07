using System.Collections.Generic;
using osu.Game.Rulesets.Difficulty.Preprocessing;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Colour
{
    public class ColourEncoding
    {
        public List<MonoEncoding> Payload { get; private set; } = new List<MonoEncoding>();

        public bool isRepetitionOf(ColourEncoding other)
        {
            return hasIdenticalMonoLength(other) &&
                other.Payload.Count == Payload.Count &&
                other.Payload[0].EncodedData[0].HitType == Payload[0].EncodedData[0].HitType;
        }

        public bool hasIdenticalMonoLength(ColourEncoding other)
        {
            return other.Payload[0].RunLength == Payload[0].RunLength;
        }

        public static List<ColourEncoding> Encode(List<MonoEncoding> data)
        {
            // Compute encoding lengths
            List<ColourEncoding> encoded = new List<ColourEncoding>();
            ColourEncoding? lastEncoded = null;
            for (int i = 0; i < data.Count; i++)
            {
                if (i == 0 || lastEncoded == null || data[i].RunLength != data[i - 1].RunLength)
                {
                    lastEncoded = new ColourEncoding();
                    lastEncoded.Payload.Add(data[i]);
                    encoded.Add(lastEncoded);
                    continue;
                }

                lastEncoded.Payload.Add(data[i]);
            }

            return encoded;
        }
    }
}