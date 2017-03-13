using System.Collections.Generic;
using OpenTK;

namespace osu.Game.Modes.Objects.Types
{
    /// <summary>
    /// A HitObject that has a curve.
    /// </summary>
    public interface IHasCurve
    {
        /// <summary>
        /// The control points that shape the curve.
        /// </summary>
        List<Vector2> ControlPoints { get; }

        /// <summary>
        /// The type of curve.
        /// </summary>
        CurveType CurveType { get; }
    }
}
