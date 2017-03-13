using OpenTK;

namespace osu.Game.Modes.Objects.Types
{
    /// <summary>
    /// A HitObject that has a starting position.
    /// </summary>
    public interface IHasPosition
    {
        /// <summary>
        /// The starting position of the HitObject.
        /// </summary>
        Vector2 Position { get; }
    }
}
