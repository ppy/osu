// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Edit.Commands;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Commands;
using osu.Game.Screens.Edit.Commands.Proxies;
using osuTK;

namespace osu.Game.Rulesets.Objects
{
    public static class SliderPathExtensions
    {
        /// <summary>
        /// Snaps the provided <paramref name="hitObject"/>'s duration using the <paramref name="snapProvider"/>.
        /// </summary>
        public static void SnapTo<THitObject>(this THitObject hitObject, IDistanceSnapProvider? snapProvider, EditorCommandHandler? commandHandler = null)
            where THitObject : HitObject, IHasPath
        {
            double distance = snapProvider?.FindSnappedDistance(hitObject, (float)hitObject.Path.CalculatedDistance, DistanceSnapTarget.Start) ?? hitObject.Path.CalculatedDistance;

            commandHandler.SafeSubmit(new SetExpectedDistanceCommand(hitObject.Path, distance));
        }

        /// <summary>
        /// Snaps the provided <paramref name="proxy"/>'s duration using the <paramref name="snapProvider"/>.
        /// </summary>
        public static void SnapTo<THitObject>(this CommandProxy<THitObject> proxy, IDistanceSnapProvider? snapProvider)
            where THitObject : HitObject, IHasPath
        {
            var hitObject = proxy.Target;
            double distance = snapProvider?.FindSnappedDistance(hitObject, (float)hitObject.Path.CalculatedDistance, DistanceSnapTarget.Start) ?? hitObject.Path.CalculatedDistance;

            proxy.Path().SetExpectedDistance(distance);
            proxy.Path().ControlPoints();
        }

        /// <summary>
        /// Reverse the direction of this path.
        /// </summary>
        /// <param name="sliderPath">The <see cref="SliderPath"/>.</param>
        /// <param name="positionalOffset">The positional offset of the resulting path. It should be added to the start position of this path.</param>
        public static void Reverse(this SliderPath sliderPath, out Vector2 positionalOffset)
        {
            sliderPath.AsCommandProxy(null).Reverse(out positionalOffset);
        }

        /// <summary>
        /// Reverses the order of the provided <see cref="SliderPath"/>'s <see cref="PathControlPoint"/>s.
        /// </summary>
        /// <param name="sliderPath">The <see cref="SliderPath"/>.</param>
        /// <param name="positionalOffset">The positional offset of the resulting path. It should be added to the start position of this path.</param>
        private static void reverseControlPoints(this SliderPath sliderPath, out Vector2 positionalOffset)
        {
            var points = sliderPath.ControlPoints.ToArray();
            positionalOffset = sliderPath.PositionAt(1);

            sliderPath.ControlPoints.Clear();

            PathType? lastType = null;

            for (int i = 0; i < points.Length; i++)
            {
                var p = points[i];
                p.Position -= positionalOffset;

                // propagate types forwards to last null type
                if (i == points.Length - 1)
                {
                    p.Type = lastType;
                    p.Position = Vector2.Zero;
                }
                else if (p.Type != null)
                    (p.Type, lastType) = (lastType, p.Type);

                sliderPath.ControlPoints.Insert(0, p);
            }
        }
    }
}
