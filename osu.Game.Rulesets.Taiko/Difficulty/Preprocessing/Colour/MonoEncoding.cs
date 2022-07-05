using osu.Game.Rulesets.Difficulty.Preprocessing;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Colour
{
    public class MonoEncoding
    {
        public List<TaikoDifficultyHitObject> EncodedData { get; private set; } = new List<TaikoDifficultyHitObject>();

        public int RunLength => EncodedData.Count;

        public static List<MonoEncoding> Encode(List<DifficultyHitObject> data)
        {
            List<MonoEncoding> encoded = new List<MonoEncoding>();

            MonoEncoding? lastEncoded = null;
            for (int i = 0; i < data.Count; i++)
            {
                TaikoDifficultyHitObject taikoObject = (TaikoDifficultyHitObject)data[i];
                // This ignores all non-note objects, which may or may not be the desired behaviour
                TaikoDifficultyHitObject previousObject = (TaikoDifficultyHitObject)taikoObject.PreviousNote(0);

                if (
                    previousObject == null ||
                    lastEncoded == null ||
                    taikoObject.HitType != previousObject.HitType)
                {
                    lastEncoded = new MonoEncoding();
                    lastEncoded.EncodedData.Add(taikoObject);
                    encoded.Add(lastEncoded);
                    continue;
                }

                lastEncoded.EncodedData.Add(taikoObject);
            }

            return encoded;
        }
    }
}