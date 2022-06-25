using System.Collections.Generic;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing
{
    public class CoupledColourEncoding
    {
        public int RunLength = 1;

        public ColourEncoding[] Payload;

        /// <summary>
        /// Beginning index in the data that this encodes
        /// </summary>
        public int StartIndex { get; private set; } = 0;

        public int EndIndex { get; private set; } = 0;

        private CoupledColourEncoding(ColourEncoding[] payload)
        {
            Payload = payload;
        }

        public static List<CoupledColourEncoding> Encode(List<ColourEncoding> data)
        {
            List<CoupledColourEncoding> encoded = new List<CoupledColourEncoding>();

            CoupledColourEncoding? lastEncoded = null;
            for (int i = 0; i < data.Count; i++)
            {
                if (lastEncoded != null) lastEncoded.EndIndex = data[i].StartIndex - 1;

                if (i >= data.Count - 2 || !data[i].isIdenticalTo(data[i + 2]))
                {
                    lastEncoded = new CoupledColourEncoding(new ColourEncoding[] { data[i] });
                    lastEncoded.StartIndex = data[i].StartIndex;
                }
                else
                {
                    lastEncoded = new CoupledColourEncoding(new ColourEncoding[] { data[i], data[i + 1] });
                    lastEncoded.StartIndex = data[i].StartIndex;
                    lastEncoded.RunLength = 3;
                    i++;

                    // Peek 2 indices ahead
                    while (i < data.Count - 2 && data[i].isIdenticalTo(data[i + 2]))
                    {
                        lastEncoded.RunLength += 1;
                        i++;
                    }

                    // Skip over peeked data
                    i++;
                }

                encoded.Add(lastEncoded);
            }

            return encoded;
        }

        public bool hasIdenticalPayload(CoupledColourEncoding other)
        {
            if (this.Payload.Length != other.Payload.Length) return false;

            for (int i = 0; i < this.Payload.Length; i++)
            {
                if (!this.Payload[i].isIdenticalTo(other.Payload[i])) return false;
            }

            return true;
        }
    }
}