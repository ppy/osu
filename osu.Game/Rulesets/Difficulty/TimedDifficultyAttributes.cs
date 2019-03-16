namespace osu.Game.Rulesets.Difficulty
{
    public class TimedDifficultyAttributes
    {
        public readonly double Time;
        public readonly DifficultyAttributes Attributes;

        public TimedDifficultyAttributes(double time, DifficultyAttributes attributes)
        {
            this.Time = time;
            this.Attributes = attributes;
        }
    }
}
