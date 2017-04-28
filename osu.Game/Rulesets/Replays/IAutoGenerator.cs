using osu.Game.Rulesets.Replays;

namespace osu.Game
{
    public interface IAutoGenerator
    {
        Replay Generate();
    }
}
