namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Colour
{
    /// <summary>
    /// Stores colour compression information for a <see cref="TaikoDifficultyHitObject"/>. This is only present for the
    /// first <see cref="TaikoDifficultyHitObject"/> in a <see cref="CoupledColourEncoding"/> chunk.
    /// </summary>
    public class TaikoDifficultyHitObjectColour
    {
        public CoupledColourEncoding Encoding { get; private set; }

        public double EvaluatedDifficulty = 0;

        public TaikoDifficultyHitObjectColour(CoupledColourEncoding encoding)
        {
            Encoding = encoding;
        }
    }
}