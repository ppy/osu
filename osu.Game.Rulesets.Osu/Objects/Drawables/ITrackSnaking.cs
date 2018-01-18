using OpenTK;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    /// <summary>
    /// A component which tracks the current end snaking position of a slider.
    /// </summary>
    public interface ITrackSnaking
    {
        void UpdateSnakingPosition(Vector2 start, Vector2 end);
    }
}
