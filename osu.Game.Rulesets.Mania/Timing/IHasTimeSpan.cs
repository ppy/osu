using OpenTK;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Rulesets.Mania.Timing
{
    public interface IHasTimeSpan : IContainer
    {
        /// <summary>
        /// The amount of time which this container spans. Drawables can be relatively positioned to this value.
        /// </summary>
        Vector2 TimeSpan { get; }
    }
}