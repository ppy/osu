// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Card
{
    public static class RankedPlayCardExtensions
    {
        /// <summary>
        /// Adjusts the transforms of a drawable relative to a parent drawable to match the given drawQuad.
        /// </summary>
        /// <param name="target">the target drawable.</param>
        /// <param name="drawQuad">screen space drawQuad to fit the drawable to.</param>
        /// <param name="parent">drawable to calculate the transforms in relation to.</param>
        public static T MatchScreenSpaceDrawQuad<T>(this T target, Quad drawQuad, CompositeDrawable parent) where T : Drawable
        {
            drawQuad = parent.ToLocalSpace(drawQuad);

            var originPosition = target.RelativeOriginPosition;

            // child may not have been made alive yet by the parent so anchor is calculated manually
            var anchorPosition = parent.ChildSize * target.RelativeAnchorPosition;

            var positionWithOrigin = Vector2.Lerp(
                Vector2.Lerp(drawQuad.TopLeft, drawQuad.TopRight, originPosition.X),
                Vector2.Lerp(drawQuad.BottomLeft, drawQuad.BottomRight, originPosition.X),
                originPosition.Y
            );

            target.Position = positionWithOrigin - anchorPosition;

            target.Rotation = MathHelper.RadiansToDegrees(new Line(drawQuad.TopLeft, drawQuad.TopRight).Theta);

            target.Scale = new Vector2(Vector2.Distance(drawQuad.TopLeft, drawQuad.TopRight) / target.DrawWidth);

            return target;
        }
    }
}
